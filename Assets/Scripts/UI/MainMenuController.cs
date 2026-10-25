using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Gamification.UI;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Title screen. Begin resumes from the player's furthest unlocked chapter (or Chapter 1 for
    /// a fresh save), so there's no separate "Continue" button to keep but no progress is lost either.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button beginButton;
        [SerializeField] private Button guideButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private SettingsMenuController settingsMenu;
        [SerializeField] private InteractionGuideUI interactionGuide;
        [SerializeField] private ChapterDataSO[] chapters = new ChapterDataSO[0];

        private void Awake() => WireListeners();

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(Button begin, Button guide, Button settings, Button quit,
            SettingsMenuController settingsPanel, InteractionGuideUI guidePanel, ChapterDataSO[] chapterList)
        {
            beginButton = begin;
            guideButton = guide;
            settingsButton = settings;
            quitButton = quit;
            settingsMenu = settingsPanel;
            interactionGuide = guidePanel;
            chapters = chapterList;
            WireListeners();
        }

        private void WireListeners()
        {
            if (beginButton != null) beginButton.onClick.AddListener(Begin);
            if (guideButton != null) guideButton.onClick.AddListener(OpenControls);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() => GameManager.Instance.QuitGame());
                quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            }
        }

        // The pixel-art settings and controls screens (inside JourneyUI) replace the scene-built panels.
        private void OpenControls()
        {
            if (JourneyUI.Instance != null) JourneyUI.Instance.OpenControls();
            else interactionGuide?.Show();
        }

        private void OpenSettings()
        {
            if (JourneyUI.Instance != null) JourneyUI.Instance.OpenSettings();
            else settingsMenu?.Show();
        }

        private void Start()
        {
            if (JourneyUI.Instance == null || guideButton == null) return;

            // Extra entries cloned from an existing button so they share its layout and skin.
            AddButton("Journey Overview", () => JourneyUI.Instance.OpenHub(HubTab.Journey), beginButton);
            AddButton("New Journey", ConfirmNewGame, quitButton);
        }

        private void AddButton(string label, UnityEngine.Events.UnityAction action, Button placeAfter)
        {
            var clone = Instantiate(guideButton.gameObject, guideButton.transform.parent);
            clone.name = label.Replace(" ", "") + "Button";
            var button = clone.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            var text = clone.GetComponentInChildren<TMPro.TMP_Text>();
            if (text != null) text.text = label;
            if (placeAfter != null) clone.transform.SetSiblingIndex(placeAfter.transform.GetSiblingIndex() + (placeAfter == beginButton ? 1 : 0));
        }

        private void ConfirmNewGame()
        {
            JourneyUI.Instance.Confirm("Start a new journey?",
                "This clears your saved progress, memories and achievements and begins again at Chapter 1. Settings are kept.",
                "Start over", () =>
                {
                    GameManager.Instance.StartNewGame();
                    Begin();
                });
        }

        private void Begin()
        {
            int targetIndex = GameManager.Instance.HighestUnlockedChapterIndex;
            var chapter = chapters.FirstOrDefault(c => c != null && (int)c.chapterId == targetIndex)
                          ?? chapters.FirstOrDefault(c => c != null);

            if (chapter == null || string.IsNullOrEmpty(chapter.sceneName))
            {
                Debug.LogError("MainMenuController: no chapter scene configured to begin with.", this);
                return;
            }
            GameManager.Instance.LoadScene(chapter.sceneName);
        }
    }
}
