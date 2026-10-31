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
    /// (E and Space stay the game's Interact keys.)
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

        private Camera gameCamera;
        private RenderTexture texture;
        private WebCamTexture webcam;
        private WebCamDevice[] devices = new WebCamDevice[0];
        private int deviceIndex;
        private int failedStarts;
        private float startedAt;
        private bool mirrored = true;

        private RectTransform canvasRect, boardRoot, boardInner;
        private RawImage background, boardImage;
        private AspectRatioFitter backgroundFit;
        private TextMeshProUGUI hint;

        private readonly WebcamMotionTracker tracker = new WebcamMotionTracker();
        private bool anchorToScene = true;

        private bool placed;
        private float scale = 1f;
        private float angle;
        private GameManager pauseOwner;

        public bool IsPlaced => placed;

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
            // The game camera may appear a few frames into the scene; the menu scene never has one.
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

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            devices = WebCamTexture.devices;
            StartWebcam(FirstUsableDevice(), true);
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
            canvas.sortingOrder = -200;
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

            hint = UIFactory.Text(canvasRect, "Hint", "", TextStyle.Label, PixelTheme.Cream, TextAlignmentOptions.Center);
            UIFactory.Anchor(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1800f, 70f));
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
        // Webcam
        // ---------------------------------------------------------------------------------------------------------

        private void StartWebcam(int index, bool userChoice)
        {
            StopWebcam();
            tracker.Reset();
            if (devices.Length == 0)
            {
                Debug.LogWarning("Camera preview: no webcam found - using a plain background.");
                background.texture = null;
                return;
            }
            if (userChoice) failedStarts = 0;
            deviceIndex = ((index % devices.Length) + devices.Length) % devices.Length;
            webcam = CreateTexture(devices[deviceIndex]);
            background.texture = webcam;
            background.color = Color.white;
            webcam.Play();
            startedAt = Time.unscaledTime;
        }

        // Asks the camera which modes it really supports and takes the one closest to 1280x720. Requesting a fixed mode
        // makes Windows fail with "Couldn't config the stream!" on cameras that do not offer exactly that.
        private static WebCamTexture CreateTexture(WebCamDevice device)
        {
            var modes = device.availableResolutions;
            if (modes != null && modes.Length > 0)
            {
                Resolution best = modes[0];
                int bestDistance = int.MaxValue;
                foreach (var mode in modes)
                {
                    int distance = Mathf.Abs(mode.width - 1280) + Mathf.Abs(mode.height - 720);
                    if (distance < bestDistance) { bestDistance = distance; best = mode; }
                }
                int fps = best.refreshRate > 0 ? best.refreshRate : 30;
                return new WebCamTexture(device.name, best.width, best.height, fps);
            }
            return new WebCamTexture(device.name); // let the system choose
        }

        // Infrared / depth cameras (the face-login sensor on many laptops) do not give a normal picture.
        private static bool LooksLikeInfrared(string name) =>
            name.IndexOf(" IR", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("infrared", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("depth", StringComparison.OrdinalIgnoreCase) >= 0;

        private int FirstUsableDevice()
        {
            for (int i = 0; i < devices.Length; i++)
                if (!LooksLikeInfrared(devices[i].name)) return i;
            return 0;
        }

        private int NextUsableDevice(int from)
        {
            for (int step = 1; step <= devices.Length; step++)
            {
                int index = (from + step) % devices.Length;
                if (!LooksLikeInfrared(devices[index].name)) return index;
            }
            return (from + 1) % Mathf.Max(1, devices.Length);
        }

        private void StopWebcam()
        {
            if (webcam == null) return;
            if (webcam.isPlaying) webcam.Stop();
            Destroy(webcam);
            webcam = null;
        }

        private void UpdateWebcamView()
        {
            if (webcam == null || !webcam.isPlaying || webcam.width < 100) return; // width stays ~16 until the first frame
            bool flippedY = webcam.videoVerticallyMirrored;
            background.uvRect = new Rect(mirrored ? 1f : 0f, flippedY ? 1f : 0f, mirrored ? -1f : 1f, flippedY ? -1f : 1f);
            background.rectTransform.localEulerAngles = new Vector3(0f, 0f, -webcam.videoRotationAngle);
            bool sideways = webcam.videoRotationAngle == 90 || webcam.videoRotationAngle == 270;
            backgroundFit.aspectRatio = sideways ? (float)webcam.height / webcam.width : (float)webcam.width / webcam.height;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Per-frame input
        // ---------------------------------------------------------------------------------------------------------

        private void Update()
        {
            if (gameCamera == null) return;

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pointer = Pointer.current;
            float dt = Time.unscaledDeltaTime; // the game is paused (timeScale 0) while placing

            if (keyboard != null)
            {
                if (keyboard.zKey.isPressed) angle += 90f * dt;
                if (keyboard.xKey.isPressed) angle -= 90f * dt;
                if (keyboard.fKey.wasPressedThisFrame) mirrored = !mirrored;
                if (keyboard.tKey.wasPressedThisFrame) anchorToScene = !anchorToScene;
                if (keyboard.cKey.wasPressedThisFrame) StartWebcam(NextUsableDevice(deviceIndex), true);
                if (placed && keyboard.mKey.wasPressedThisFrame) BeginMove();
            }
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f) scale = Mathf.Clamp(scale * (1f + scroll * 0.0006f), 0.3f, 1.6f);
            }

            if (!placed)
            {
                if (pointer != null)
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
            UpdateWebcamView();

            // A camera that never delivers a picture (wrong mode, in use elsewhere) is skipped in favour of the next one.
            if (webcam != null && webcam.width < 100 && devices.Length > 1 && failedStarts < devices.Length - 1 &&
                Time.unscaledTime - startedAt > 3f)
            {
                failedStarts++;
                Debug.LogWarning("Camera preview: '" + devices[deviceIndex].name + "' gave no picture - trying the next camera.");
                StartWebcam(NextUsableDevice(deviceIndex), false);
            }

            if (placed && anchorToScene) { if (webcam != null && webcam.didUpdateThisFrame) FollowScene(); }
            else tracker.Reset();

            string anchor = "T: anchor to scene " + (anchorToScene ? "ON" : "OFF");
            hint.text = placed
                ? "M: move board   " + anchor + "   Scroll: size   Z / X: rotate   C: camera   F: mirror"
                : "Click to place the board anywhere   Scroll: size   Z / X: rotate   C: camera   F: mirror";
        }

        // Moves the board with the picture: whatever the camera did between two frames, the board does too.
        private void FollowScene()
        {
            if (webcam.videoRotationAngle == 90 || webcam.videoRotationAngle == 270) return; // desktop cameras are upright
            if (!tracker.Step(webcam, mirrored, webcam.videoVerticallyMirrored, out float a, out float b, out Vector2 t)) return;

            // The tracker works in pixels of its 160-wide frame; the webcam picture is shown at the background's size.
            float k = background.rectTransform.rect.width / WebcamMotionTracker.Width;
            if (k < 0.01f) return;

            Vector2 p = boardRoot.anchoredPosition / k;
            var q = new Vector2(a * p.x - b * p.y + t.x, b * p.x + a * p.y + t.y);
            boardRoot.anchoredPosition = q * k;
            scale = Mathf.Clamp(scale * Mathf.Sqrt(a * a + b * b), 0.3f, 1.6f);
            angle += Mathf.Atan2(b, a) * Mathf.Rad2Deg;
        }

        private void Place()
        {
            if (placed) return;
            placed = true;
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
            CameraFollow2D.OverrideScreenHeight = 0;
            if (gameCamera != null) gameCamera.targetTexture = null;
            if (texture != null) { texture.Release(); Destroy(texture); }
            StopWebcam();
            tracker.Dispose();
            ReleasePause();
        }
    }
}
