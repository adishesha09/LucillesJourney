using UnityEngine;

namespace LucillesJourney.World
{
    /// <summary>Keeps a moving character's sorting order in step with its feet position (see <see cref="WorldDepthSort"/>).</summary>
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
