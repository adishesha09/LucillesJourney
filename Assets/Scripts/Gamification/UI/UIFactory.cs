using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LucillesJourney.UI;

namespace LucillesJourney.Gamification.UI
{
    public enum TextStyle { Title, Heading, Label, Button, Body, BodySmall, Meta }
    public enum ButtonKind { Primary, Secondary, Tab, TabActive, Card }

    /// <summary>
    /// Builds the pixel-art uGUI controls shared by every screen. All sizes are authored in source
    /// pixels and multiplied by PixelTheme.PixelScale so sprites stay on a whole-pixel grid.
    /// If a sprite is missing the control falls back to a flat theme colour instead of failing.
    /// </summary>
    public static class UIFactory
    {
        public const int S = PixelTheme.PixelScale;

        // Type scale (font size at the 1920x1080 reference). Headings use Press Start 2P, reading text VT323.
        public static float SizeOf(TextStyle style)
        {
            switch (style)
            {
                case TextStyle.Title: return 48f;
                case TextStyle.Heading: return 32f;
                case TextStyle.Label: return 24f;
                case TextStyle.Button: return 24f;
                case TextStyle.Body: return 44f;
                case TextStyle.BodySmall: return 38f;
                default: return 32f; // Meta
            }
        }

        public static bool UsesHeadingFont(TextStyle style) =>
            style == TextStyle.Title || style == TextStyle.Heading || style == TextStyle.Label || style == TextStyle.Button;

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        private static GridLayoutGroup GetOrAdd<T>(GameObject go, bool responsive) where T : GridLayoutGroup
        {
            var existing = go.GetComponent<GridLayoutGroup>();
            if (existing != null) return existing;
            return responsive ? (GridLayoutGroup)go.AddComponent<ResponsiveGrid>() : go.AddComponent<GridLayoutGroup>();
        }

        // --- Layout primitives -------------------------------------------------------------------

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt, float padding = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        public static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        /// <summary>
        /// Sets explicit layout sizes. A dimension given a fixed size becomes non-flexible unless a flexible
        /// weight is also passed, otherwise it would silently inherit "flexible" from its children.
        /// </summary>
        public static LayoutElement Size(GameObject go, float width = -1f, float height = -1f, float flexWidth = -1f, float flexHeight = -1f,
            float minWidth = -1f, float minHeight = -1f)
        {
            var le = GetOrAdd<LayoutElement>(go);
            if (width >= 0f) { le.preferredWidth = width; le.flexibleWidth = flexWidth >= 0f ? flexWidth : 0f; }
            else if (flexWidth >= 0f) le.flexibleWidth = flexWidth;
            if (height >= 0f) { le.preferredHeight = height; le.flexibleHeight = flexHeight >= 0f ? flexHeight : 0f; }
            else if (flexHeight >= 0f) le.flexibleHeight = flexHeight;
            if (minWidth >= 0f) le.minWidth = minWidth;
            if (minHeight >= 0f) le.minHeight = minHeight;
            return le;
        }

        public static VerticalLayoutGroup VBox(GameObject go, float spacing = 0f, int padding = 0, TextAnchor align = TextAnchor.UpperLeft,
            bool expandWidth = true, bool expandHeight = false)
        {
            var v = GetOrAdd<VerticalLayoutGroup>(go);
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childAlignment = align;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = expandWidth;
            v.childForceExpandHeight = expandHeight;
            return v;
        }

        public static HorizontalLayoutGroup HBox(GameObject go, float spacing = 0f, int padding = 0, TextAnchor align = TextAnchor.MiddleLeft,
            bool expandWidth = false, bool expandHeight = true)
        {
            var h = GetOrAdd<HorizontalLayoutGroup>(go);
            h.spacing = spacing;
            h.padding = new RectOffset(padding, padding, padding, padding);
            h.childAlignment = align;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = expandWidth;
            h.childForceExpandHeight = expandHeight;
            return h;
        }

        public static GridLayoutGroup Grid(GameObject go, Vector2 cell, Vector2 spacing, int columns, bool responsive = false)
        {
            var g = GetOrAdd<GridLayoutGroup>(go, responsive);
            g.cellSize = cell;
            g.spacing = spacing;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = columns;
            g.childAlignment = TextAnchor.UpperLeft;
            return g;
        }

