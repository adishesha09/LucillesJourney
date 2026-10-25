using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Entrance animation for the menu and ending interfaces: a fade in from black, then the title and each control
    /// fading and settling into place one after another. Reduced Motion shows everything at once. Nothing here can
    /// block input: groups stay interactable, so a click during the animation still works.
    /// </summary>
    public class MenuEntrance : MonoBehaviour
    {
        private sealed class Item { public CanvasGroup Group; public RectTransform Rect; public float Delay; }

        private readonly List<Item> items = new List<Item>();
        private Image fade;
        private float start;
        private const float FadeSeconds = 1.4f, ItemSeconds = 0.55f;

        /// <summary>Creates the entrance driver on its own overlay canvas (above the scene interface, below JourneyUI).</summary>
        public static MenuEntrance Create()
        {
            var go = new GameObject("MenuEntrance", typeof(Canvas), typeof(CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            go.AddComponent<NoInterfaceScale>();

            var overlay = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(go.transform, false);
            var rt = (RectTransform)overlay.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = overlay.GetComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;

            var entrance = go.AddComponent<MenuEntrance>();
            entrance.fade = img;
            entrance.start = Time.unscaledTime;
            return entrance;
        }

        public void Add(Component target, float delay)
        {
            if (target == null) return;
            var group = target.GetComponent<CanvasGroup>();
            if (group == null) group = target.gameObject.AddComponent<CanvasGroup>();
            items.Add(new Item { Group = group, Rect = (RectTransform)target.transform, Delay = delay });
            if (!SettingsManager.ReducedMotion) group.alpha = 0f;
        }

        private void Update()
        {
            if (SettingsManager.ReducedMotion)
            {
                foreach (var item in items) Apply(item, 1f);
                fade.color = Color.clear;
                Destroy(gameObject);
                return;
            }

            float t = Time.unscaledTime - start;
            var c = fade.color;
            c.a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / FadeSeconds));
            fade.color = c;

            bool done = t > FadeSeconds;
            foreach (var item in items)
            {
                float k = Mathf.Clamp01((t - 0.5f - item.Delay) / ItemSeconds);
                Apply(item, Mathf.SmoothStep(0f, 1f, k));
                if (k < 1f) done = false;
            }
            if (done) Destroy(gameObject);
        }

        private static void Apply(Item item, float k)
        {
            if (item.Group == null) return;
            item.Group.alpha = k;
            // A tiny settle: controls grow from 94% to full size. Layout groups ignore scale, so nothing shifts.
            item.Rect.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k);
        }
    }

    /// <summary>A gentle vertical bob for title text, in whole canvas steps so the pixel font stays crisp.</summary>
    public class TitleBob : MonoBehaviour
    {
        private RectTransform rt;
        private Vector2 basePosition;

        private void Awake()
        {
            rt = (RectTransform)transform;
            basePosition = rt.anchoredPosition;
        }

        private void Update()
        {
            float offset = SettingsManager.ReducedMotion ? 0f : Mathf.Round(Mathf.Sin(Time.unscaledTime * 0.9f) * 3f) * 2f;
            rt.anchoredPosition = basePosition + new Vector2(0f, offset);
        }
    }

    /// <summary>Shared decoration for the menu-style scenes: a title drop shadow and an ornament divider.</summary>
    public static class MenuDecor
    {
        /// <summary>Duplicates a title label behind itself as a dark offset shadow, and returns the shadow's CanvasGroup owner.</summary>
        public static TMP_Text AddShadow(TMP_Text title, Vector2 offset, Color color)
        {
            if (title == null) return null;
            var copy = UnityEngine.Object.Instantiate(title.gameObject, title.transform.parent);
            copy.name = title.name + "Shadow";
            var text = copy.GetComponent<TMP_Text>();
            text.color = color;
            var rt = (RectTransform)copy.transform;
            rt.anchoredPosition += offset;
            copy.transform.SetSiblingIndex(title.transform.GetSiblingIndex());
            return text;
        }

        public static Image AddOrnament(Transform parent, Vector2 anchoredPosition, float scale)
        {
            var sprite = Resources.Load<Sprite>("Menu/menu_title_ornament");
            if (sprite == null) return null;
            var go = new GameObject("TitleOrnament", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sprite.rect.size * scale;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }
    }
}
