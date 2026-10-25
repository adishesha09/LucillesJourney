using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Gamification.UI;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Title screen. Continue resumes from the player's furthest unlocked chapter (it reads "Begin Journey" until
    /// there is a save), and New Journey starts over after a confirmation.
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
            if (JourneyUI.Instance != null && guideButton != null)
            {
                // Extra entries cloned from an existing button so they share its layout and skin.
                journeyButton = AddButton("Journey Overview", () => JourneyUI.Instance.OpenHub(HubTab.Journey));
                newJourneyButton = AddButton("New Journey", ConfirmNewGame);
            }
            ArrangeButtons();
            Decorate();
        }

        private Button journeyButton, newJourneyButton;

        private bool HasProgress =>
            GameManager.Instance.HighestUnlockedChapterIndex > 0 || GameManager.Instance.IsObjectiveComplete("ch1_garden")
            || GameManager.Instance.IsObjectiveComplete("ch1_breakfast") || GameManager.Instance.PlayTimeSeconds > 1f;

        /// <summary>
        /// Orders the menu from "play" to "leave": Continue and New Journey first (carry on or start over), then the
        /// archive of what you have found, then settings and help, and finally Quit. New Journey only appears when there
        /// is something to start over from, and the main button reads "Begin Journey" until the first save exists.
        /// </summary>
        private void ArrangeButtons()
        {
            bool hasProgress = HasProgress;
            SetLabel(beginButton, hasProgress ? "Continue" : "Begin Journey");
            if (newJourneyButton != null) newJourneyButton.gameObject.SetActive(hasProgress);
            if (journeyButton != null) journeyButton.gameObject.SetActive(hasProgress);

            var order = new Button[] { beginButton, newJourneyButton, journeyButton, settingsButton, guideButton, quitButton };
            int index = 0;
            foreach (var button in order)
            {
                if (button == null) continue;
                button.transform.SetSiblingIndex(index++);
            }
            SetLabel(guideButton, "How To Play");
        }

        private static void SetLabel(Button button, string text)
        {
            if (button == null) return;
            var label = button.GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null) label.text = text;
        }

        // Animated backdrop, title shadow and ornament, and the staggered entrance.
        private void Decorate()
        {
            MenuBackdrop.Create(BackdropKind.Menu);

            var entrance = MenuEntrance.Create();
            var titleBlock = transform.Find("TitleBlock");
            if (titleBlock != null)
            {
                var title = titleBlock.Find("Title")?.GetComponent<TMPro.TMP_Text>();
                var subtitle = titleBlock.Find("Subtitle")?.GetComponent<TMPro.TMP_Text>();
                if (title != null)
                {
                    var shadow = MenuDecor.AddShadow(title, new Vector2(8f, -8f), new Color(0.10f, 0.07f, 0.06f, 0.9f));
                    if (shadow != null) { shadow.gameObject.AddComponent<TitleBob>(); entrance.Add(shadow, 0.05f); }
                    title.gameObject.AddComponent<TitleBob>();
                    entrance.Add(title, 0f);
                }
                var ornament = MenuDecor.AddOrnament(titleBlock, new Vector2(0f, -112f), 5f);
                if (ornament != null) entrance.Add(ornament, 0.15f);
                if (subtitle != null) entrance.Add(subtitle, 0.25f);
            }

            var footer = transform.Find("Footer")?.GetComponent<TMPro.TMP_Text>();
            if (footer != null)
            {
                var footerShadow = MenuDecor.AddShadow(footer, new Vector2(3f, -3f), new Color(0.07f, 0.05f, 0.04f, 1f));
                footer.fontStyle = TMPro.FontStyles.Bold;
                entrance.Add(footer, 1.6f);
                if (footerShadow != null) entrance.Add(footerShadow, 1.6f);
            }

            float delay = 0.35f;
            foreach (var button in transform.GetComponentsInChildren<Button>(false))
            {
                if (button.transform.parent.name != "ButtonColumn") continue;
                entrance.Add(button, delay);
                delay += 0.09f;
            }
        }

        private Button AddButton(string label, UnityEngine.Events.UnityAction action)
        {
            var clone = Instantiate(guideButton.gameObject, guideButton.transform.parent);
            clone.name = label.Replace(" ", "") + "Button";
            var button = clone.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            var text = clone.GetComponentInChildren<TMPro.TMP_Text>();
            if (text != null) text.text = label;
            return button;
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
