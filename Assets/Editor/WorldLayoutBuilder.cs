using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Interaction;
using LucillesJourney.Player;
using LucillesJourney.World;

namespace LucillesJourney.EditorTools
{
    [Serializable] public class LayoutPoint { public int x, y; }

    [Serializable]
    public class LayoutProp
    {
        public string name = "";
        public string sprite = "";
        public int x, y;
        public string sort = "y";
        public string showWhen = "";
    }

    [Serializable]
    public class LayoutAnimated
    {
        public string name = "";
        public string[] frames = new string[0];
        public float fps = 2f;
        public int x, y;
        public string sort = "y";
        public string showWhen = "";
    }

    [Serializable] public class LayoutCollider { public int x, y, w, h; }

    [Serializable]
    public class LayoutInteractable
    {
        public string id = "";
        public int x, y;
        public int radius = 12;
        public int markerX, markerY;
    }

    /// <summary>The data produced by Tools/WorldArt: one chapter's map, props, colliders and interaction spots.</summary>
    [Serializable]
    public class WorldLayout
    {
        public int ppu = 16;
        public int widthPx, heightPx;
        public string ground = "";
        public LayoutPoint playerStart = new LayoutPoint();
        public LayoutProp[] props = new LayoutProp[0];
        public LayoutAnimated[] animated = new LayoutAnimated[0];
        public LayoutCollider[] colliders = new LayoutCollider[0];
        public LayoutInteractable[] interactables = new LayoutInteractable[0];
    }

    /// <summary>
    /// Builds a chapter's overworld from a generated layout file: baked ground, y-sorted props and animated props,
    /// wall and furniture colliders, the animated player, the follow camera, and the interaction spots wired to the
    /// existing objective pipeline. Chapters without a layout file keep the original placeholder room.
    /// </summary>
    public static class WorldLayoutBuilder
    {
        // The 2D renderer ignores the camera's custom sort axis, so draw order is explicit (see WorldDepthSort):
        // props are ordered by their base, Lucille re-sorts against them as she moves, and only the sky-level effects
        // (smoke, butterflies) and the objective markers are always above her.
        private const int GroundOrder = WorldDepthSort.GroundOrder, FloorOrder = WorldDepthSort.FloorOrder,
            AboveOrder = WorldDepthSort.AboveOrder, MarkerOrder = WorldDepthSort.MarkerOrder;

        public static string LayoutPath(ChapterId chapter) => $"Assets/Resources/World/ch{(int)chapter + 1}_layout.json";

        public static bool HasLayout(ChapterId chapter) => AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath(chapter)) != null;

        private static Sprite LoadSprite(string name) =>
            string.IsNullOrEmpty(name) ? null : AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/World/{name}.png");

        private static Vector3 ToWorld(float xPx, float yPx, int ppu) => new Vector3(xPx / ppu, yPx / ppu, 0f);

        public static int OrderFor(string sort, float baselineYPx)
        {
            switch (sort)
            {
                case "floor": return FloorOrder;
                case "above": return AboveOrder;
                default: return WorldDepthSort.OrderForBaseline(baselineYPx);
            }
        }

