using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Gamification.UI;
using LucillesJourney.Interaction;
using LucillesJourney.Narrative;
using LucillesJourney.UI;
using LucillesJourney.World;

namespace LucillesJourney.AR
{
    /// <summary>
    /// Desktop "camera preview" mode - not real AR. Your webcam fills the screen and the game appears on a board you
    /// place by clicking. There is no surface detection or tracking, so the board stays where it is on screen if the
    /// camera moves. Needs no AR packages and works in the editor and on PC builds.
    ///
    /// Click = place. Scroll = size. Z / X = rotate. M = pick the board up again. C = next camera. F = mirror.
    /// T = anchor to scene on/off: once placed, the board follows the picture as the camera moves (see WebcamMotionTracker).
    /// H = show/hide this help. (E and Space stay the game's Interact keys.)
    ///
    /// Switched on from the main menu or the in-game pause menu (<see cref="Enable"/> / <see cref="Disable"/>).
    /// On the ending screen there is no game board: the webcam simply becomes the backdrop behind the ending.
    ///
    /// The game keeps running exactly as before: its camera renders into a small crisp texture shown on the board,
    /// the same trick the real AR mode uses.
    /// </summary>
    public class WebcamTabletop : MonoBehaviour
    {
        public static WebcamTabletop Instance { get; private set; }

        private static readonly Vector2Int TextureSize = new Vector2Int(384, 216);
        private const float BaseBoardWidth = 1152f;   // canvas units (1920x1080 reference): 60% of the screen width
        private const float Border = 12f;
        private const float MinScale = 0.3f, MaxScale = 2.5f;

        private Camera gameCamera;
        private RenderTexture texture;
        private readonly WebcamFeed feed = new WebcamFeed();
        private bool endingMode;

        private RectTransform canvasRect, boardRoot, boardInner;
        private RawImage background, boardImage;
        private AspectRatioFitter backgroundFit;
        private TextMeshProUGUI hint;
        private GameObject hintStrip;

        private readonly WebcamMotionTracker tracker = new WebcamMotionTracker();
        private bool anchorToScene = true;

        private bool placed;
        private bool showHelp;
        private float helpUntil;
        private float scale = 1f;
        private float angle;
        private GameManager pauseOwner;

        public bool IsPlaced => placed;

        /// <summary>True while the player has the laptop camera mode switched on.</summary>
        public static bool IsActive => ARMode.WebcamRequested;

        /// <summary>Switches the camera mode on from anywhere (the pause menu uses this). Starts at once in the current scene.</summary>
        public static void Enable()
        {
            if (!ARMode.WebcamSupported) return;
            ARMode.WebcamRequested = true;
            if (FindFirstObjectByType<WebcamTabletop>() == null) new GameObject("WebcamTabletop").AddComponent<WebcamTabletop>();
        }

        /// <summary>Switches the camera mode off and puts the game back on the normal screen.</summary>
        public static void Disable()
        {
            ARMode.WebcamRequested = false;
            foreach (var mode in FindObjectsByType<WebcamTabletop>(FindObjectsSortMode.None)) Destroy(mode.gameObject);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Start-up: spawn in gameplay scenes only when the player chose "Play with Camera"
        // ---------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!ARMode.WebcamRequested || ARMode.Requested) return;
            if (FindFirstObjectByType<WebcamTabletop>() != null) return;
            new GameObject("WebcamTabletop").AddComponent<WebcamTabletop>();
        }

