using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Narrative;
using LucillesJourney.UI;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// The Chapter 6 "second chance" decision point. Plays optional framing narrative, shows the
    /// three options via ChoiceUI, then plays the chosen consequence before completing its objective.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ChoiceInteractable : MonoBehaviour
    {
        [SerializeField, TextArea(2, 4)] private string prompt = "What does Lucille do with what little money she has?";
        [SerializeField] private NarrativeSequenceSO framingNarrative;
        [SerializeField] private ChoiceOption[] options = new ChoiceOption[0];

        private Interactable interactable;

        /// <summary>Used by the editor scene builder to assign the choice content at build time.</summary>
        public void Configure(string promptText, NarrativeSequenceSO framing, ChoiceOption[] choiceOptions)
        {
            prompt = promptText;
            framingNarrative = framing;
            options = choiceOptions;
        }

        private void Awake() => interactable = GetComponent<Interactable>();
        private void OnEnable() => interactable.Interacted += HandleInteracted;
        private void OnDisable() => interactable.Interacted -= HandleInteracted;

        private void HandleInteracted(Interactable source)
        {
            if (NarrativePanelUI.Instance != null)
                NarrativePanelUI.Instance.Play(framingNarrative, ShowChoice);
            else
                ShowChoice();
        }

        private void ShowChoice()
        {
            if (ChoiceUI.Instance != null)
                ChoiceUI.Instance.Show(prompt, options, OnOptionChosen);
            else
                Debug.LogError("ChoiceInteractable: no ChoiceUI found in scene.", this);
        }

        private void OnOptionChosen(ChoiceOption chosen)
        {
            GameManager.Instance.SetFinalChoice(chosen.id, chosen.label);
            if (NarrativePanelUI.Instance != null)
                NarrativePanelUI.Instance.Play(chosen.consequence, OnConsequenceComplete);
            else
                OnConsequenceComplete();
        }

        private void OnConsequenceComplete()
        {
            string id = interactable.InteractionId;
            if (string.IsNullOrEmpty(id)) return;
            GameManager.Instance.CompleteObjective(id);
        }
    }
}