        /// <summary>Returns the player GameObject, or null if the chapter has no layout.</summary>
        public static GameObject Build(ChapterId chapter, GameObject cameraGo, InteractablePlacement[] placements)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath(chapter));
            if (asset == null) return null;

            var layout = JsonUtility.FromJson<WorldLayout>(asset.text);
            int ppu = layout.ppu > 0 ? layout.ppu : 16;
            var root = new GameObject("World");

            // Ground
            var ground = new GameObject("Ground", typeof(SpriteRenderer));
            ground.transform.SetParent(root.transform, false);
            var groundRenderer = ground.GetComponent<SpriteRenderer>();
            groundRenderer.sprite = LoadSprite(layout.ground);
            groundRenderer.sortingOrder = GroundOrder;
            if (groundRenderer.sprite == null) Debug.LogError($"WorldLayoutBuilder: missing ground sprite '{layout.ground}'.");

            // Props and animated props
            var propsRoot = new GameObject("Props").transform;
            propsRoot.SetParent(root.transform, false);
            foreach (var p in layout.props) CreateProp(propsRoot, p.name, new[] { p.sprite }, 1f, p.x, p.y, p.sort, p.showWhen, ppu);
            foreach (var a in layout.animated) CreateProp(propsRoot, a.name, a.frames, a.fps, a.x, a.y, a.sort, a.showWhen, ppu);

            // Colliders (walls, furniture footprints, fences, trunks)
            var collidersRoot = new GameObject("Colliders").transform;
            collidersRoot.SetParent(root.transform, false);
            int wallLayer = LayerMask.NameToLayer("Walls");
            int index = 0;
            foreach (var c in layout.colliders)
            {
                var go = new GameObject($"Collider_{index++}", typeof(BoxCollider2D));
                go.transform.SetParent(collidersRoot, false);
                go.transform.position = ToWorld(c.x + c.w / 2f, c.y + c.h / 2f, ppu);
                go.GetComponent<BoxCollider2D>().size = new Vector2(c.w, c.h) / ppu;
                go.isStatic = true;
                if (wallLayer >= 0) go.layer = wallLayer;
            }

            // Player and camera
            var player = BuildPlayer(layout, ppu);
            var follow = cameraGo.AddComponent<CameraFollow2D>();
            var bounds = new Rect(0f, 0f, layout.widthPx / (float)ppu, layout.heightPx / (float)ppu);
            follow.Configure(player.transform, bounds, ppu);
            var cam = cameraGo.GetComponent<Camera>();
            cam.transparencySortMode = TransparencySortMode.CustomAxis;
            cam.transparencySortAxis = Vector3.up;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(0x1A, 0x22, 0x24, 255);
            cameraGo.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);

            // Interaction spots
            var interactablesRoot = new GameObject("Interactables").transform;
            interactablesRoot.SetParent(root.transform, false);
            foreach (var spot in layout.interactables)
            {
                var placement = placements.FirstOrDefault(p => p.objectiveId == spot.id);
                if (string.IsNullOrEmpty(placement.objectiveId))
                {
                    Debug.LogWarning($"WorldLayoutBuilder: layout interactable '{spot.id}' has no matching placement; skipped.");
                    continue;
                }
                CreateInteractable(interactablesRoot, spot, placement, ppu);
            }

            foreach (var p in placements)
                if (!layout.interactables.Any(s => s.id == p.objectiveId))
                    Debug.LogError($"WorldLayoutBuilder: objective '{p.objectiveId}' has no interaction spot in the layout, so it cannot be completed.");

            return player;
        }

        private static void CreateProp(Transform parent, string name, string[] frameNames, float fps, int x, int y, string sort, string showWhen, int ppu)
        {
            var sprites = frameNames.Select(LoadSprite).Where(s => s != null).ToArray();
            if (sprites.Length == 0)
            {
                Debug.LogWarning($"WorldLayoutBuilder: prop '{name}' has no sprites ({string.Join(",", frameNames)}).");
                return;
            }

            var go = new GameObject(string.IsNullOrEmpty(name) ? sprites[0].name : name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.position = ToWorld(x, y, ppu);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprites[0];
            renderer.sortingOrder = OrderFor(sort, y);

            if (sprites.Length > 1)
            {
                var animator = go.AddComponent<SpriteFrameAnimator>();
                animator.Configure(sprites, fps, (x * 7 + y * 13) % 10 * 0.1f);
            }
            if (!string.IsNullOrEmpty(showWhen)) go.AddComponent<EnvironmentStateGate>().Configure(showWhen);
        }

        private static GameObject BuildPlayer(WorldLayout layout, int ppu)
        {
            var go = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            go.transform.position = ToWorld(layout.playerStart.x, layout.playerStart.y, ppu);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite("lucille_down_idle") ?? PlaceholderArtBuilder.PlayerBlob;
            renderer.sortingOrder = WorldDepthSort.OrderForPlayer(layout.playerStart.y);

            var body = go.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // The collider is a small circle at the feet, so Lucille can walk close to furniture and her head may overlap it.
            var collider = go.GetComponent<CircleCollider2D>();
            collider.radius = 5f / ppu;
            collider.offset = new Vector2(0f, 5f / ppu);

            go.AddComponent<PlayerController2D>();
            go.AddComponent<PlayerSpriteAnimator>();
            go.AddComponent<DepthSortedCharacter>().Configure(ppu);
            var detector = go.AddComponent<InteractionDetector>();
            detector.Configure(LayerMask.GetMask("Interactable"), LayerMask.GetMask("Walls"), 1.5f);
            return go;
        }

        private static void CreateInteractable(Transform parent, LayoutInteractable spot, InteractablePlacement p, int ppu)
        {
            var go = new GameObject($"Interactable_{p.objectiveId}", typeof(CircleCollider2D));
            go.transform.SetParent(parent, false);
            go.transform.position = ToWorld(spot.x, spot.y, ppu);
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = Mathf.Max(6, spot.radius) / (float)ppu;

            var interactable = go.AddComponent<Interactable>();
            interactable.Configure(p.objectiveId, p.prompt);

            var narrative = go.AddComponent<NarrativeInteractable>();
            narrative.Configure(NarrativeContentBuilder.Load(p.narrativeName), p.isMemory);

            // Completing the objective records a persistent flag the props use to change (dry beds -> watered beds, ...).
            var visual = go.AddComponent<EnvironmentStateVisual>();
            visual.Configure(string.IsNullOrEmpty(p.stateKey) ? $"{p.objectiveId}_done" : p.stateKey, p.objectiveId, null, false, Color.white);

            go.AddComponent<InteractionPrerequisite>();

            // Floating sparkle over the object, until the objective is done.
            var markerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/JourneyUI/world_pending.png");
            if (markerSprite != null)
            {
                var marker = new GameObject("Marker", typeof(SpriteRenderer));
                marker.transform.SetParent(go.transform, false);
                marker.transform.position = ToWorld(spot.markerX != 0 || spot.markerY != 0 ? spot.markerX : spot.x,
                    spot.markerX != 0 || spot.markerY != 0 ? spot.markerY : spot.y + 20, ppu);
                var renderer = marker.GetComponent<SpriteRenderer>();
                renderer.sprite = markerSprite;
                renderer.sortingOrder = MarkerOrder;
                marker.AddComponent<ObjectiveMarker>().Configure(p.objectiveId);
            }
        }
    }
}