        private IEnumerator Start()
        {
            if (FindFirstObjectByType<MainMenuController>() != null) { Destroy(gameObject); yield break; }

            // The ending scene has no game camera: there the webcam is just the backdrop.
            if (FindFirstObjectByType<EndingUI>() != null)
            {
                endingMode = true;
                Instance = this;
                BuildUi();
                yield return StartCamera();
                yield break;
            }

            // The game camera may appear a few frames into the scene; other scenes never have one.
            CameraFollow2D follow = null;
            for (int i = 0; i < 120 && follow == null; i++)
            {
                follow = FindFirstObjectByType<CameraFollow2D>();
                if (follow == null) yield return null;
            }
            if (follow == null) { Destroy(gameObject); yield break; }

            Instance = this;
            gameCamera = follow.GetComponent<Camera>();
            SetUpGameView();
            BuildUi();
            InteractionDetector.ScreenToWorldOverride = ScreenToGameWorld;
            ApplyBoardTransform();
            helpUntil = Time.unscaledTime + 10f;
            yield return StartCamera();
        }

        private IEnumerator StartCamera()
        {
            feed.Restarted += tracker.Reset;
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            feed.Begin(background);
        }

        private void SetUpGameView()
        {
            CameraFollow2D.OverrideScreenHeight = TextureSize.y; // exactly 384x216 art pixels: crisp, no shimmer
            texture = new RenderTexture(TextureSize.x, TextureSize.y, 16, RenderTextureFormat.ARGB32)
            {
                name = "LucilleWebcamView",
                filterMode = FilterMode.Point,
                antiAliasing = 1,
                useMipMap = false,
            };
            texture.Create();
            gameCamera.targetTexture = texture;
            gameCamera.ResetAspect();
            // The game camera keeps its tag and AudioListener, so Camera.main and sound behave as normal.
        }

        // ---------------------------------------------------------------------------------------------------------
        // UI: webcam background + board, on a canvas that sits behind the game's own menus and HUD
        // ---------------------------------------------------------------------------------------------------------

