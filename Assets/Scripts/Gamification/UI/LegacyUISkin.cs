using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LucillesJourney.UI;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// Re-skins the scene-built interface that predates the pixel-art design system (main menu, ending,
    /// narrative panel, interaction prompt) so the whole game shares one look: pixel fonts everywhere,
    /// pixel-art panels, buttons and sliders. It runs when each scene loads, only changes presentation
    /// (never behaviour), and leaves full-screen overlays and anything inside JourneyUI untouched.
    /// </summary>
    public static class LegacyUISkin
    {
        private static readonly HashSet<int> Processed = new HashSet<int>();

        public static void ApplyToLoadedScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++) Apply(SceneManager.GetSceneAt(i));
        }

        public static void Apply(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInParent<JourneyUI>() != null) continue;
                SkinHierarchy(root.transform);
            }
        }

        private static void SkinHierarchy(Transform root)
        {
            foreach (var slider in root.GetComponentsInChildren<Slider>(true)) SkinSlider(slider);
            foreach (var button in root.GetComponentsInChildren<Button>(true)) SkinButton(button);
            foreach (var image in root.GetComponentsInChildren<Image>(true)) SkinPanel(image);
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true)) SkinText(text);
        }

        private static bool FirstVisit(Object o) => Processed.Add(o.GetInstanceID());

        private static void SkinText(TMP_Text text)
        {
            if (!FirstVisit(text)) return;

            // Large display titles use the heading font (scaled down: it is much wider); everything else the reading font.
            bool display = text.fontSize >= 56f;
            text.font = display ? PixelTheme.HeadingFont : PixelTheme.BodyFont;
            float factor = display ? 0.8f : 1.35f;

            var scalable = text.GetComponent<ScalableText>();
            if (scalable != null) scalable.Rebase(factor);
            else text.fontSize *= factor;
        }

        private static void SkinButton(Button button)
        {
            if (!FirstVisit(button)) return;
            var image = button.targetGraphic as Image;
            if (image == null) return;

            bool primary = Vector4.Distance(image.color, PixelTheme.Orange) < 0.2f;
            UIFactory.ApplyFrame(image, primary ? "ui_button_hover" : "ui_button", Color.white);

            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = PixelTheme.Sprite(primary ? "ui_button_selected" : "ui_button_hover"),
                selectedSprite = PixelTheme.Sprite("ui_button_selected"),
                pressedSprite = PixelTheme.Sprite("ui_button_pressed"),
                disabledSprite = PixelTheme.Sprite("ui_button_disabled"),
            };

            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true)) label.color = PixelTheme.Cream;
        }

        private static void SkinPanel(Image image)
        {
            if (!FirstVisit(image)) return;
            if (image.GetComponentInParent<Button>() != null || image.GetComponentInParent<Slider>() != null) return;
            if (image.sprite != null && image.sprite.name != "UISprite" && image.sprite.name != "Background") return;
            if (image.color.a < 0.2f) return;

            var rt = image.rectTransform;
            bool fullScreen = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one;
            if (fullScreen) return; // overlays and fades stay flat

            UIFactory.ApplyFrame(image, "ui_panel", Color.white);

            // The dialogue box is a fixed 1500 wide; keep it on screen when the interface is zoomed in.
            if (image.GetComponent<LucillesJourney.Narrative.NarrativePanelUI>() != null && image.GetComponent<ClampSize>() == null)
            {
                var design = image.rectTransform.sizeDelta;
                var clamp = image.gameObject.AddComponent<ClampSize>();
                clamp.Design = design;
                clamp.ClampHeight = false;
            }
            // Keep text clear of the 9-slice border once the pixel frame is applied.
            var layout = image.GetComponent<LayoutGroup>();
            if (layout != null)
            {
                int pad = 9 * UIFactory.S;
                var p = layout.padding;
                layout.padding = new RectOffset(Mathf.Max(p.left, pad), Mathf.Max(p.right, pad), Mathf.Max(p.top, pad), Mathf.Max(p.bottom, pad));
            }
        }

        private static void SkinSlider(Slider slider)
        {
            if (!FirstVisit(slider)) return;

            var background = slider.transform.Find("Background")?.GetComponent<Image>();
            if (background != null) UIFactory.ApplyFrame(background, "ui_slider_bg", Color.white);

            if (slider.fillRect != null && slider.fillRect.TryGetComponent<Image>(out var fill))
            {
                fill.sprite = PixelTheme.Sprite("ui_bar_fill");
                fill.type = Image.Type.Simple;
                fill.color = PixelTheme.Gold;
            }

            if (slider.handleRect != null && slider.handleRect.TryGetComponent<Image>(out var handle))
            {
                var knob = PixelTheme.Sprite("ui_knob");
                if (knob != null)
                {
                    handle.sprite = knob;
                    handle.type = Image.Type.Simple;
                    handle.preserveAspect = true;
                    handle.color = Color.white;
                    slider.handleRect.sizeDelta = new Vector2(14f * UIFactory.S, 14f * UIFactory.S);
                }
            }
        }
    }
}
