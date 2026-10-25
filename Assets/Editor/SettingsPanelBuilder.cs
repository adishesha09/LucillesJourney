using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the shared Settings panel (master volume + text size) used in every scene that has one.</summary>
    public static class SettingsPanelBuilder
    {
        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.75f);
        private static readonly Color PanelColor = new Color32(0x3E, 0x52, 0x52, 255);
        private static readonly Color AccentColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);

        public static SettingsMenuController Build(Transform parent)
        {
            var overlay = SceneBuildUtility.CreatePanel(parent, "SettingsPanel", OverlayColor);
            SceneBuildUtility.Stretch(overlay);

            var box = SceneBuildUtility.CreatePanel(overlay.transform, "Box", PanelColor);
            SceneBuildUtility.Anchor(box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 440));
            var layout = SceneBuildUtility.AddVerticalLayout(box, spacing: 24, padding: new RectOffset(40, 40, 40, 40));
            layout.childAlignment = TextAnchor.UpperCenter;

            SceneBuildUtility.CreateText(box.transform, "Title", "Settings", 40, TextColor, TextAlignmentOptions.Center);

            var (volumeSlider, volumeLabel) = CreateRow(box.transform, "Master Volume");
            var (textScaleSlider, textScaleLabel) = CreateRow(box.transform, "Text Size");

            var (closeGO, closeBtn, _) = SceneBuildUtility.CreateButton(box.transform, "CloseButton", "Close", AccentColor, PanelColor, preferredHeight: 56f);

            var controller = overlay.AddComponent<SettingsMenuController>();
            controller.Wire(overlay, volumeSlider, volumeLabel, textScaleSlider, textScaleLabel, closeBtn);
            SceneBuildUtility.ForceRebuildLayout(box);
            overlay.SetActive(false);
            return controller;
        }

        private static (Slider slider, TMP_Text valueLabel) CreateRow(Transform parent, string title)
        {
            var row = new GameObject(title.Replace(" ", "") + "Row", typeof(RectTransform));
            SceneBuildUtility.SetParent(row, parent);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60;
            SceneBuildUtility.AddHorizontalLayout(row, spacing: 16);

            SceneBuildUtility.CreateText(row.transform, "Label", title, 26, TextColor, TextAlignmentOptions.Left)
                .gameObject.AddComponent<LayoutElement>().preferredWidth = 260;

            var slider = SceneBuildUtility.CreateSlider(row.transform, "Slider");
            slider.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var valueLabel = SceneBuildUtility.CreateText(row.transform, "Value", "100%", 24, TextColor, TextAlignmentOptions.Right);
            valueLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 100;

            return (slider, valueLabel);
        }
    }
}
