using TMPro;
using UnityEngine;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the Escape-key pause menu for a gameplay scene.</summary>
    public static class PauseMenuBuilder
    {
        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.75f);
        private static readonly Color PanelColor = new Color32(0x3E, 0x52, 0x52, 255);
        private static readonly Color AccentColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);

        public static PauseMenuController Build(Transform parent, SettingsMenuController settingsMenu, InteractionGuideUI guide)
        {
            var overlay = SceneBuildUtility.CreatePanel(parent, "PauseMenuPanel", OverlayColor);
            SceneBuildUtility.Stretch(overlay);

            var box = SceneBuildUtility.CreatePanel(overlay.transform, "Box", PanelColor);
            SceneBuildUtility.Anchor(box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 440));
            SceneBuildUtility.AddVerticalLayout(box, spacing: 18, padding: new RectOffset(32, 32, 32, 32));

            SceneBuildUtility.CreateText(box.transform, "Title", "Paused", 38, TextColor, TextAlignmentOptions.Center);

            var (resumeGO, resumeBtn, _) = SceneBuildUtility.CreateButton(box.transform, "ResumeButton", "Resume", AccentColor, PanelColor, preferredHeight: 64f);
            var (settingsGO, settingsBtn, _) = SceneBuildUtility.CreateButton(box.transform, "SettingsButton", "Settings", PanelColor, TextColor, preferredHeight: 56f);
            var (guideGO, guideBtn, _) = SceneBuildUtility.CreateButton(box.transform, "GuideButton", "How To Play", PanelColor, TextColor, preferredHeight: 56f);
            var (mainMenuGO, mainMenuBtn, _) = SceneBuildUtility.CreateButton(box.transform, "MainMenuButton", "Main Menu", PanelColor, TextColor, preferredHeight: 56f);
            var (quitGO, quitBtn, _) = SceneBuildUtility.CreateButton(box.transform, "QuitButton", "Quit", PanelColor, TextColor, preferredHeight: 56f);

            // Must rebuild while still active - PauseMenuController.Wire() immediately hides the
            // panel, and Unity's layout system skips inactive hierarchies. Target "box" (the
            // actual layout group owner), not "overlay" - ForceRebuildLayoutImmediate only
            // recurses into children if the passed-in RectTransform itself has a layout
            // controller component.
            SceneBuildUtility.ForceRebuildLayout(box);

            var controllerGO = new GameObject("PauseMenuController");
            var controller = controllerGO.AddComponent<PauseMenuController>();
            controller.Wire(overlay, resumeBtn, settingsBtn, guideBtn, mainMenuBtn, quitBtn, settingsMenu, guide);
            return controller;
        }
    }
}
