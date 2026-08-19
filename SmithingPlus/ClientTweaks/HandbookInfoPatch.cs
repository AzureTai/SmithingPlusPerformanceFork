using System.Collections.Generic;
using Cairo;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace SmithingPlus.ClientTweaks;

#nullable enable

[HarmonyPatchCategory(Core.ClientTweaksCategories.HandbookExtraInfo)]
public partial class HandbookInfoPatch
{
    public static ItemStack[] StacksFromCode(ICoreClientAPI capi, ItemStack moldStack,
        out List<string> existingMetalVariants)
    {
        existingMetalVariants = new List<string>();
        var stacks = new List<ItemStack>();
        SurvivalCoreSystem? survivalCoreSystem = capi.ModLoader.GetModSystem<SurvivalCoreSystem>();
        if (survivalCoreSystem?.metalsByCode == null)
        {
            return stacks.ToArray();
        }

        foreach (string metalVariant in survivalCoreSystem.metalsByCode.Keys)
        {
            var stack = GetStackForVariant(capi, moldStack, metalVariant);
            if (stack == null) continue;
            stacks.Add(stack);
            existingMetalVariants.Add(metalVariant);
        }

        return stacks.ToArray();
    }

    private static ItemStack? GetStackForVariant(ICoreClientAPI capi, ItemStack moldStack, string metalVariant)
    {
        CollectibleObject? mold = moldStack?.Collectible;
        if (mold?.Code == null)
        {
            return null;
        }

        JsonObject? dropAttribute = mold.Attributes?["drop"];
        JsonItemStack? jstack = dropAttribute?.AsObject<JsonItemStack>(null, mold.Code.Domain)?.Clone();
        if (jstack == null) return null;
        if (jstack.Code == null) return null;
        string toolVariant = mold.LastCodePart();
        jstack.Code.Path = jstack.Code.Path.Replace("{tooltype}", toolVariant).Replace("{metal}", metalVariant);
        jstack.Resolve(capi.World, "tool mold drop for " + mold.Code, false);
        return jstack.ResolvedItemstack;
    }

    public static string ToolMoldType(CollectibleObject mold)
    {
        var dropAttr = mold.Attributes?["drop"];
        if (dropAttr == null) return mold.LastCodePart();

        var jstack = dropAttr.AsObject<JsonItemStack>(null, mold.Code.Domain);
        if (jstack?.Code == null) return mold.LastCodePart();

        return jstack.Code.Path.Contains("{tooltype}") ? mold.LastCodePart() : jstack.Code.FirstCodePart();
    }

    public static void AddHeading(
        List<RichTextComponentBase> components,
        ICoreClientAPI capi,
        string heading,
        ref bool haveText)
    {
        if (haveText)
            components.Add(new ClearFloatTextComponent(capi, 14f));
        haveText = true;
        var richTextComponent = new RichTextComponent(capi, Lang.Get(heading) + "\n",
            CairoFont.WhiteSmallText().WithWeight(FontWeight.Bold));
        components.Add(richTextComponent);
    }

    public static void AddSubHeading(
        List<RichTextComponentBase> components,
        ICoreClientAPI capi,
        ActionConsumable<string> openDetailPageFor,
        string subheading,
        string detailpage)
    {
        if (detailpage == null)
        {
            var richTextComponent =
                new RichTextComponent(capi, "• " + Lang.Get(subheading) + "\n", CairoFont.WhiteSmallText())
                {
                    PaddingLeft = 2.0
                };
            components.Add(richTextComponent);
        }
        else
        {
            var richTextComponent = new RichTextComponent(capi, "• ", CairoFont.WhiteSmallText())
            {
                PaddingLeft = 2.0
            };
            components.Add(richTextComponent);
            components.Add(new LinkTextComponent(capi, Lang.Get(subheading) + "\n", CairoFont.WhiteSmallText(),
                cs => _ = openDetailPageFor(detailpage) ? 1 : 0));
        }
    }
}