        public static void FitHeight(GameObject go)
        {
            var fitter = GetOrAdd<ContentSizeFitter>(go);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        public static void Spacer(Transform parent, float height = 0f, float flexible = 0f)
        {
            var rt = Rect(parent, "Spacer");
            Size(rt.gameObject, height: height, flexHeight: flexible);
        }

        // --- Images ----------------------------------------------------------------------------------

        /// <summary>A plain sprite image sized in whole multiples of the source pixels.</summary>
        public static Image Icon(Transform parent, string name, string spriteName, int scale = S, Color? tint = null)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            var sprite = PixelTheme.Sprite(spriteName);
            img.sprite = sprite;
            img.color = tint ?? Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            if (sprite != null)
                Size(rt.gameObject, sprite.rect.width * scale, sprite.rect.height * scale);
            else
                Size(rt.gameObject, 16 * scale, 16 * scale);
            return img;
        }

        public static Image Panel(Transform parent, string name, string spriteName = "ui_panel", Color? tint = null, bool raycast = false)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            ApplyFrame(img, spriteName, tint ?? Color.white);
            img.raycastTarget = raycast;
            return img;
        }

        public static void ApplyFrame(Image img, string spriteName, Color tint, int scale = S)
        {
            var sprite = PixelTheme.Sprite(spriteName);
            img.sprite = sprite;
            if (sprite != null)
            {
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 4f / scale; // UI sprites import at 25 PPU (4x); scale down to the theme scale
                img.color = tint;
            }
            else
            {
                img.type = Image.Type.Simple;
                img.color = tint == Color.white ? PixelTheme.Brown : tint;
            }
        }

        public static Image Solid(Transform parent, string name, Color color)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        // --- Text ------------------------------------------------------------------------------------

        public static TextMeshProUGUI Text(Transform parent, string name, string text, TextStyle style = TextStyle.Body,
            Color? color = null, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var rt = Rect(parent, name);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = UsesHeadingFont(style) ? PixelTheme.HeadingFont : PixelTheme.BodyFont;
            label.fontSize = SizeOf(style);
            label.color = color ?? PixelTheme.Cream;
            label.alignment = align;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.richText = false;
            label.text = text;
            if (UsesHeadingFont(style)) label.lineSpacing = 18f; // Press Start 2P is tightly leaded; open it up for wrapped lines
            rt.gameObject.AddComponent<ScalableText>(); // after the size is set, so the text-size setting scales from this base
            return label;
        }

        // --- Buttons ---------------------------------------------------------------------------------

        public static Button MakeButton(Transform parent, string name, string label, Action onClick,
            ButtonKind kind = ButtonKind.Secondary, string iconName = null, float width = -1f, float height = 18f * S,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            string normal = "ui_button", highlight = "ui_button_hover";
            switch (kind)
            {
                case ButtonKind.Primary: normal = "ui_button_hover"; highlight = "ui_button_selected"; break;
                case ButtonKind.Tab: normal = "ui_tab"; highlight = "ui_button_hover"; break;
                case ButtonKind.TabActive: normal = "ui_tab_active"; highlight = "ui_tab_active"; break;
                case ButtonKind.Card: normal = "ui_slot_open"; highlight = "ui_button_selected"; break;
            }
            ApplyFrame(img, normal, Color.white);

            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = PixelTheme.Sprite(highlight),
                selectedSprite = PixelTheme.Sprite("ui_button_selected"),
                pressedSprite = PixelTheme.Sprite("ui_button_pressed"),
                disabledSprite = PixelTheme.Sprite("ui_button_disabled"),
            };
            if (button.spriteState.highlightedSprite == null)
            {
                button.transition = Selectable.Transition.ColorTint; // missing art: still show hover/focus
            }

            if (onClick != null)
            {
                button.onClick.AddListener(() => UISound.Click());
                button.onClick.AddListener(() => onClick());
            }
            rt.gameObject.AddComponent<HoverSound>();

            var layout = rt.gameObject.AddComponent<LayoutElement>();
            if (width >= 0f) layout.preferredWidth = width; else layout.flexibleWidth = 1f;
            layout.minHeight = height; // may grow when the player enlarges the text size

            var row = HBox(rt.gameObject, spacing: 2 * S, padding: 3 * S, align: TextAnchor.MiddleCenter, expandWidth: false, expandHeight: false);
            row.childControlWidth = true;
            row.childControlHeight = true;

