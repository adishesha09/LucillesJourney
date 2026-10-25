using UnityEngine;

namespace LucillesJourney.World
{
    /// <summary>Cycles a SpriteRenderer through a few sprites (idle loops, smoke, sway, seated children, a sleeping cat).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames = new Sprite[0];
        [SerializeField, Min(0.1f)] private float framesPerSecond = 2f;
        [SerializeField] private float phase;

        private SpriteRenderer spriteRenderer;

        public void Configure(Sprite[] sprites, float fps, float startPhase = 0f)
        {
            frames = sprites;
            framesPerSecond = Mathf.Max(0.1f, fps);
            phase = startPhase;
        }

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void Update()
        {
            if (frames == null || frames.Length < 2) return;
            // Unscaled so ambient life keeps going quietly while a menu is open.
            int index = Mathf.FloorToInt((Time.unscaledTime + phase) * framesPerSecond) % frames.Length;
            if (spriteRenderer.sprite != frames[index]) spriteRenderer.sprite = frames[index];
        }
    }
}
