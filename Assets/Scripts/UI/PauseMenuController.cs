using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Gamification.UI;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Scene-facing pause menu handle. The pause menu itself (Escape / Start, journal sections, settings,
    /// controls, main menu with confirmation) now lives in JourneyUI so every gameplay scene shares one
    /// implementation. This component keeps the original references and Wire() signature, hides the old
    /// scene-built panel, and exposes Open() for any caller that wants to pause.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button guideButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private SettingsMenuController settingsMenu;
        [SerializeField] private InteractionGuideUI interactionGuide;

        private void Awake() => HideLegacyPanel();

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(GameObject root, Button resume, Button settings, Button guide, Button mainMenu, Button quit,
            SettingsMenuController settingsPanel, InteractionGuideUI guidePanel)
        {
            panelRoot = root;
            resumeButton = resume;
            settingsButton = settings;
            guideButton = guide;
            mainMenuButton = mainMenu;
            quitButton = quit;
            settingsMenu = settingsPanel;
            interactionGuide = guidePanel;
            HideLegacyPanel();
        }

        private void HideLegacyPanel()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Open() => JourneyUI.Instance?.OpenPause();
    }
}
