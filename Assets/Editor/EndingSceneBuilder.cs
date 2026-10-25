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

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SceneBuildUtility.CreateCamera(BackgroundColor, 6f);
            SceneBuildUtility.CreateEventSystem();

            var canvas = SceneBuildUtility.CreateCanvas("EndingCanvas").gameObject;

            // One top-to-bottom flow (title, decision, reflection, then the buttons) so nothing overlaps when the
            // interface is zoomed in and the canvas shrinks.
            var flow = new GameObject("Flow", typeof(RectTransform));
            SceneBuildUtility.SetParent(flow, canvas.transform);
            SceneBuildUtility.Stretch(flow);
            var flowLayout = SceneBuildUtility.AddVerticalLayout(flow, spacing: 24, padding: new RectOffset(80, 80, 60, 50));
            flowLayout.childForceExpandWidth = false;
            flowLayout.childAlignment = TextAnchor.UpperCenter;

            var title = SceneBuildUtility.CreateText(flow.transform, "Title", "A Second Chance", 56, AccentColor, TextAlignmentOptions.Center);
            title.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1f;

            var summary = SceneBuildUtility.CreateText(flow.transform, "Summary", "", 30, TextColor, TextAlignmentOptions.Center);
            summary.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1f;

            var reflection = SceneBuildUtility.CreateText(flow.transform,
                "Reflection",
                "Every choice Lucille made in this room had a cost she could not avoid.\n" +
                "This is the reality for many displaced families - there is rarely a version of \"enough\".\n\n" +
                "Thank you for walking through part of Lucille's story.",
                24, TextColor * new Color(1, 1, 1, 0.85f), TextAlignmentOptions.Center);
            reflection.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth = 1f;

            var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(UnityEngine.UI.LayoutElement));
            SceneBuildUtility.SetParent(spacer, flow.transform);
            spacer.GetComponent<UnityEngine.UI.LayoutElement>().flexibleHeight = 1f;

            var buttonColumn = new GameObject("ButtonColumn", typeof(RectTransform));
            SceneBuildUtility.SetParent(buttonColumn, flow.transform);
            var columnElement = buttonColumn.AddComponent<UnityEngine.UI.LayoutElement>();
            columnElement.preferredWidth = 360f;
            SceneBuildUtility.AddVerticalLayout(buttonColumn, spacing: 14, padding: new RectOffset(0, 0, 0, 0));

            var (mainMenuGO, mainMenuBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "MainMenuButton", "Main Menu", AccentColor, PanelColor, preferredHeight: 60f);
            var (quitGO, quitBtn, _) = SceneBuildUtility.CreateButton(buttonColumn.transform, "QuitButton", "Quit", PanelColor, TextColor, preferredHeight: 54f);

            var controller = canvas.AddComponent<EndingUI>();
            controller.Wire(summary, mainMenuBtn, quitBtn);

            SceneBuildUtility.ForceRebuildLayout(flow);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Ending.unity");
        }
    }
}
