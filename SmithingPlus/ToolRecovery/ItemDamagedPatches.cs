using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using SmithingPlus.Common.Metal;
using SmithingPlus.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace SmithingPlus.ToolRecovery;

#nullable enable

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyPatch(typeof(CollectibleObject))]
[HarmonyPatchCategory(Core.ToolRecoveryCategory)]
public class ItemDamagedPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CollectibleObject.OnCreatedByCrafting))]
    [HarmonyPriority(-int.MaxValue)]
    public static void Postfix_OnCreatedByCrafting(
        ItemSlot[] allInputSlots,
        ItemSlot outputSlot,
        IRecipeBase byRecipe)
    {
        ItemStack? outputStack = outputSlot?.Itemstack;
        if (outputStack == null || allInputSlots == null || byRecipe == null) return;
        ItemStack? brokenStack = allInputSlots.FirstOrDefault(slot =>
            slot.Itemstack?.GetBrokenCount() > 0 &&
            slot.Itemstack?.Collectible.HasBehavior<CollectibleBehaviorRepairableToolHead>() == true
        )?.Itemstack;
        if (brokenStack == null) return;
        var brokenCount = brokenStack.GetBrokenCount();
        if (brokenCount <= 0) return;
        if (brokenStack.Item?.IsRepairableTool() is not true) return;
        var repairedStack = brokenStack.GetRepairedToolStack();
        if (repairedStack == null) return;
        ICoreAPI? coreApi = allInputSlots.FirstOrDefault()?.Inventory?.Api ?? Core.Api;
        if (coreApi == null) return;
        repairedStack.ResolveBlockOrItem(coreApi.World);
        if (repairedStack.Collectible?.Code != byRecipe.RecipeOutput?.ResolvedItemStack?.Collectible?.Code) return;
        foreach (var attributeKey in Core.Config.GetToolRepairForgettableAttributes)
            repairedStack.Attributes?.RemoveAttribute(attributeKey);
        var repairSmith = brokenStack.GetRepairSmith();
        if (repairSmith != null) repairedStack.SetRepairSmith(repairSmith);
        var smithingQuality = brokenStack.GetSmithingQuality();
        if (smithingQuality != 0) repairedStack.SetSmithingQuality(smithingQuality);
        var toolRepairPenaltyModifier = brokenStack.GetToolRepairPenaltyModifier();
        if (toolRepairPenaltyModifier != 0)
            repairedStack.SetToolRepairPenaltyModifier(toolRepairPenaltyModifier);
        var repairedAttributes = repairedStack.Attributes ?? new TreeAttribute();
        ITreeAttribute outputAttributes = outputStack.Attributes ?? new TreeAttribute();
        outputStack.Attributes = outputAttributes;
        foreach (var attribute in repairedAttributes)
            outputAttributes[attribute.Key] = attribute.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CollectibleObject.DamageItem))]
    private static void Prefix_DamageItem(
        IWorldAccessor? world,
        Entity? byEntity,
        ItemSlot? itemSlot,
        int amount = 1,
        bool destroyOnZeroDurability = true)
    {
        if (world == null || byEntity == null || itemSlot == null)
        {
            return;
        }

        if (world.Api.Side.IsClient())
            return;
        if (!destroyOnZeroDurability)
            return;
        ItemStack? itemStack = itemSlot.Itemstack;
        int? durability = itemStack?.GetRemainingDurability();
        if (!durability.HasValue || durability > amount) return;
        if (itemStack?.Collectible?.HasBehavior<CollectibleBehaviorRepairableTool>() != true) return;
        Core.Logger.VerboseDebug("Broken tool in InventoryID: {0}, Entity: {1}", itemSlot.Inventory?.InventoryID,
            byEntity.GetName());
        var entityPlayer = byEntity as EntityPlayer;
        string? toolCode = itemStack.Collectible.Code?.ToString();
        if (toolCode == null) return;
        SmithingRecipe? smithingRecipe;
        if (!Core.ToolToRecipeCache.TryGetValue(toolCode, out smithingRecipe))
        {
            smithingRecipe = GetHeadSmithingRecipe(world.Api, itemStack);
            if (smithingRecipe != null)
            {
                Core.ToolToRecipeCache[toolCode] = smithingRecipe;
            }
        }
        if (smithingRecipe == null)
        {
            Core.Logger.VerboseDebug("Head or tool smithing recipe not found for: {0}", toolCode);
            return;
        }

        var metalMaterial = itemStack.GetOrCacheMetalMaterial(byEntity.Api);
        var workItem = metalMaterial?.WorkItem;
        if (workItem is null)
        {
            Core.Logger.VerboseDebug(
                $"Work item not found. Metal material: {metalMaterial?.IngotCode}, " +
                $"collectible: {itemStack?.Collectible.Code}");
            return;
        }

        Core.Logger.VerboseDebug("Found work item: {0}", workItem.Code);
        var wItemStack = new ItemStack(workItem);
        ItemStack? recipeOutputStack = smithingRecipe.Output?.ResolvedItemstack;
        if (recipeOutputStack?.Collectible?.Code == null)
        {
            Core.Logger.VerboseDebug("The smithing recipe has no resolved output for: {0}", toolCode);
            return;
        }

        Core.Logger.VerboseDebug("Found smithing recipe: {0}", recipeOutputStack.Collectible.Code);
        byte[,,] byteVoxels = ByteVoxelsFromRecipe(smithingRecipe, recipeOutputStack.StackSize);
        wItemStack.Attributes.SetBytes("voxels", BlockEntityAnvil.serializeVoxels(byteVoxels));
        wItemStack.Attributes.SetInt("selectedRecipeId", smithingRecipe.RecipeId);
        ItemStack? cloneStack = itemStack.Clone();
        if (cloneStack == null) return;
        cloneStack.CloneBrokenCount(itemStack, 1);
        wItemStack.SetRepairedToolStack(cloneStack);

        var gaveStack = false;
        if (entityPlayer != null) gaveStack = entityPlayer.TryGiveItemStack(wItemStack);
        if (!gaveStack) world.SpawnItemEntity(wItemStack, byEntity.Pos.XYZ);
        Core.Logger.VerboseDebug(gaveStack ? "Gave work item {0} to player {1}" : "Dropped work item {0} to player {1}",
            wItemStack.Collectible.Code, entityPlayer?.Player.PlayerName);
        itemSlot.MarkDirty();
    }

    private static SmithingRecipe? GetHeadSmithingRecipe(ICoreAPI api, ItemStack itemStack)
    {
        ItemStack toolHead = GetToolHead(api, itemStack);
        return toolHead.GetSmithingRecipe(api);
    }

    private static ItemStack GetToolHead(ICoreAPI api, ItemStack itemStack)
    {
        var toolRecipe = itemStack
            .GetGridRecipes(api)
            .FirstOrDefault(r =>
                r.Output?.ResolvedItemStack?.StackSize == 1);
        var toolHead = toolRecipe?.RecipeIngredients
            .FirstOrDefault(k =>
                k?.ResolvedItemStack?.Collectible?.HasBehavior<CollectibleBehaviorRepairableToolHead>() ?? false)
            ?.ResolvedItemStack;
        if (toolHead == null)
        {
            toolHead = itemStack;
            Core.Logger.VerboseDebug("Tool head not found for: {0}", itemStack);
        }

        Core.Logger.VerboseDebug("Tool head: {0}", toolHead);
        return toolHead;
    }

    private static byte[,,] ByteVoxelsFromRecipe(SmithingRecipe recipe, int stackSize = 1)
    {
        var recipeVoxels = recipe.Voxels;
        if (Core.Config.BrokenToolVoxelPercent < 0.2)
            Core.Logger.Warning(
                $"[ItemDamagedPatches#ByteVoxelsFromRecipe] Config setting {nameof(Core.Config.BrokenToolVoxelPercent)}" +
                $"has a very low value, your broken tools well be almost or fully empty.");
        ;
        var byteVoxels = recipeVoxels.ErodeToPercentage(Core.Config.BrokenToolVoxelPercent);
        return byteVoxels;
    }
}
