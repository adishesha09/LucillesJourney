using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the Chapter 6 "impossible choice" panel: a prompt and up to three option buttons.</summary>
    public static class ChoiceUIBuilder
    {
        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.8f);
        private static readonly Color PanelColor = new Color32(0x3E, 0x52, 0x52, 255);
        private static readonly Color ButtonColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);

        public static ChoiceUI Build(Transform parent)
        {
            var overlay = SceneBuildUtility.CreatePanel(parent, "ChoicePanel", OverlayColor);
            SceneBuildUtility.Stretch(overlay);

            var box = SceneBuildUtility.CreatePanel(overlay.transform, "Box", PanelColor);
            SceneBuildUtility.Anchor(box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 520));
            SceneBuildUtility.AddVerticalLayout(box, spacing: 24, padding: new RectOffset(48, 48, 48, 48));

            var prompt = SceneBuildUtility.CreateText(box.transform, "Prompt", "", 30, TextColor, TextAlignmentOptions.Center);
            prompt.gameObject.AddComponent<LayoutElement>().preferredHeight = 120;

            var buttons = new List<Button>();
            var labels = new List<TMP_Text>();
            for (int i = 0; i < 3; i++)
            {
                var (go, btn, label) = SceneBuildUtility.CreateButton(
                    box.transform, $"Option{i + 1}", "", ButtonColor, PanelColor, 26, preferredHeight: 80f);
                buttons.Add(btn);
                labels.Add(label);
            }

            var controller = overlay.AddComponent<ChoiceUI>();
            controller.Wire(SceneBuildUtility.AddCanvasGroup(overlay), prompt, buttons, labels);
            SceneBuildUtility.ForceRebuildLayout(box);
            // Deliberately left active (hidden only via the CanvasGroup, same as NarrativePanelUI) -
            // ChoiceUI.Instance is assigned in Awake(), which Unity never calls for a GameObject
            // that starts inactive, so SetActive(false) here would leave Instance permanently null.
            return controller;
        }
    }
}
