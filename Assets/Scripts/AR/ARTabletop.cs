#if LJ_AR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using LucillesJourney.Core;
using LucillesJourney.Interaction;
using LucillesJourney.World;

namespace LucillesJourney.AR
{
    /// <summary>
    /// Tabletop AR. The normal 2D game keeps running exactly as before (physics, depth sorting, pixel snapping), but its
    /// camera renders into a small crisp RenderTexture that is shown on a board lying on a real table.
    ///
    /// Flow: scan for a flat surface -> a translucent ghost of the board follows the middle of the screen -> tap PLACE ->
    /// play. While placed, pinch to resize and twist to rotate the board; MOVE puts it back into placing mode.
    ///
    /// Drop this on any GameObject in the scene that also has an XR Origin (AR), ARPlaneManager and ARRaycastManager.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ARTabletop : MonoBehaviour
    {
        public static ARTabletop Instance { get; private set; }

        [Header("AR rig (found automatically if left empty)")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Camera arCamera;

        [Header("Board")]
        [Tooltip("Width of the board on the table, in metres, at scale 1.")]
        [SerializeField, Min(0.1f)] private float boardWidthMeters = 0.6f;
        [Tooltip("Render size of the game view. 384x216 is exactly the pixel-art window the game is designed for.")]
        [SerializeField] private Vector2Int textureSize = new Vector2Int(384, 216);
        [SerializeField, Min(0.05f)] private float minScale = 0.4f;
        [SerializeField, Min(0.1f)] private float maxScale = 3f;
        [Tooltip("Hold the game paused until the board has been placed.")]
        [SerializeField] private bool pauseUntilPlaced = true;

        private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
        private Camera gameCamera;
        private int boardLayer;
        private RenderTexture texture;
        private Transform root;
        private Transform board;
        private Material boardMaterial;
        private float userScale = 1f;
        private GameManager pauseOwner;
        private bool gestureActive;
        private float previousDistance, previousAngle;

        /// <summary>How many flat surfaces AR has found so far (shown in the on-screen hint to help with setup).</summary>
        public int PlaneCount => planeManager != null ? planeManager.trackables.count : 0;

        public bool IsPlaced { get; private set; }
        /// <summary>True while a flat surface is under the middle of the screen (placing mode only).</summary>
        public bool HasSurface { get; private set; }
        public event Action<bool> PlacedChanged;

        private void Awake()
        {
            Instance = this;

            if (raycastManager == null) raycastManager = FindFirstObjectByType<ARRaycastManager>();
            if (planeManager == null) planeManager = FindFirstObjectByType<ARPlaneManager>();
            if (arCamera == null)
            {
                var cameraManager = FindFirstObjectByType<ARCameraManager>();
                if (cameraManager != null) arCamera = cameraManager.GetComponent<Camera>();
            }
            if (raycastManager == null || planeManager == null || arCamera == null)
            {
                Debug.LogError("ARTabletop needs an XR Origin (AR) with ARPlaneManager and ARRaycastManager in the scene. Disabling.", this);
                enabled = false;
                return;
            }

            boardLayer = FindBoardLayer();
            IsolateRig();
            SetUpGameCamera();
            BuildBoard();

            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            if (GetComponent<ARTabletopUI>() == null) gameObject.AddComponent<ARTabletopUI>();
        }

        private void OnEnable() => InteractionDetector.ScreenToWorldOverride = ScreenToGameWorld;

        private void OnDisable()
        {
            if (InteractionDetector.ScreenToWorldOverride == (Func<Vector2, Vector2?>)ScreenToGameWorld)
                InteractionDetector.ScreenToWorldOverride = null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            CameraFollow2D.OverrideScreenHeight = 0;
            if (gameCamera != null) gameCamera.targetTexture = null;
            if (texture != null) { texture.Release(); Destroy(texture); }
            ReleasePause();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Setup
        // ---------------------------------------------------------------------------------------------------------

        // The AR camera draws ONLY the board (its own layer). The 2D game world is never drawn around the player - it is
        // shown only through the board. This needs no moving of the rig, so it also works inside the XR simulator.
        private void IsolateRig()
        {
            arCamera.cullingMask = 1 << boardLayer;
            arCamera.nearClipPlane = 0.01f;
            arCamera.farClipPlane = 30f;
            arCamera.tag = "MainCamera";
            if (!arCamera.TryGetComponent<AudioListener>(out _)) arCamera.gameObject.AddComponent<AudioListener>();
        }

        // Prefers a layer that has no name (nothing else uses it). The board has no colliders, so physics is unaffected.
        private static int FindBoardLayer()
        {
            for (int i = 31; i >= 8; i--)
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i))) return i;
            return 31;
        }

