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

    /// <summary>Keeps a moving character's sorting order in step with its feet position.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DepthSortedCharacter : MonoBehaviour
    {
        [SerializeField, Min(1)] private int pixelsPerUnit = 16;
        private SpriteRenderer spriteRenderer;

        public void Configure(int ppu) => pixelsPerUnit = Mathf.Max(1, ppu);

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void LateUpdate()
        {
            spriteRenderer.sortingOrder = WorldDepthSort.OrderForPlayer(transform.position.y * pixelsPerUnit);
        }
    }
}
