using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Gamification.UI;
using LucillesJourney.World;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Touch controls for phones and tablets: a virtual stick plus USE / LOG / MENU buttons. Appears automatically in
    /// gameplay scenes on a mobile device (normal mode and AR mode alike) and never on PC or in menus.
    ///
    /// They are Input System on-screen controls that act like a gamepad, and the game already listens to a gamepad:
    /// left stick moves, South (A) interacts / advances dialogue, North (Y) opens the journal, Start opens the menu.
    /// So no change to the player, interaction or dialogue scripts is needed. Hidden while the game is paused.
    /// </summary>
    public class MobileControls : MonoBehaviour
    {
        private const string EditorPref = "lj_touch_controls"; // lets you test the controls in the editor

        private static MobileControls instance;
        private static bool visible = true;
        private static Sprite circle;

        private GameObject root;

        /// <summary>True on a phone/tablet, or in the editor when the "show touch controls" test option is on.</summary>
        public static bool Wanted => Application.isMobilePlatform || (Application.isEditor && PlayerPrefs.GetInt(EditorPref, 0) == 1);

        /// <summary>Lets other systems (the AR placement screen) hide the controls until they are needed.</summary>
        public static bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                if (instance != null && instance.root != null) instance.root.SetActive(value);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!Wanted || FindFirstObjectByType<MobileControls>() != null) return;
            new GameObject("MobileControls").AddComponent<MobileControls>();
        }

        private IEnumerator Start()
        {
            // Gameplay scenes have the game camera (maybe a few frames in); menus never do, so they get no controls.
            CameraFollow2D follow = null;
            for (int i = 0; i < 120 && follow == null; i++)
            {
                follow = FindFirstObjectByType<CameraFollow2D>();
                if (follow == null) yield return null;
            }
            if (follow == null) { Destroy(gameObject); yield break; }

            EnsureEventSystem();
            Build();
            instance = this;
            root.SetActive(visible);
        }

        private void Update()
        {
            if (root == null) return;
            bool show = visible && !GameManager.Instance.IsPaused;
            if (root.activeSelf != show) root.SetActive(show);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        // ---------------------------------------------------------------------------------------------------------

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private void Build()
        {
            var canvasObject = new GameObject("MobileControlsCanvas");
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

            // Everything lives under one object so it can be shown/hidden as a unit.
            root = new GameObject("Controls", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.SetParent(canvasObject.transform, false);
            UIFactory.Stretch(rootRect);

            BuildStick(rootRect);
            BuildButton(rootRect, "Interact", "USE", "<Gamepad>/buttonSouth", new Vector2(1f, 0f), new Vector2(-70f, 90f), 230f, PixelTheme.Gold);
            BuildButton(rootRect, "Journal", "LOG", "<Gamepad>/buttonNorth", new Vector2(1f, 0f), new Vector2(-360f, 150f), 130f, PixelTheme.Sand);
            BuildButton(rootRect, "Menu", "MENU", "<Gamepad>/start", new Vector2(1f, 1f), new Vector2(-40f, -40f), 130f, PixelTheme.Sand);
        }

        private void BuildStick(RectTransform parent)
        {
            var ring = Circle(parent, "StickBase", new Color(0.10f, 0.13f, 0.14f, 0.45f));
            UIFactory.Anchor(ring.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(90f, 90f), new Vector2(330f, 330f));

            // The control path has to be set before the component enables, so build it inactive.
            var handleObject = new GameObject("StickHandle", typeof(RectTransform));
            handleObject.SetActive(false);
            var handleRect = (RectTransform)handleObject.transform;
            handleRect.SetParent(ring.rectTransform, false);
            UIFactory.Anchor(handleRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170f, 170f));
            var image = handleObject.AddComponent<Image>();
            image.sprite = CircleSprite();
            image.color = new Color(0.96f, 0.92f, 0.82f, 0.85f);
            var stick = handleObject.AddComponent<OnScreenStick>();
            stick.controlPath = "<Gamepad>/leftStick";
            stick.movementRange = 80f;
            handleObject.SetActive(true);
        }

        private void BuildButton(RectTransform parent, string name, string label, string controlPath,
            Vector2 anchor, Vector2 position, float size, Color tint)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.SetActive(false);
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            UIFactory.Anchor(rect, anchor, anchor, position, new Vector2(size, size));
            var image = go.AddComponent<Image>();
            image.sprite = CircleSprite();
            image.color = new Color(tint.r, tint.g, tint.b, 0.85f);
            var button = go.AddComponent<OnScreenButton>();
            button.controlPath = controlPath;
            go.SetActive(true);

            var text = UIFactory.Text(rect, "Label", label, TextStyle.Label, PixelTheme.Ink, TextAlignmentOptions.Center);
            UIFactory.Stretch(text.rectTransform);
        }

        private static Image Circle(RectTransform parent, string name, Color color)
        {
            var rect = UIFactory.Rect(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = CircleSprite();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite CircleSprite()
        {
            if (circle != null) return circle;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            float r = size * 0.5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                byte a = (byte)Mathf.Clamp(Mathf.RoundToInt((r - d) * 255f), 0, 255); // 1px soft edge
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return circle;
        }
    }
}
