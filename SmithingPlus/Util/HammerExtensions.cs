using System;
using Vintagestory.API.Common;

namespace SmithingPlus.Util;

internal static class HammerExtensions
{
    public enum HammerToolMode
    {
        HeavyHit = 0,
        UpsetNorth = 1,
        UpsetEast = 2,
        UpsetSouth = 3,
        UpsetWest = 4,
        Split = 5
    }

    public static HammerToolMode GetHammerToolMode(this ItemSlot hotbarSlot, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (hotbarSlot == null)
        {
            throw new ArgumentNullException(nameof(hotbarSlot));
        }

        if (hotbarSlot.Itemstack?.Collectible == null)
        {
            throw new InvalidOperationException("A hammer tool mode cannot be read from an empty slot.");
        }

        return (HammerToolMode)hotbarSlot.Itemstack.Collectible.GetToolMode(hotbarSlot, byPlayer, blockSel);
    }
}