        private void BuildUi()
        {
            var canvasObject = new GameObject("WebcamTabletopCanvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = endingMode ? -99 : -200; // ending: above the menu backdrop (-100), below the ending's own canvas
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasRect = (RectTransform)canvasObject.transform;

            // Webcam fills the screen (cropped, never stretched). Dark grey until the first frame or if there is no camera.
            var bgObject = new GameObject("Webcam", typeof(RectTransform));
            bgObject.transform.SetParent(canvasRect, false);
            background = bgObject.AddComponent<RawImage>();
            background.color = new Color(0.12f, 0.13f, 0.14f, 1f);
            background.raycastTarget = false;
            backgroundFit = bgObject.AddComponent<AspectRatioFitter>();
            backgroundFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            backgroundFit.aspectRatio = 16f / 9f;

            if (endingMode)
            {
                BuildHint();
                return;
            }

            // Board: dark frame (root) with the game picture inside.
            var rootObject = new GameObject("Board", typeof(RectTransform));
            rootObject.transform.SetParent(canvasRect, false);
            boardRoot = (RectTransform)rootObject.transform;
            boardRoot.anchorMin = boardRoot.anchorMax = new Vector2(0.5f, 0.5f);
            boardRoot.pivot = new Vector2(0.5f, 0.5f);
            var frame = rootObject.AddComponent<Image>();
            frame.color = new Color(0.10f, 0.13f, 0.14f, 1f);
            frame.raycastTarget = false;

            var innerObject = new GameObject("Game", typeof(RectTransform));
            innerObject.transform.SetParent(boardRoot, false);
            boardInner = (RectTransform)innerObject.transform;
            boardInner.anchorMin = Vector2.zero;
            boardInner.anchorMax = Vector2.one;
            boardInner.offsetMin = new Vector2(Border, Border);
            boardInner.offsetMax = new Vector2(-Border, -Border);
            boardImage = innerObject.AddComponent<RawImage>();
            boardImage.texture = texture;
            boardImage.raycastTarget = false;

            BuildHint();
        }

        // The help line lives on its own canvas ABOVE the game's HUD (the chapter panel and buttons would cover it), at the
        // bottom centre on a dark strip so it reads over any webcam picture. It hides itself during dialogue and menus.
        private void BuildHint()
        {
            var canvasObject = new GameObject("WebcamHintCanvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var stripObject = new GameObject("HintStrip", typeof(RectTransform));
            stripObject.transform.SetParent(canvasObject.transform, false);
            hintStrip = stripObject;
            var strip = stripObject.AddComponent<Image>();
            strip.color = new Color(0.10f, 0.13f, 0.14f, 0.78f);
            strip.raycastTarget = false;
            UIFactory.Anchor((RectTransform)stripObject.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(1760f, 46f));

            hint = UIFactory.Text((RectTransform)stripObject.transform, "Hint", "", TextStyle.Label, PixelTheme.Cream, TextAlignmentOptions.Center);
            UIFactory.Stretch(hint.rectTransform);
            hint.fontSize *= 0.6f;
            hint.raycastTarget = false;
            hintStrip.SetActive(false);
        }

        private void SetHint(string text)
        {
            if (hint == null) return;
            hint.text = text;
            bool show = !string.IsNullOrEmpty(text);
            if (hintStrip.activeSelf != show) hintStrip.SetActive(show);
        }

        private void ApplyBoardTransform()
        {
            float width = BaseBoardWidth * scale;
            float height = width * TextureSize.y / TextureSize.x;
            boardRoot.sizeDelta = new Vector2(width + Border * 2f, height + Border * 2f);
            boardRoot.localEulerAngles = new Vector3(0f, 0f, angle);
            var c = Color.white;
            c.a = placed ? 1f : 0.65f;
            boardImage.color = c;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Per-frame input
        // ---------------------------------------------------------------------------------------------------------

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.fKey.wasPressedThisFrame) feed.ToggleMirror();
                if (keyboard.cKey.wasPressedThisFrame) feed.Next();
            }
            feed.Tick(backgroundFit);

            if (endingMode)
            {
                SetHint(HintBlocked ? "" : "C: next camera   F: mirror");
                return;
            }
            if (gameCamera == null) return;

            var mouse = Mouse.current;
            var pointer = Pointer.current;
            float dt = Time.unscaledDeltaTime; // the game is paused (timeScale 0) while placing

            if (keyboard != null)
            {
                if (keyboard.zKey.isPressed) angle += 90f * dt;
                if (keyboard.xKey.isPressed) angle -= 90f * dt;
                if (keyboard.tKey.wasPressedThisFrame) anchorToScene = !anchorToScene;
                if (keyboard.hKey.wasPressedThisFrame) showHelp = !showHelp;
                float grow = (keyboard.equalsKey.isPressed || keyboard.numpadPlusKey.isPressed ? 1f : 0f) -
                             (keyboard.minusKey.isPressed || keyboard.numpadMinusKey.isPressed ? 1f : 0f);
                if (grow != 0f) scale = Mathf.Clamp(scale * Mathf.Exp(grow * 0.9f * dt), MinScale, MaxScale);
                if (placed && keyboard.mKey.wasPressedThisFrame) BeginMove();
            }
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    // A wheel notch reads +-120, a laptop trackpad only a few units: give every scroll event a usable step.
                    float step = Mathf.Sign(scroll) * Mathf.Clamp(Mathf.Abs(scroll), 20f, 120f);
                    scale = Mathf.Clamp(scale * (1f + step * 0.0008f), MinScale, MaxScale);
                }
            }

            if (!placed)
            {
                // Not while a menu (the pause menu, say) is open: a click there must not place the board.
                if (pointer != null && !(JourneyUI.Instance != null && JourneyUI.Instance.IsAnyScreenOpen))
                {
                    Vector2 screen = pointer.position.ReadValue();
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out var local))
                        boardRoot.anchoredPosition = local;

