using UnityEngine;
using LucillesJourney.Player;

namespace LucillesJourney.World
{
    /// <summary>
    /// Drives Lucille's sprite from her movement: four facing directions, an idle pose and a four-frame walk cycle,
    /// loaded by name from Resources/World (lucille_down_idle, lucille_down_walk0 ... lucille_right_walk3). Falls back
    /// to the existing sprite if the art is missing, so the player is never invisible.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(PlayerController2D), typeof(Rigidbody2D))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        private static readonly string[] Directions = { "down", "up", "left", "right" };

        private SpriteRenderer spriteRenderer;
        private PlayerController2D controller;
        private Rigidbody2D body;
        private readonly Sprite[][] idle = new Sprite[4][];
        private readonly Sprite[][] walk = new Sprite[4][];
        private bool hasArt;
        private int direction;
        private float walkTimer;
        private int walkFrame;

        private const float StepSeconds = 0.13f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController2D>();
            body = GetComponent<Rigidbody2D>();

            for (int d = 0; d < 4; d++)
            {
                idle[d] = new[] { Resources.Load<Sprite>($"World/lucille_{Directions[d]}_idle") };
                walk[d] = new Sprite[4];
                for (int f = 0; f < 4; f++) walk[d][f] = Resources.Load<Sprite>($"World/lucille_{Directions[d]}_walk{f}");
            }
            hasArt = idle[0][0] != null;
        }

        private void Update()
        {
            if (!hasArt) return;

            var velocity = body.linearVelocity;
            bool moving = controller.MovementEnabled && velocity.sqrMagnitude > 0.2f;

            Vector2 facing = moving ? velocity : controller.FacingDirection;
            if (facing.sqrMagnitude > 0.01f)
                direction = Mathf.Abs(facing.x) > Mathf.Abs(facing.y) ? (facing.x < 0f ? 2 : 3) : (facing.y < 0f ? 0 : 1);

            if (moving)
            {
                walkTimer += Time.deltaTime;
                if (walkTimer >= StepSeconds)
                {
                    walkTimer -= StepSeconds;
                    walkFrame = (walkFrame + 1) % 4;
                }
                spriteRenderer.sprite = walk[direction][walkFrame] ?? idle[direction][0];
            }
            else
            {
                walkTimer = 0f;
                walkFrame = 0;
                spriteRenderer.sprite = idle[direction][0];
            }
        }
    }
}
