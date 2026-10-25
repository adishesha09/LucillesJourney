using TMPro;
using UnityEngine;
using LucillesJourney.Narrative;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the bottom-of-screen narrative/dialogue box used in every chapter scene.</summary>
    public static class NarrativePanelBuilder
    {
        private static readonly Color PanelColor = new Color32(0x1E, 0x29, 0x2B, 235);
        private static readonly Color AccentColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);
        private static readonly Color SpeakerColor = new Color32(0xD9, 0x8F, 0x4B, 255);

        public static NarrativePanelUI Build(Transform parent)
        {
            var root = SceneBuildUtility.CreatePanel(parent, "NarrativePanel", PanelColor);
            SceneBuildUtility.Anchor(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(1500, 260));
            var canvasGroup = SceneBuildUtility.AddCanvasGroup(root);

            var speaker = SceneBuildUtility.CreateText(root.transform, "Speaker", "Speaker", 26, SpeakerColor, TextAlignmentOptions.TopLeft);
            SceneBuildUtility.Anchor(speaker.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 1f), new Vector2(40, -24), new Vector2(-80, 40));

            var body = SceneBuildUtility.CreateText(root.transform, "Body", "...", 30, TextColor, TextAlignmentOptions.TopLeft);
            SceneBuildUtility.Anchor(body.gameObject, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(0f, 0.5f), new Vector2(40, -16), new Vector2(-80, -110));

            var (continueGO, continueBtn, continueLabel) = SceneBuildUtility.CreateButton(
                root.transform, "ContinueButton", "Continue", AccentColor, PanelColor, 24);
            SceneBuildUtility.Anchor(continueGO, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-40, 24), new Vector2(220, 56));

            var controller = root.AddComponent<NarrativePanelUI>();
            controller.Wire(canvasGroup, speaker, body, continueBtn, continueLabel);
            return controller;
        }
    }
}
