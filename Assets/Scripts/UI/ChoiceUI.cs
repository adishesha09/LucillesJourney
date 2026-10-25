using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Interaction;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Presents Lucille's final, "impossible" choice as a set of buttons, one per ChoiceOption.
    /// Only one instance is expected per gameplay scene (Chapter 6).
    /// </summary>
    public class ChoiceUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private List<Button> optionButtons = new List<Button>();
        [SerializeField] private List<TMP_Text> optionLabels = new List<TMP_Text>();

        public static ChoiceUI Instance { get; private set; }

        /// <summary>True while the scene-built (legacy) choice panel is visible.</summary>
        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0.5f;

        private Action<ChoiceOption> onChosen;

        private void Awake()
        {
            Instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(CanvasGroup group, TMP_Text prompt, List<Button> buttons, List<TMP_Text> labels)
        {
            canvasGroup = group;
            promptLabel = prompt;
            optionButtons = buttons;
            optionLabels = labels;
            Hide();
        }

        public void Show(string prompt, IReadOnlyList<ChoiceOption> options, Action<ChoiceOption> onChosenCallback)
        {
            // The illustrated, confirm-before-commit choice screen replaces the scene-built panel.
            if (LucillesJourney.Gamification.UI.JourneyUI.Instance != null)
            {
                LucillesJourney.Gamification.UI.JourneyUI.Instance.ShowFinalChoice(prompt, options, onChosenCallback);
                return;
            }

            onChosen = onChosenCallback;
            if (promptLabel != null) promptLabel.text = prompt;

            for (int i = 0; i < optionButtons.Count; i++)
            {
                bool hasOption = i < options.Count;
                optionButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption) continue;

                var option = options[i];
                if (i < optionLabels.Count && optionLabels[i] != null) optionLabels[i].text = option.label;

                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => Choose(option));
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void Choose(ChoiceOption option)
        {
            Hide();
            var callback = onChosen;
            onChosen = null;
            callback?.Invoke(option);
        }

        private void Hide()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
