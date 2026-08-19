#nullable enable
using System.Text;
using SmithingPlus.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace SmithingPlus.ToolRecovery;

public class CollectibleBehaviorRepairableTool : CollectibleBehavior
{
    public CollectibleBehaviorRepairableTool(CollectibleObject collObj) : base(collObj)
    {
    }

    protected virtual string LangKey => "Repaired";

    public override void GetHeldItemInfo(ItemSlot? inSlot, StringBuilder dsc, IWorldAccessor world,
        bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        ItemStack? itemStack = inSlot?.Itemstack;
        CollectibleObject? collectible = itemStack?.Collectible;
        var code = collectible?.Code;

        if (code == null || itemStack == null)
        {
            Core.Logger.Error("Failed to get code for itemstack {0}", itemStack);
            return;
        }

        if (!WildcardUtil.Match(Core.Config.RepairableToolSelector, code.ToString()))
            return;
        var brokenCount = itemStack.GetBrokenCount();
        if (brokenCount <= 0) return;
        if (Core.CConfig.ShowRepairedCount) dsc.AppendLine(Lang.Get($"{LangKey} {{0}} times", brokenCount));
        if (Core.CConfig.ShowRepairSmithName && itemStack.GetRepairSmith() is { } repairSmith)
            dsc.AppendLine(Lang.Get("Last repaired by {0}", repairSmith));
    }
}
