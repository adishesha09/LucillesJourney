using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>Builds the embedded "How To Play" interaction guide panel, used in every scene that has one.</summary>
    public static class InteractionGuidePanelBuilder
    {
        private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.75f);
        private static readonly Color PanelColor = new Color32(0x3E, 0x52, 0x52, 255);
        private static readonly Color AccentColor = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);

        private const string GuideText =
            "MOVE          WASD or Arrow Keys / Left Stick\n" +
            "INTERACT      E or Space, when a prompt is on screen / Gamepad A\n" +
            "ADVANCE TEXT  Space or Enter while reading narrative\n" +
            "PAUSE         Esc\n\n" +
            "Walk up to glowing objects to see a prompt, then interact to reveal Lucille's story.\n" +
            "Golden sparks mark memory fragments - optional details that add depth to the story.\n" +
            "Complete every objective shown in the top-left HUD to move on to the next chapter.";

        public static InteractionGuideUI Build(Transform parent)
        {
            var overlay = SceneBuildUtility.CreatePanel(parent, "InteractionGuidePanel", OverlayColor);
            SceneBuildUtility.Stretch(overlay);

            var box = SceneBuildUtility.CreatePanel(overlay.transform, "Box", PanelColor);
            SceneBuildUtility.Anchor(box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 560));
            SceneBuildUtility.AddVerticalLayout(box, spacing: 24, padding: new RectOffset(40, 40, 40, 40));

            SceneBuildUtility.CreateText(box.transform, "Title", "How To Play", 40, TextColor, TextAlignmentOptions.Center);
            var body = SceneBuildUtility.CreateText(box.transform, "Body", GuideText, 26, TextColor, TextAlignmentOptions.TopLeft);
            body.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

            var (closeGO, closeBtn, _) = SceneBuildUtility.CreateButton(box.transform, "CloseButton", "Close", AccentColor, PanelColor, preferredHeight: 56f);

            var controller = overlay.AddComponent<InteractionGuideUI>();
            controller.Wire(overlay, closeBtn);
            SceneBuildUtility.ForceRebuildLayout(box);
            overlay.SetActive(false);
            return controller;
        }
    }
}
