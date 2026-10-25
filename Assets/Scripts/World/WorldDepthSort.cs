using UnityEngine;

namespace LucillesJourney.World
{
    /// <summary>
    /// Shared draw-order rules for the overworld. Sprites have a bottom-centre pivot, so the transform's y is the base
    /// of the object: whatever is lower on screen is drawn in front. Lucille re-evaluates this every frame, so she walks
    /// behind trees and tall furniture and in front of anything she stands below.
    /// </summary>
    public static class WorldDepthSort
    {
        public const int GroundOrder = -30000, FloorOrder = -20000, AboveOrder = 20000, MarkerOrder = 25000;
        public const int OrderPerPixel = 4, YOrderRange = 15000;
        // Wins ties with props whose base is at exactly her feet, but never beats a prop that is a whole pixel lower.
        public const int PlayerBias = 2;

        public static int OrderForBaseline(float baselineYPx) =>
            -Mathf.Clamp(Mathf.RoundToInt(baselineYPx * OrderPerPixel), 0, YOrderRange);

        public static int OrderForPlayer(float feetYPx) => OrderForBaseline(feetYPx) + PlayerBias;
    }
}