                    bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                    if (pointer.press.wasPressedThisFrame && !overUi) Place();
                }
                // Place() may just have unpaused the game; pausing again here would freeze it for good.
                if (!placed) ApplyPause();
            }

            ApplyBoardTransform();

            var webcam = feed.Texture;
            if (placed && anchorToScene) { if (webcam != null && webcam.didUpdateThisFrame) FollowScene(); }
            else tracker.Reset();

            UpdateHint();
        }

        // Dialogue and menus have their own text; the camera hint stays out of their way.
        private static bool HintBlocked =>
            (NarrativePanelUI.Instance != null && NarrativePanelUI.Instance.IsShowing) ||
            (JourneyUI.Instance != null && JourneyUI.Instance.IsAnyScreenOpen);

        private void UpdateHint()
        {
            if (HintBlocked) { SetHint(""); return; }

            string anchor = "T: anchor to scene " + (anchorToScene ? "ON" : "OFF");
            if (!placed)
                SetHint("Click to place the board   Scroll or + / -: size   Z / X: rotate   C: camera   F: mirror   " + anchor);
            else if (showHelp || Time.unscaledTime < helpUntil)
                SetHint("M: move board   " + anchor + "   Scroll or + / -: size   Z / X: rotate   C: camera   F: mirror   H: hide help");
            else
                SetHint("H: camera help");
        }

        // Moves the board with the picture: whatever the camera did between two frames, the board does too.
        private void FollowScene()
        {
            var webcam = feed.Texture;
            if (webcam.videoRotationAngle == 90 || webcam.videoRotationAngle == 270) return; // desktop cameras are upright
            if (!tracker.Step(webcam, feed.Mirrored, webcam.videoVerticallyMirrored, out float a, out float b, out Vector2 t)) return;

            // The tracker works in pixels of its 160-wide frame; the webcam picture is shown at the background's size.
            float k = background.rectTransform.rect.width / WebcamMotionTracker.Width;
            if (k < 0.01f) return;

            Vector2 p = boardRoot.anchoredPosition / k;
            var q = new Vector2(a * p.x - b * p.y + t.x, b * p.x + a * p.y + t.y);
            boardRoot.anchoredPosition = q * k;
            scale = Mathf.Clamp(scale * Mathf.Sqrt(a * a + b * b), MinScale, MaxScale);
            angle += Mathf.Atan2(b, a) * Mathf.Rad2Deg;
        }

        private void Place()
        {
            if (placed) return;
            placed = true;
            helpUntil = Time.unscaledTime + 6f;
            ReleasePause();
        }

        private void BeginMove()
        {
            placed = false;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Tap-to-interact mapping and pausing
        // ---------------------------------------------------------------------------------------------------------

        private Vector2? ScreenToGameWorld(Vector2 screenPoint)
        {
            if (!placed || boardInner == null || gameCamera == null) return null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(boardInner, screenPoint, null, out var local)) return null;

            Rect rect = boardInner.rect;
            float u = local.x / rect.width + 0.5f, v = local.y / rect.height + 0.5f;
            if (u < 0f || u > 1f || v < 0f || v > 1f) return null; // clicked outside the board

            Vector3 world = gameCamera.ViewportToWorldPoint(new Vector3(u, v, 0f));
            return new Vector2(world.x, world.y);
        }

        private void ApplyPause()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsPaused) return;
            gm.SetPaused(true);
            pauseOwner = gm;
        }

        // Never goes through GameManager.Instance: that getter creates a new GameManager if none exists, which must not
        // happen while a scene or Play mode is shutting down.
        private void ReleasePause()
        {
            var gm = pauseOwner;
            pauseOwner = null;
            if (gm != null && gm.IsPaused) gm.SetPaused(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (InteractionDetector.ScreenToWorldOverride == (Func<Vector2, Vector2?>)ScreenToGameWorld)
                InteractionDetector.ScreenToWorldOverride = null;
            if (!endingMode) CameraFollow2D.OverrideScreenHeight = 0;
            if (gameCamera != null) { gameCamera.targetTexture = null; gameCamera.ResetAspect(); }
            if (texture != null) { texture.Release(); Destroy(texture); }
            feed.Stop();
            tracker.Dispose();
            ReleasePause();
        }
    }
}