        private void SetUpGameCamera()
        {
            var follow = FindFirstObjectByType<CameraFollow2D>();
            if (follow == null)
            {
                Debug.LogError("ARTabletop could not find the game's CameraFollow2D camera.", this);
                enabled = false;
                return;
            }
            gameCamera = follow.GetComponent<Camera>();

            // Whole-number zoom against the texture height makes the view exactly textureSize pixels: crisp, no shimmer.
            CameraFollow2D.OverrideScreenHeight = textureSize.y;

            texture = new RenderTexture(textureSize.x, textureSize.y, 16, RenderTextureFormat.ARGB32)
            {
                name = "LucilleTabletopView",
                filterMode = FilterMode.Point,
                antiAliasing = 1,
                useMipMap = false,
            };
            texture.Create();

            gameCamera.targetTexture = texture;
            gameCamera.ResetAspect();
            gameCamera.cullingMask &= ~(1 << boardLayer);
            if (gameCamera.CompareTag("MainCamera")) gameCamera.tag = "Untagged";
            var listener = gameCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }

        private void BuildBoard()
        {
            root = new GameObject("ARTabletopRoot").transform;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            boardMaterial = new Material(shader) { mainTexture = texture };

            float height = boardWidthMeters * textureSize.y / textureSize.x;
            board = MakeQuad("Board", boardMaterial, new Vector3(boardWidthMeters, height, 1f), 0.002f);

            // A dark frame slightly bigger than the board so it reads as an object on the table.
            var frameMaterial = new Material(shader) { color = new Color(0.10f, 0.13f, 0.14f, 1f) };
            const float border = 0.015f;
            MakeQuad("Frame", frameMaterial, new Vector3(boardWidthMeters + border * 2f, height + border * 2f, 1f), 0.001f);

            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = boardLayer;

            SetGhost(true);
            root.gameObject.SetActive(false);
        }

