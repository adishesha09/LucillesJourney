using System;
using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Gamification;
using LucillesJourney.Narrative;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// Bridges the existing Interactable component to the narrative and objective systems.
    /// Plays a NarrativeSequenceSO when interacted with, then (once the player has finished
    /// reading it) marks the Interactable's interactionId as a completed objective and,
    /// optionally, as a discovered memory fragment.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class NarrativeInteractable : MonoBehaviour
    {
        [Tooltip("Story beat shown when the player interacts. Can be left empty for a silent objective.")]
        [SerializeField] private NarrativeSequenceSO narrativeSequence;

        [Tooltip("If true, this interaction is also recorded as a discovered memory fragment.")]
        [SerializeField] private bool isMemoryFragment = false;

        /// <summary>Raised after the narrative finishes and the objective is recorded, with the objectiveId.</summary>
        public static event Action<string> ObjectiveCompleted;

        /// <summary>Lets other interactable types (e.g. ChoiceInteractable) report into the same objective pipeline.</summary>
        public static void RaiseObjectiveCompleted(string objectiveId) => ObjectiveCompleted?.Invoke(objectiveId);

        private Interactable interactable;

        /// <summary>Used by the editor scene builder to assign the sequence/memory flag at build time.</summary>
        public void Configure(NarrativeSequenceSO sequence, bool memoryFragment = false)
        {
            narrativeSequence = sequence;
            isMemoryFragment = memoryFragment;
        }

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void OnEnable() => interactable.Interacted += HandleInteracted;

        private void OnDisable() => interactable.Interacted -= HandleInteracted;

        private void HandleInteracted(Interactable source)
        {
            if (NarrativePanelUI.Instance != null)
                NarrativePanelUI.Instance.Play(narrativeSequence, OnNarrativeComplete);
            else
                OnNarrativeComplete();
        }

        private void OnNarrativeComplete()
        {
            string id = interactable.InteractionId;
            if (string.IsNullOrEmpty(id)) return;

            // One batch: a single save, one achievement evaluation and one ProgressChanged for
            // everything this interaction reveals (objective, memory, keepsake).
            GameManager.Instance.Batch(() =>
            {
                GameManager.Instance.CompleteObjective(id);
                if (isMemoryFragment || JourneyCatalog.TryGetMemory(id, out _)) GameManager.Instance.DiscoverMemory(id);
                if (JourneyCatalog.TryGetKeepsake(id, out _)) GameManager.Instance.CollectKeepsake(id);
            });
            ObjectiveCompleted?.Invoke(id);
        }
    }
}
