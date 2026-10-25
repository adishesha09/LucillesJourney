using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Gamification;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// Locks an Interactable until the objectives listed for it in JourneyCatalog.RequiresIds are
    /// complete (for example, departing needs the bag packed). Unlocks live when the last
    /// prerequisite completes. Looks the rules up by interaction id, so no per-object setup is needed.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class InteractionPrerequisite : MonoBehaviour
    {
        private static readonly Color LockedTint = new Color(1f, 1f, 1f, 0.35f);

        private Interactable interactable;
        private SpriteRenderer spriteRenderer;
        private ObjectiveInfo info;
        private Color originalColor = Color.white;
        private bool locked;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        private void OnEnable() => JourneyEvents.ObjectiveCompleted += HandleObjectiveCompleted;
        private void OnDisable() => JourneyEvents.ObjectiveCompleted -= HandleObjectiveCompleted;

        private void Start()
        {
            JourneyCatalog.TryGetObjective(interactable.InteractionId, out info);
            Refresh();
        }

        private void HandleObjectiveCompleted(string _) => Refresh();

        private void Refresh()
        {
            if (info == null || info.RequiresIds.Length == 0) return;

            bool met = true;
            foreach (var required in info.RequiresIds)
                if (!GameManager.Instance.IsObjectiveComplete(required)) { met = false; break; }

            bool shouldLock = !met && !GameManager.Instance.IsObjectiveComplete(info.Id);
            locked = shouldLock;

            interactable.SetInteractionEnabled(!locked);
            if (spriteRenderer != null && !GameManager.Instance.IsObjectiveComplete(info.Id))
                spriteRenderer.color = locked ? originalColor * LockedTint : originalColor;
        }
    }
}
