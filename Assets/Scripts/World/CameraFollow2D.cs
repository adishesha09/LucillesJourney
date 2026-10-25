using UnityEngine;

namespace LucillesJourney.World
{
    /// <summary>
    /// Follows a target inside a rectangle of world space and keeps the pixel art crisp: the camera always uses a
    /// whole-number zoom (screen pixels per art pixel) and snaps to the art-pixel grid, so nothing shimmers or
    /// blurs. At 1080p the zoom is 5, showing a 384 x 216 art-pixel window.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Rect worldBounds = new Rect(-100, -100, 200, 200);
        [SerializeField] private float pixelsPerUnit = 16f;
        [SerializeField] private int referenceHeightPixels = 216;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private Vector2 focusOffset = new Vector2(0f, 0.75f);

        /// <summary>Automated tests set this so the zoom does not depend on the editor's game-window size.</summary>
        public static int OverrideScreenHeight;

        private Camera cam;
        private Vector2 position;
        private bool initialised;

        public void Configure(Transform follow, Rect bounds, float ppu = 16f)
        {
            target = follow;
            worldBounds = bounds;
            pixelsPerUnit = ppu;
        }

        private void Awake() => cam = GetComponent<Camera>();

        private void LateUpdate()
        {
            ApplyZoom();
            if (target == null) return;

            Vector2 desired = (Vector2)target.position + focusOffset;
            if (!initialised)
            {
                position = desired;
                initialised = true;
            }
            else
            {
                float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
                position = Vector2.Lerp(position, desired, t);
            }

            position = Clamp(position);
            float step = 1f / pixelsPerUnit;
            transform.position = new Vector3(Mathf.Round(position.x / step) * step, Mathf.Round(position.y / step) * step, transform.position.z);
        }

        private void ApplyZoom()
        {
            int screenHeight = OverrideScreenHeight > 0 ? OverrideScreenHeight : Screen.height;
            int zoom = Mathf.Max(1, screenHeight / referenceHeightPixels);
            cam.orthographicSize = screenHeight / (2f * zoom * pixelsPerUnit);
        }

        private Vector2 Clamp(Vector2 p)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            // If the map is smaller than the view, centre it instead.
            p.x = worldBounds.width <= halfW * 2f ? worldBounds.center.x : Mathf.Clamp(p.x, worldBounds.xMin + halfW, worldBounds.xMax - halfW);
            p.y = worldBounds.height <= halfH * 2f ? worldBounds.center.y : Mathf.Clamp(p.y, worldBounds.yMin + halfH, worldBounds.yMax - halfH);
            return p;
        }

        /// <summary>Jumps straight to the target (after a teleport or scene start).</summary>
        public void Snap() => initialised = false;
    }
}