            if (!string.IsNullOrEmpty(iconName))
            {
                var icon = Icon(rt, "Icon", iconName, S);
                icon.raycastTarget = false;
            }

            if (!string.IsNullOrEmpty(label))
            {
                var color = kind == ButtonKind.TabActive ? PixelTheme.DeepBrown : PixelTheme.Cream;
                var text = Text(rt, "Label", label, TextStyle.Button, color, align);
                Size(text.gameObject, flexWidth: 1f);
            }
            return button;
        }

        // --- Progress bar ----------------------------------------------------------------------------

        /// <summary>Switches a tab button between its idle and active look.</summary>
        public static void SetTabActive(Button button, bool active)
        {
            var img = (Image)button.targetGraphic;
            var sprite = PixelTheme.Sprite(active ? "ui_tab_active" : "ui_tab");
            if (sprite != null) img.sprite = sprite;
            var state = button.spriteState;
            state.highlightedSprite = PixelTheme.Sprite(active ? "ui_tab_active" : "ui_button_hover");
            button.spriteState = state;
            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = active ? PixelTheme.DeepBrown : PixelTheme.Cream;
        }

        public static PixelBar Bar(Transform parent, string name, Color fillColor, float height = 8f * S)
        {
            var frame = Panel(parent, name, "ui_bar_frame");
            Size(frame.gameObject, height: height, flexWidth: 1f);

            var fillRt = Rect(frame.transform, "Fill");
            Stretch(fillRt, 3f * S); // sits inside the frame's 4px border
            var fill = fillRt.gameObject.AddComponent<Image>();
            fill.sprite = PixelTheme.Sprite("ui_bar_fill");
            fill.color = fillColor;
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;

            var bar = frame.gameObject.AddComponent<PixelBar>();
            bar.Init(fill);
            return bar;
        }

        // --- Slider / toggle ---------------------------------------------------------------------------

        public static Slider MakeSlider(Transform parent, string name, float min, float max, float value, Action<float> onChanged)
        {
            var rt = Rect(parent, name);
            Size(rt.gameObject, height: 14f * S, flexWidth: 1f);

            var background = Panel(rt, "Background", "ui_slider_bg");
            Stretch(background.rectTransform);

            var fillArea = Rect(rt, "Fill Area");
            Stretch(fillArea, 5f * S);
            var fillRt = Rect(fillArea, "Fill");
            var fill = fillRt.gameObject.AddComponent<Image>();
            fill.sprite = PixelTheme.Sprite("ui_bar_fill");
            fill.color = PixelTheme.Gold;
            fill.raycastTarget = false;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.sizeDelta = Vector2.zero;

            var handleArea = Rect(rt, "Handle Slide Area");
            Stretch(handleArea, 0f);
            handleArea.offsetMin = new Vector2(8f * S, 0f);
            handleArea.offsetMax = new Vector2(-8f * S, 0f);
            var handleRt = Rect(handleArea, "Handle");
            var handle = handleRt.gameObject.AddComponent<Image>();
            handle.sprite = PixelTheme.Sprite("ui_knob");
            handle.color = Color.white;
            handleRt.sizeDelta = new Vector2(14f * S, 14f * S);
            if (handle.sprite == null) handle.color = PixelTheme.Gold;

            var slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            var colors = slider.colors;
            colors.highlightedColor = PixelTheme.Gold;
            colors.selectedColor = PixelTheme.Gold;
            colors.pressedColor = PixelTheme.Orange;
            slider.colors = colors;
            slider.onValueChanged.AddListener(v => onChanged?.Invoke(v));
            return slider;
        }

        public static Toggle MakeToggle(Transform parent, string name, string label, bool value, Action<bool> onChanged)
        {
            var rt = Rect(parent, name);
            Size(rt.gameObject, height: 14f * S, flexWidth: 1f);
            HBox(rt.gameObject, spacing: 4 * S, align: TextAnchor.MiddleLeft, expandWidth: false, expandHeight: false);

            var box = Panel(rt, "Box", null);
            box.sprite = PixelTheme.Sprite("ui_toggle_box");
            box.type = Image.Type.Simple;
            box.color = box.sprite != null ? Color.white : PixelTheme.Brown;
            box.preserveAspect = true;
            box.raycastTarget = true;
            Size(box.gameObject, 16f * S, 16f * S);

            var check = Icon(box.transform, "Check", "ui_toggle_check", S);
            Stretch(check.rectTransform);
            Destroy(check.GetComponent<LayoutElement>());

            Text(rt, "Label", label, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.MidlineLeft);

            var toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = value;
            var colors = toggle.colors;
            colors.highlightedColor = PixelTheme.Gold;
            colors.selectedColor = PixelTheme.Gold;
            toggle.colors = colors;
            toggle.onValueChanged.AddListener(v => { UISound.Click(); onChanged?.Invoke(v); });
            return toggle;
        }

        private static void Destroy(UnityEngine.Object o)
        {
            if (o != null) UnityEngine.Object.Destroy(o);
        }

        // --- Scroll view ----------------------------------------------------------------------------------

        /// <summary>A vertically scrolling area with a pixel scrollbar. Returns the content rect to add children to.</summary>
        public static RectTransform ScrollView(Transform parent, string name, out ScrollRect scroll)
        {
            var root = Rect(parent, name);
            scroll = root.gameObject.AddComponent<ScrollRect>();
            Size(root.gameObject, flexWidth: 1f, flexHeight: 1f);

            var viewport = Rect(root, "Viewport");
            Stretch(viewport);
            viewport.offsetMax = new Vector2(-10f * S, 0f);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = Rect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(-3f * S, 0f); // keeps right-aligned text clear of the clipping edge and scrollbar

            var barRt = Rect(root, "Scrollbar");
            barRt.anchorMin = new Vector2(1f, 0f);
            barRt.anchorMax = new Vector2(1f, 1f);
            barRt.pivot = new Vector2(1f, 0.5f);
            barRt.sizeDelta = new Vector2(8f * S, 0f);
            var barBg = barRt.gameObject.AddComponent<Image>();
            ApplyFrame(barBg, "ui_slider_bg", Color.white);

            var slideArea = Rect(barRt, "Sliding Area");
            Stretch(slideArea, 1f * S);
            var handleRt = Rect(slideArea, "Handle");
            Stretch(handleRt);
            var handleImg = handleRt.gameObject.AddComponent<Image>();
            ApplyFrame(handleImg, "ui_button_hover", Color.white);

            var scrollbar = barRt.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRt;
            scrollbar.targetGraphic = handleImg;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };

            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            root.gameObject.AddComponent<ScrollFollowSelection>().Init(scroll);
            return content;
        }

        /// <summary>A parchment card that behaves as a button (hover and keyboard focus tint it gold).</summary>
        public static Button MakeCardButton(Transform parent, string name, Action onClick, string spriteName = "ui_slot_open")
        {
            var img = Panel(parent, name, spriteName, raycast: true);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.96f, 0.8f, 1f);
            colors.selectedColor = new Color(1f, 0.9f, 0.55f, 1f);
            colors.pressedColor = new Color(0.9f, 0.8f, 0.5f, 1f);
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(() => { UISound.Click(); onClick(); });
            img.gameObject.AddComponent<HoverSound>();
            return button;
        }

        /// <summary>
        /// Makes a read-only element focusable (keyboard/gamepad) so long lists can be scrolled without a mouse.
        /// It is a plain Selectable, not a button: it highlights but has no click action.
        /// </summary>
        public static Selectable MakeFocusable(Graphic graphic)
        {
            var sel = graphic.gameObject.AddComponent<Selectable>();
            sel.targetGraphic = graphic;
            sel.transition = Selectable.Transition.ColorTint;
            var colors = sel.colors;
            colors.highlightedColor = new Color(1f, 0.96f, 0.8f, 1f);
            colors.selectedColor = new Color(1f, 0.93f, 0.62f, 1f);
            colors.pressedColor = colors.selectedColor;
            sel.colors = colors;
            graphic.raycastTarget = true;
            return sel;
        }

        public static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }
        }
    }

    /// <summary>A segmented-looking progress bar whose fill animates toward its target (instantly when reduced motion is on).</summary>
    public class PixelBar : MonoBehaviour
    {
        private Image fill;
        private float target;

        public void Init(Image fillImage) => fill = fillImage;

        public void SetValue(float value, bool animate = true)
        {
            target = Mathf.Clamp01(value);
            if (!animate || LucillesJourney.Core.SettingsManager.ReducedMotion || !isActiveAndEnabled) fill.fillAmount = target;
        }

        private void Update()
        {
            if (fill == null || Mathf.Approximately(fill.fillAmount, target)) return;
            fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, Time.unscaledDeltaTime * 1.5f);
        }
    }

    /// <summary>A grid that fits as many whole columns as the available width allows, so cards reflow instead of clipping.</summary>
    public class ResponsiveGrid : GridLayoutGroup
    {
        public override void SetLayoutHorizontal()
        {
            float inner = rectTransform.rect.width - padding.horizontal;
            if (inner > 0f)
            {
                int columns = Mathf.Max(1, Mathf.FloorToInt((inner + spacing.x) / (cellSize.x + spacing.x)));
                constraint = Constraint.FixedColumnCount;
                constraintCount = columns;
            }
            base.SetLayoutHorizontal();
        }
    }

    /// <summary>
    /// Keeps a centred panel at its designed size, but never larger than its parent minus a margin. Needed
    /// because the interface scale setting shrinks the canvas (in canvas units) as it grows.
    /// </summary>
    public class ClampSize : MonoBehaviour
    {
        public Vector2 Design;
        public float Margin = 24f;
        public bool ClampWidth = true, ClampHeight = true;

        private RectTransform rt;

        private void OnEnable() => Clamp();

        private void LateUpdate() => Clamp();

        private void Clamp()
        {
            if (Design == Vector2.zero) return; // not configured yet (components run OnEnable as soon as they are added)
            if (rt == null) rt = (RectTransform)transform;
            var parent = rt.parent as RectTransform;
            if (parent == null) return;

            var size = rt.sizeDelta;
            if (ClampWidth) size.x = Mathf.Min(Design.x, parent.rect.width - Margin * 2f);
            if (ClampHeight) size.y = Mathf.Min(Design.y, parent.rect.height - Margin * 2f);
            if (size != rt.sizeDelta) rt.sizeDelta = size;
        }
    }

    /// <summary>Plays the soft hover tick when the pointer enters or keyboard/gamepad focus lands on a control.</summary>
    public class HoverSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        public void OnPointerEnter(PointerEventData eventData) => UISound.Hover();
        public void OnSelect(BaseEventData eventData) => UISound.Hover();
    }

    /// <summary>Keeps the keyboard/gamepad-selected child inside a ScrollRect's viewport.</summary>
    public class ScrollFollowSelection : MonoBehaviour
    {
        private ScrollRect scroll;
        private GameObject lastSelected;

        public void Init(ScrollRect rect) => scroll = rect;

        private int enabledFrame;

        // A screen that has just opened shows its top; only later keyboard/gamepad moves scroll to the selection.
        private void OnEnable()
        {
            enabledFrame = Time.frameCount;
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
        }

        private void LateUpdate()
        {
            if (scroll == null || EventSystem.current == null) return;
            var selected = EventSystem.current.currentSelectedGameObject;
            if (Time.frameCount - enabledFrame < 4) { lastSelected = selected; return; }
            if (selected == null || selected == lastSelected) return;
            lastSelected = selected;
            if (!selected.transform.IsChildOf(scroll.content)) return;

            var target = (RectTransform)selected.transform;
            var content = scroll.content;
            var viewport = scroll.viewport;

            Canvas.ForceUpdateCanvases();
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var local = new[] { content.InverseTransformPoint(corners[0]), content.InverseTransformPoint(corners[1]) };
            float itemTop = local[1].y, itemBottom = local[0].y;

            float contentHeight = content.rect.height, viewHeight = viewport.rect.height;
            if (contentHeight <= viewHeight) return;

            float scrollableTop = -(1f - scroll.verticalNormalizedPosition) * (contentHeight - viewHeight);
            float viewTop = scrollableTop, viewBottom = scrollableTop - viewHeight;
            if (itemTop > viewTop) scroll.verticalNormalizedPosition = Mathf.Clamp01(1f + (itemTop) / (contentHeight - viewHeight));
            else if (itemTop - itemBottom > viewHeight) scroll.verticalNormalizedPosition = Mathf.Clamp01(1f + itemTop / (contentHeight - viewHeight)); // taller than the view: show its top
            else if (itemBottom < viewBottom) scroll.verticalNormalizedPosition = Mathf.Clamp01(1f + (itemBottom + viewHeight) / (contentHeight - viewHeight));
        }
    }
}
