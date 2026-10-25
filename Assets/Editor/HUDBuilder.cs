using TMPro;
using UnityEngine;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the top-left HUD: chapter title and live objective checklist.</summary>
    public static class HUDBuilder
    {
        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.35f);
        private static readonly Color TitleColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);

        public static HUDController Build(Transform parent)
        {
            var root = SceneBuildUtility.CreatePanel(parent, "HUD", PanelColor);
            SceneBuildUtility.Anchor(root, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(24, -24), new Vector2(560, 220));
            SceneBuildUtility.AddVerticalLayout(root, spacing: 8, padding: new RectOffset(20, 20, 16, 16));

            var title = SceneBuildUtility.CreateText(root.transform, "ChapterTitle", "Chapter", 28, TitleColor, TextAlignmentOptions.TopLeft);
            var objectives = SceneBuildUtility.CreateText(root.transform, "Objectives", "", 22, TextColor, TextAlignmentOptions.TopLeft);

            var controller = root.AddComponent<HUDController>();
            controller.Wire(title, objectives);
            SceneBuildUtility.ForceRebuildLayout(root);
            return controller;
        }
    }
}