        private Transform MakeQuad(string name, Material material, Vector3 size, float lift)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            Destroy(quad.GetComponent<Collider>());
            quad.GetComponent<MeshRenderer>().sharedMaterial = material;
            var t = quad.transform;
            t.SetParent(root, false);
            t.localRotation = Quaternion.Euler(90f, 0f, 0f); // lie flat, game "up" pointing away from the player
            t.localPosition = new Vector3(0f, lift, 0f);
            t.localScale = size;
            return t;
        }

        private void SetGhost(bool ghost)
        {
            var c = Color.white;
            c.a = ghost ? 0.6f : 1f;
            boardMaterial.color = c;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Placing
        // ---------------------------------------------------------------------------------------------------------

        private void Start()
        {
            if (enabled) ApplyPause();
        }

        private void Update()
        {
            if (IsPlaced) UpdateGestures();
            else
            {
                UpdatePlacing();
                ApplyPause();
            }
        }

        private void UpdatePlacing()
        {
            HasSurface = false;
            var centre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (raycastManager.Raycast(centre, hits, TrackableType.PlaneWithinPolygon))
            {
                foreach (var hit in hits)
                {
                    var plane = planeManager.GetPlane(hit.trackableId);
                    if (plane == null || plane.alignment != PlaneAlignment.HorizontalUp) continue;

                    HasSurface = true;
                    root.position = hit.pose.position;
                    Vector3 forward = Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up);
                    if (forward.sqrMagnitude > 0.001f) root.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
                    break;
                }
            }
            root.gameObject.SetActive(HasSurface);
        }

        /// <summary>Locks the board where the ghost is and starts the game. Returns false if there is no surface yet.</summary>
        public bool Place()
        {
            if (IsPlaced || !HasSurface) return false;
            IsPlaced = true;
            SetGhost(false);
            planeManager.SetTrackablesActive(false);
            planeManager.enabled = false;
            ReleasePause();
            PlacedChanged?.Invoke(true);
            return true;
        }

        /// <summary>Picks the board back up so it can be put somewhere else. The game pauses meanwhile.</summary>
        public void BeginMove()
        {
            if (!IsPlaced) return;
            IsPlaced = false;
            gestureActive = false;
            SetGhost(true);
            planeManager.enabled = true;
            planeManager.SetTrackablesActive(true);
            PlacedChanged?.Invoke(false);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Pinch to resize, twist to rotate
        // ---------------------------------------------------------------------------------------------------------

        private void UpdateGestures()
        {
            var screen = Touchscreen.current;
            if (screen == null) return;

            int count = 0;
            Vector2 a = default, b = default;
            bool overUi = false;
            foreach (var touch in screen.touches)
            {
                if (!touch.press.isPressed) continue;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue())) overUi = true;
                if (count == 0) a = touch.position.ReadValue();
                else if (count == 1) b = touch.position.ReadValue();
                count++;
            }

            // Exactly two fingers, neither on the joystick or a button.
            if (count != 2 || overUi) { gestureActive = false; return; }

            Vector2 delta = b - a;
            float distance = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (gestureActive && previousDistance > 1f)
            {
                userScale = Mathf.Clamp(userScale * distance / previousDistance, minScale, maxScale);
                root.localScale = Vector3.one * userScale;
                // Seen from above, a counter-clockwise twist on screen is a negative turn about Y.
                root.Rotate(0f, -Mathf.DeltaAngle(previousAngle, angle), 0f, Space.World);
            }
            previousDistance = distance;
            previousAngle = angle;
            gestureActive = true;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Tap-to-interact: map a screen tap on the board into 2D game-world coordinates
        // ---------------------------------------------------------------------------------------------------------

        private Vector2? ScreenToGameWorld(Vector2 screenPoint)
        {
            if (!IsPlaced || board == null || gameCamera == null) return null;

            Ray ray = arCamera.ScreenPointToRay(screenPoint);
            var plane = new Plane(-board.forward, board.position); // the quad's visible side faces its local -Z
            if (!plane.Raycast(ray, out float distance)) return null;

            Vector3 local = board.InverseTransformPoint(ray.GetPoint(distance));
            if (Mathf.Abs(local.x) > 0.5f || Mathf.Abs(local.y) > 0.5f) return null; // tapped outside the board

            Vector3 world = gameCamera.ViewportToWorldPoint(new Vector3(local.x + 0.5f, local.y + 0.5f, 0f));
            return new Vector2(world.x, world.y);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Pausing
        // ---------------------------------------------------------------------------------------------------------

        private void ApplyPause()
        {
            if (!pauseUntilPlaced) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.IsPaused) return;
            gm.SetPaused(true);
            pauseOwner = gm;
        }

        // Uses the reference kept from ApplyPause, never GameManager.Instance: that getter creates a new GameManager when
        // none exists, which is exactly what must not happen while the scene or Play mode is shutting down.
        private void ReleasePause()
        {
            var gm = pauseOwner;
            pauseOwner = null;
            if (gm != null && gm.IsPaused) gm.SetPaused(false);
        }
    }
}
#endif