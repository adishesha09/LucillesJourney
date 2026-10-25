using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Gamification.UI;

namespace LucillesJourney.UI
{
    /// <summary>
    /// The closing screen after Chapter 6. Shows which impossible choice the player made and a
    /// closing reflection, then returns to the main menu (or quits, where supported).
    /// </summary>
    public class EndingUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text choiceSummaryLabel;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(TMP_Text summaryLabel, Button mainMenu, Button quit)
        {
            choiceSummaryLabel = summaryLabel;
            mainMenuButton = mainMenu;
            quitButton = quit;
        }

        private void Start()
        {
            if (choiceSummaryLabel != null)
            {
                string label = GameManager.Instance.FinalChoiceLabel;
                choiceSummaryLabel.text = string.IsNullOrEmpty(label)
                    ? "Lucille's journey continues, however she chose to spend what little she had."
                    : $"Lucille chose to: {label}";
            }

            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());
            AddJourneyButton();
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() => GameManager.Instance.QuitGame());
                quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            }
            Decorate();
        }

        // The dawn road backdrop, then the panels and buttons settling in one after another.
        private void Decorate()
        {
            MenuBackdrop.Create(BackdropKind.Ending);
            var entrance = MenuEntrance.Create();
            entrance.Add(transform.Find("TitlePanel"), 0.2f);
            entrance.Add(transform.Find("MessagePanel"), 0.7f);
            entrance.Add(transform.Find("ButtonColumn"), 1.2f);
        }

        // Lets the player look back over everything they found: chapters, memories, keepsakes and achievements.
        private void AddJourneyButton()
        {
            if (JourneyUI.Instance == null || quitButton == null) return;

            var clone = Instantiate(quitButton.gameObject, quitButton.transform.parent);
            clone.name = "JourneyButton";
            var button = clone.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => JourneyUI.Instance.OpenHub(HubTab.Journey));
            var label = clone.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "Journey Overview";
            clone.transform.SetSiblingIndex(quitButton.transform.GetSiblingIndex());
        }
    }
}
