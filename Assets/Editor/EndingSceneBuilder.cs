using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the closing scene shown after Chapter 6's decision: Ending.unity.</summary>
    public static class EndingSceneBuilder
    {
        private static readonly Color BackgroundColor = new Color32(0x1A, 0x22, 0x24, 255);
        private static readonly Color AccentColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color PanelColor = new Color32(0x3E, 0x52, 0x52, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);
        private static readonly Color PanelDark = new Color32(0x2B, 0x23, 0x21, 235);

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SceneBuildUtility.CreateCamera(BackgroundColor, 6f);
            SceneBuildUtility.CreateEventSystem();

            var canvas = SceneBuildUtility.CreateCanvas("EndingCanvas").gameObject;

            // Title banner across the top, the closing reflection on the left, the actions on the right: the middle stays
            // clear for the road and the walking family. Panels use proportional anchors so they hold at any text size.
            var titlePanel = SceneBuildUtility.CreatePanel(canvas.transform, "TitlePanel", PanelDark);
            SceneBuildUtility.Anchor(titlePanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(880, 120));
            var titleLayout = SceneBuildUtility.AddVerticalLayout(titlePanel, spacing: 0, padding: new RectOffset(30, 30, 18, 18));
            titleLayout.childAlignment = TextAnchor.MiddleCenter;
            var title = SceneBuildUtility.CreateText(titlePanel.transform, "Title", "A Second Chance", 56, AccentColor, TextAlignmentOptions.Center);
            title.enableAutoSizing = true;
            title.fontSizeMin = 24f;
            title.fontSizeMax = 56f;
            title.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1f;

            var messagePanel = SceneBuildUtility.CreatePanel(canvas.transform, "MessagePanel", PanelDark);
            // Sized to its text (vertical fit), centred on the left side, so there is never an empty gap inside it.
            SceneBuildUtility.Anchor(messagePanel, new Vector2(0.03f, 0.5f), new Vector2(0.41f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(0f, 300f));
            var messageRect = messagePanel.GetComponent<RectTransform>();
            messageRect.offsetMin = new Vector2(messageRect.offsetMin.x, messageRect.offsetMin.y);
            var messageLayout = SceneBuildUtility.AddVerticalLayout(messagePanel, spacing: 30, padding: new RectOffset(40, 40, 36, 36));
            messageLayout.childAlignment = TextAnchor.UpperLeft;
            var messageFitter = messagePanel.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            messageFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            var summary = SceneBuildUtility.CreateText(messagePanel.transform, "Summary", "", 34, AccentColor, TextAlignmentOptions.TopLeft);

            var reflection = SceneBuildUtility.CreateText(messagePanel.transform,
                "Reflection",
                "Every choice Lucille made in this room had a cost she could not avoid.\n" +
                "This is the reality for many displaced families - there is rarely a version of \"enough\".\n\n" +
                "Thank you for walking through part of Lucille's story.",
                28, TextColor, TextAlignmentOptions.TopLeft);

            var buttonColumn = SceneBuildUtility.CreatePanel(canvas.transform, "ButtonColumn", PanelDark);
            SceneBuildUtility.Anchor(buttonColumn, new Vector2(0.69f, 0.1f), new Vector2(0.97f, 0.1f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0, 100));
            buttonColumn.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            buttonColumn.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            buttonColumn.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            SceneBuildUtility.AddVerticalLayout(buttonColumn, spacing: 14, padding: new RectOffset(24, 24, 24, 24));
            var columnFitter = buttonColumn.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            columnFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            var (mainMenuGO, mainMenuBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "MainMenuButton", "Main Menu", AccentColor, PanelColor, preferredHeight: 60f);
            var (quitGO, quitBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "QuitButton", "Quit", PanelColor, TextColor, preferredHeight: 54f);

            var controller = canvas.AddComponent<EndingUI>();
            controller.Wire(summary, mainMenuBtn, quitBtn);

            SceneBuildUtility.ForceRebuildLayout(buttonColumn);
            SceneBuildUtility.ForceRebuildLayout(messagePanel);
            SceneBuildUtility.ForceRebuildLayout(titlePanel);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Ending.unity");
        }
    }
}
