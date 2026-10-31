#if LJ_AR
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Gamification.UI;
using LucillesJourney.UI;

namespace LucillesJourney.AR
{
    /// <summary>
    /// Placement prompts for the AR tabletop (hint text, PLACE and MOVE). Added automatically by <see cref="ARTabletop"/>.
    /// The stick and action buttons are the shared <see cref="MobileControls"/>; this only shows them once the board is placed.
    /// </summary>
    public class ARTabletopUI : MonoBehaviour
    {
        private ARTabletop tabletop;
        private TextMeshProUGUI hint;
        private Button placeButton;
        private Button moveButton;

        private void Start()
        {
            tabletop = ARTabletop.Instance;
            if (tabletop == null) { enabled = false; return; }

            Build();
            tabletop.PlacedChanged += Refresh;
            Refresh(tabletop.IsPlaced);
        }

        private void OnDestroy()
        {
            if (tabletop != null) tabletop.PlacedChanged -= Refresh;
            MobileControls.Visible = true; // leave the shared controls on for whatever comes next
        }

        private void Update()
        {
            if (tabletop.IsPlaced) return;
            hint.text = tabletop.HasSurface
                ? "Aim at the table, then tap PLACE"
                : "Move your phone slowly to find a flat surface  (surfaces found: " + tabletop.PlaneCount + ")";
            placeButton.interactable = tabletop.HasSurface;
        }

        private void Refresh(bool placed)
        {
            hint.gameObject.SetActive(!placed);
            placeButton.gameObject.SetActive(!placed);
            moveButton.gameObject.SetActive(placed);
            MobileControls.Visible = placed; // no stick while still choosing where the board goes
        }

        private void Build()
        {
            var canvasObject = new GameObject("ARTabletopCanvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            var root = (RectTransform)canvasObject.transform;

            hint = UIFactory.Text(root, "Hint", "", TextStyle.Label, PixelTheme.Cream, TextAlignmentOptions.Center);
            UIFactory.Anchor(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(1500f, 90f));

            placeButton = UIFactory.MakeButton(root, "PlaceButton", "PLACE", () => tabletop.Place(), ButtonKind.Primary, width: 520f);
            UIFactory.Anchor((RectTransform)placeButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(520f, 110f));

            moveButton = UIFactory.MakeButton(root, "MoveButton", "MOVE", () => tabletop.BeginMove(), ButtonKind.Secondary, width: 300f);
            UIFactory.Anchor((RectTransform)moveButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(300f, 90f));
        }
    }
}
#endif
