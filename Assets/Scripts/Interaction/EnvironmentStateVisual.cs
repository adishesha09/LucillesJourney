using System.Collections;
using UnityEngine;
using LucillesJourney.Core;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// Reusable "visible consequence" for an interaction. When the trigger objective completes it
    /// records a persistent environment-state flag in the save file and applies a visual change
    /// (swap sprite, tint, show/hide other objects). On scene load the saved flag is read back and
    /// the changed state is restored instantly, so the world stays consistent across scene changes
    /// and restarts. Configure in the Inspector or via Configure() from the editor scene builder.
    /// </summary>
    public class EnvironmentStateVisual : MonoBehaviour
    {
        [Tooltip("Key stored in the save file, e.g. ch1_garden_watered. Must be unique across the game.")]
        [SerializeField] private string stateKey = "";

        [Tooltip("Objective whose completion triggers this change. Leave empty to use the Interactable on this object.")]
        [SerializeField] private string triggerObjectiveId = "";

        [Header("Visual change")]
        [SerializeField] private Sprite completedSprite;
        [SerializeField] private bool changeColor = false;
        [SerializeField] private Color completedColor = Color.white;
        [SerializeField] private GameObject[] activateOnComplete = new GameObject[0];
        [SerializeField] private GameObject[] deactivateOnComplete = new GameObject[0];
        [SerializeField] private bool hideSelfOnComplete = false;

        private SpriteRenderer spriteRenderer;
        private Interactable interactable;

        public string StateKey => stateKey;

        /// <summary>Hide this whole object once the state is applied (for example an item that has been packed away).</summary>
        public void ConfigureHideSelf() => hideSelfOnComplete = true;

        /// <summary>Used by the editor scene builder.</summary>
        public void Configure(string key, string objectiveId, Sprite sprite, bool tint, Color color,
            GameObject[] activate = null, GameObject[] deactivate = null)
        {
            stateKey = key;
            triggerObjectiveId = objectiveId;
            completedSprite = sprite;
            changeColor = tint;
            completedColor = color;
            activateOnComplete = activate ?? new GameObject[0];
            deactivateOnComplete = deactivate ?? new GameObject[0];
        }

        private string TriggerId
        {
            get
            {
                if (!string.IsNullOrEmpty(triggerObjectiveId)) return triggerObjectiveId;
                return interactable != null ? interactable.InteractionId : "";
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            interactable = GetComponent<Interactable>();
        }

        private void OnEnable() => JourneyEvents.ObjectiveCompleted += HandleObjectiveCompleted;
        private void OnDisable() => JourneyEvents.ObjectiveCompleted -= HandleObjectiveCompleted;

        private void Start()
        {
            if (string.IsNullOrEmpty(stateKey)) return;
            var game = GameManager.Instance;

            // Saves made before this flag existed still show the consequence once the objective is done.
            if (!game.IsEnvironmentState(stateKey) && game.IsObjectiveComplete(TriggerId))
                game.SetEnvironmentState(stateKey, true);

            if (game.IsEnvironmentState(stateKey)) Apply(false);
        }

        private void HandleObjectiveCompleted(string objectiveId)
        {
            if (string.IsNullOrEmpty(stateKey) || objectiveId != TriggerId) return;
            GameManager.Instance.SetEnvironmentState(stateKey, true);
            Apply(true);
        }

        private void Apply(bool animate)
        {
            if (spriteRenderer != null)
            {
                if (completedSprite != null) spriteRenderer.sprite = completedSprite;
                if (changeColor) spriteRenderer.color = completedColor;
            }
            foreach (var go in activateOnComplete) if (go != null) go.SetActive(true);
            foreach (var go in deactivateOnComplete) if (go != null) go.SetActive(false);

            if (hideSelfOnComplete)
            {
                gameObject.SetActive(false);
                return;
            }

            if (animate && !SettingsManager.ReducedMotion && isActiveAndEnabled) StartCoroutine(Pop());
        }

        private IEnumerator Pop()
        {
            Vector3 baseScale = transform.localScale;
            const float duration = 0.25f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.Sin(t / duration * Mathf.PI);
                transform.localScale = baseScale * (1f + 0.2f * k);
                yield return null;
            }
            transform.localScale = baseScale;
        }
    }
}
