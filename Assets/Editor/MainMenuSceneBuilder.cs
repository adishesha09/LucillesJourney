using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using LucillesJourney.Core;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the title screen scene: MainMenu.unity.</summary>
    public static class MainMenuSceneBuilder
    {
        private static readonly Color BackgroundColor = new Color32(0x2B, 0x3A, 0x3F, 255);
        private static readonly Color PanelColor = new Color32(0x3E, 0x52, 0x52, 230);
        private static readonly Color AccentColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);

        public static void Build(ChapterId[] chapterIds)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Resolved fresh after NewScene - see the comment in ChapterSceneBuilder.Build.
            var chapters = chapterIds
                .Select(id => AssetDatabase.LoadAssetAtPath<ChapterDataSO>($"Assets/Data/Chapters/Chapter_{id}.asset"))
                .Where(c => c != null)
                .ToArray();

            SceneBuildUtility.CreateCamera(BackgroundColor, 6f);
            SceneBuildUtility.CreateEventSystem();

            var canvas = SceneBuildUtility.CreateCanvas("MainMenuCanvas");
            var canvasRoot = canvas.gameObject;

            // Title block.
            var titleGroup = new GameObject("TitleBlock", typeof(RectTransform));
            SceneBuildUtility.SetParent(titleGroup, canvasRoot.transform);
            SceneBuildUtility.Anchor(titleGroup, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(1200, 180));

            SceneBuildUtility.CreateText(titleGroup.transform, "Title", "Lucille's Journey", 72, TextColor,
                TextAlignmentOptions.Center);
            var subtitle = SceneBuildUtility.CreateText(titleGroup.transform, "Subtitle",
                "A story of ordinary life, displacement, and an impossible choice.", 28, TextColor * new Color(1, 1, 1, 0.85f),
                TextAlignmentOptions.Center);
            SceneBuildUtility.Anchor(subtitle.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0, 0), new Vector2(1000, 60));
            SceneBuildUtility.Anchor(titleGroup.transform.GetChild(0).gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(1200, 100));

            // Button column.
            var buttonColumn = new GameObject("ButtonColumn", typeof(RectTransform));
            SceneBuildUtility.SetParent(buttonColumn, canvasRoot.transform);
            // Anchored under the title (top-down) rather than from the screen centre, so the title, subtitle and the
            // six buttons never collide when the interface is zoomed in and the canvas gets smaller.
            SceneBuildUtility.Anchor(buttonColumn, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0, -270), new Vector2(460, 320));
            SceneBuildUtility.AddVerticalLayout(buttonColumn, spacing: 14, controlHeight: true);
            var fitter = buttonColumn.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            var (beginGO, beginBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "BeginButton", "Begin", AccentColor, BackgroundColor, preferredHeight: 64f);
            var (guideGO, guideBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "GuideButton", "How To Play", PanelColor, TextColor, preferredHeight: 54f);
            var (settingsGO, settingsBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "SettingsButton", "Settings", PanelColor, TextColor, preferredHeight: 54f);
            var (quitGO, quitBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "QuitButton", "Quit", PanelColor, TextColor, preferredHeight: 54f);

            // Footer credit.
            var footer = SceneBuildUtility.CreateText(canvasRoot.transform, "Footer",
                "Built for GADS6222 in collaboration with UXTE7312, for The Refugee Archive.", 18,
                TextColor * new Color(1, 1, 1, 0.6f), TextAlignmentOptions.Center);
            SceneBuildUtility.Anchor(footer.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0, 24), new Vector2(1200, 40));

            var settingsMenu = SettingsPanelBuilder.Build(canvasRoot.transform);
            var guidePanel = InteractionGuidePanelBuilder.Build(canvasRoot.transform);

            var controller = canvasRoot.AddComponent<MainMenuController>();
            controller.Wire(beginBtn, guideBtn, settingsBtn, quitBtn, settingsMenu, guidePanel, chapters);

            // Target buttonColumn (the actual layout group owner), not canvasRoot -
            // ForceRebuildLayoutImmediate only recurses into children if the passed-in
            // RectTransform itself has a layout controller component.
            SceneBuildUtility.ForceRebuildLayout(buttonColumn);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }
    }
}
