using System.Text;
using SmithingPlus.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SmithingPlus.BitsRecovery;

#nullable enable

public class CollectibleBehaviorSmeltedContainer(CollectibleObject collObj) : CollectibleBehavior(collObj)
{
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        ItemStack? itemStack = inSlot?.Itemstack;
        if (itemStack == null)
        {
            return;
        }

        float temp = itemStack.GetTemperature(world);
        if (temp < CollectibleBehaviorScrapeCrucible.MaxScrapeTemperature)
            dsc.AppendLine(Lang.Get($"{Core.ModId}:heldhelp-scrapecrucible"));
    }
}
