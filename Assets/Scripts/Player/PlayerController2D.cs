using UnityEngine;
using UnityEngine.InputSystem;

namespace LucillesJourney.Player
{
    /// <summary>
    /// Top-down 2D movement using the Input System.
    /// The input actions are built in code so there is no Input Actions asset to wire up.
    /// Movement can be switched off (e.g. during dialogue) via SetMovementEnabled.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 3f;

        private Rigidbody2D body;
        private InputAction moveAction;
        private Vector2 moveInput;

        /// <summary>Last non-zero movement direction. Useful for sprite facing and interaction aim.</summary>
        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        public bool MovementEnabled { get; private set; } = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;

            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");
        }

        private void OnEnable() => moveAction.Enable();

        private void OnDisable()
        {
            moveAction.Disable();
            moveInput = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnDestroy() => moveAction?.Dispose();

        private void Update()
        {
            moveInput = MovementEnabled ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f) : Vector2.zero;
            if (moveInput.sqrMagnitude > 0.01f) FacingDirection = moveInput.normalized;
        }

        private void FixedUpdate()
        {
            // linearVelocity replaces the deprecated Rigidbody2D.velocity in Unity 6.
            body.linearVelocity = moveInput * moveSpeed;
        }

        public void SetMovementEnabled(bool enabled)
        {
            MovementEnabled = enabled;
            if (!enabled) moveInput = Vector2.zero;
        }
    }
}