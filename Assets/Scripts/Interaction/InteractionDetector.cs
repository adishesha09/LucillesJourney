using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using LucillesJourney.Core;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// Sits on the Player. Finds the nearest valid Interactable in range, checks there is no wall in between,
    /// and activates it with the Interact key (E / Space / gamepad A) or by clicking/tapping it.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.25f;
        [SerializeField] private LayerMask interactableMask;
        [Tooltip("Layers that block interaction (walls). Prevents interacting through walls.")]
        [SerializeField] private LayerMask obstructionMask;

        private readonly Collider2D[] overlapBuffer = new Collider2D[16];
        private InputAction interactAction;
        private InputAction pointerPressAction;
        private Camera mainCamera;

        /// <summary>
        /// Set by the AR tabletop so a screen tap can be mapped onto the 2D world (returns null for a miss).
        /// Stays null on a normal flat-screen build, where the main camera is used.
        /// </summary>
        public static Func<Vector2, Vector2?> ScreenToWorldOverride;

        /// <summary>The Interactable currently highlighted, or null.</summary>
        public Interactable Current { get; private set; }

        public bool InteractionEnabled { get; private set; } = true;

        /// <summary>Raised whenever the focused interactable changes (including to null).</summary>
        public event Action<Interactable> FocusChanged;

        /// <summary>Used by the editor scene builder to assign layer masks created at build time.</summary>
        public void Configure(LayerMask interactable, LayerMask obstruction, float radius)
        {
            interactableMask = interactable;
            obstructionMask = obstruction;
            interactionRadius = radius;
        }

        private void Awake()
        {
            mainCamera = Camera.main;

            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            interactAction.AddBinding("<Keyboard>/space");
            interactAction.AddBinding("<Gamepad>/buttonSouth");

            // <Pointer>/press covers both mouse left button and touch.
            pointerPressAction = new InputAction("PointerPress", InputActionType.Button);
            pointerPressAction.AddBinding("<Pointer>/press");
        }

        private void OnEnable()
        {
            interactAction.performed += OnInteractPerformed;
            pointerPressAction.performed += OnPointerPerformed;
            interactAction.Enable();
            pointerPressAction.Enable();
        }

        private void OnDisable()
        {
            interactAction.performed -= OnInteractPerformed;
            pointerPressAction.performed -= OnPointerPerformed;
            interactAction.Disable();
            pointerPressAction.Disable();
            SetCurrent(null);
        }

        private void OnDestroy()
        {
            interactAction?.Dispose();
            pointerPressAction?.Dispose();
        }

        private void Update()
        {
            SetCurrent(InteractionEnabled ? FindBestInteractable() : null);
        }

        /// <summary>Disable during dialogue, pause, or cutscenes.</summary>
        public void SetInteractionEnabled(bool value)
        {
            InteractionEnabled = value;
            if (!value) SetCurrent(null);
        }

        private Interactable FindBestInteractable()
        {
            Vector2 origin = transform.position;
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = interactableMask, useTriggers = true };
            int count = Physics2D.OverlapCircle(origin, interactionRadius, filter, overlapBuffer);

            Interactable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var candidate = overlapBuffer[i].GetComponentInParent<Interactable>();
                if (candidate == null || !candidate.CanInteract) continue;
                if (!HasClearLine(origin, overlapBuffer[i])) continue;

                float distance = Vector2.Distance(origin, overlapBuffer[i].ClosestPoint(origin));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
            return best;
        }

        private bool HasClearLine(Vector2 origin, Collider2D target)
        {
            Vector2 targetPoint = target.ClosestPoint(origin);
            return Physics2D.Linecast(origin, targetPoint, obstructionMask).collider == null;
        }

        private void SetCurrent(Interactable next)
        {
            if (next == Current) return;
            Current = next;
            FocusChanged?.Invoke(Current);
        }

        private void OnInteractPerformed(InputAction.CallbackContext _)
        {
            if (!InteractionEnabled || Current == null || GameManager.Instance.IsPaused) return;
            Current.Interact(gameObject);
        }

        private void OnPointerPerformed(InputAction.CallbackContext _)
        {
            if (!InteractionEnabled || GameManager.Instance.IsPaused) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // clicked UI
            if (Pointer.current == null) return;
            Vector2 screen = Pointer.current.position.ReadValue();

            Vector2 world;
            if (ScreenToWorldOverride != null)
            {
                // AR tabletop: the game is shown on a board, so the tap has to be mapped through it.
                var mapped = ScreenToWorldOverride(screen);
                if (mapped == null) return;
                world = mapped.Value;
            }
            else
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera == null) return;
                world = mainCamera.ScreenToWorldPoint(screen);
            }

            var hit = Physics2D.OverlapPoint(world, interactableMask);
            if (hit == null) return;

            var clicked = hit.GetComponentInParent<Interactable>();
            // Only allow clicks on the object we are already close enough to use (same range + wall rules).
            if (clicked != null && clicked == Current) clicked.Interact(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}