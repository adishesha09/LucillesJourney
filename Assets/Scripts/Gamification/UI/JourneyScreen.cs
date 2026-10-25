using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>Key labels shown in the HUD and controls guide. The bindings themselves are created in JourneyUI.</summary>
    public static class JourneyInput
    {
        public const string Pause = "Esc";
        public const string Journal = "J";
        public const string Album = "M";
        public const string Keepsakes = "K";
        public const string NextTab = "Tab";
    }

    /// <summary>
    /// Base class for a full-screen interface (pause menu, journal hub, settings...). Screens are
    /// plain objects built from code by JourneyUI so they all share one design system and one
    /// open/close/back stack.
    /// </summary>
    public abstract class JourneyScreen
    {
        protected JourneyUI Ui { get; private set; }
        public GameObject Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        /// <summary>Pause-style screens can be dismissed with Back/Escape; decision screens cannot.</summary>
        public virtual bool CanDismiss => true;

        /// <summary>The control that gets keyboard/gamepad focus when the screen opens.</summary>
        protected abstract Selectable DefaultSelection { get; }

        protected abstract void Build(RectTransform root);

        public void Create(JourneyUI owner, Transform parent)
        {
            Ui = owner;
            var rt = UIFactory.Rect(parent, GetType().Name);
            UIFactory.Stretch(rt);
            Root = rt.gameObject;
            Group = Root.AddComponent<CanvasGroup>();
            Build(rt);
            Root.SetActive(false);
        }

        public virtual void OnOpen() { }
        public virtual void Refresh() { }

        /// <summary>Return true if the screen handled Back itself (for example by leaving a detail view).</summary>
        public virtual bool HandleBack() => false;

        public void Focus()
        {
            var es = EventSystem.current;
            if (es == null) return;
            var target = DefaultSelection;
            es.SetSelectedGameObject(null);
            if (target != null && target.gameObject.activeInHierarchy && target.IsInteractable())
                es.SetSelectedGameObject(target.gameObject);
        }

        // --- Shared frame --------------------------------------------------------------------------------

        /// <summary>
        /// Builds the standard framed panel (title row + body) used by most screens and returns the body.
        /// </summary>
        protected RectTransform BuildFrame(RectTransform root, string title, string iconName, Vector2 size, out RectTransform titleRow)
        {
            var frame = UIFactory.Panel(root, "Frame", "ui_panel", raycast: true);
            UIFactory.Anchor(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            frame.gameObject.AddComponent<ClampSize>().Design = size;
            UIFactory.VBox(frame.gameObject, spacing: 5 * UIFactory.S, padding: 8 * UIFactory.S);

            titleRow = UIFactory.Rect(frame.transform, "TitleRow");
            UIFactory.Size(titleRow.gameObject, height: 20 * UIFactory.S);
            UIFactory.HBox(titleRow.gameObject, spacing: 4 * UIFactory.S, align: TextAnchor.MiddleLeft);
            if (!string.IsNullOrEmpty(iconName)) UIFactory.Icon(titleRow, "Icon", iconName, UIFactory.S);
            var t = UIFactory.Text(titleRow, "Title", title, TextStyle.Heading, PixelTheme.Gold, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(t.gameObject, flexWidth: 1f);

            var divider = UIFactory.Solid(frame.transform, "Divider", PixelTheme.Tan);
            UIFactory.Size(divider.gameObject, height: UIFactory.S);

            var body = UIFactory.Rect(frame.transform, "Body");
            UIFactory.Size(body.gameObject, flexWidth: 1f, flexHeight: 1f);
            return body;
        }

        protected static TMP_Text AddHint(Transform parent, string text)
        {
            var hint = UIFactory.Text(parent, "Hint", text, TextStyle.Meta, PixelTheme.Sand, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(hint.gameObject, height: 12 * UIFactory.S);
            return hint;
        }
    }

    /// <summary>Raises a callback when a selectable receives keyboard/gamepad/pointer focus (drives master-detail panes).</summary>
    public class SelectNotifier : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        public System.Action OnSelected;
        public void OnSelect(BaseEventData eventData) => OnSelected?.Invoke();
        public void OnPointerEnter(PointerEventData eventData) { }
    }
}
