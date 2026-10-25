using UnityEngine;
using LucillesJourney.Core;

namespace LucillesJourney.World
{
    /// <summary>
    /// A small floating sparkle above something the player can still discover or do. It bobs gently, fades out once
    /// the matching objective is complete, and stays still when reduced motion is on.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ObjectiveMarker : MonoBehaviour
    {
        [SerializeField] private string objectiveId = "";

        private SpriteRenderer spriteRenderer;
        private Vector3 basePosition;
        private float alpha = 1f;

        public void Configure(string id) => objectiveId = id;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            basePosition = transform.localPosition;
        }

        private void OnEnable() => Refresh();

        private void Update()
        {
            bool done = GameManager.Instance.IsObjectiveComplete(objectiveId);
            alpha = Mathf.MoveTowards(alpha, done ? 0f : 1f, Time.unscaledDeltaTime * 3f);
            var c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
            spriteRenderer.enabled = alpha > 0.01f;

            float bob = SettingsManager.ReducedMotion ? 0f : Mathf.Round(Mathf.Sin(Time.unscaledTime * 3f) * 2f) / 16f; // whole-pixel steps
            transform.localPosition = basePosition + new Vector3(0f, bob, 0f);
        }

        private void Refresh()
        {
            if (spriteRenderer == null) return;
            alpha = GameManager.Instance.IsObjectiveComplete(objectiveId) ? 0f : 1f;
        }
    }
}
