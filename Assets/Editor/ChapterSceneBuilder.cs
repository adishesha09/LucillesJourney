using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using LucillesJourney.Core;
using LucillesJourney.Interaction;
using LucillesJourney.Narrative;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>A single interactable object to place in a chapter scene.</summary>
    public struct InteractablePlacement
    {
        public Vector2 position;
        public string objectiveId;
        public string prompt;

        /// <summary>Narrative sequence asset name (e.g. "Ch1_Garden"), resolved fresh at build time.</summary>
        public string narrativeName;
        public bool isMemory;
        public Color color;

        /// <summary>Resources/JourneyUI sprite shown before the interaction (empty = default placeholder marker).</summary>
        public string spriteName;

        /// <summary>Sprite shown after the interaction (empty = keep the sprite, just dim it).</summary>
        public string doneSpriteName;

        /// <summary>Hide the object once its objective is complete (e.g. an item that has been packed).</summary>
        public bool hideWhenDone;

        /// <summary>Persistent environment-state key recorded when the objective completes.</summary>
        public string stateKey;

        public float scale;
    }

    /// <summary>Optional Chapter 6 "impossible choice" content for a scene.</summary>
    public struct ChoiceContent
    {
        public Vector2 position;
        public string objectiveId;
        public string prompt;
        public string choicePrompt;

        /// <summary>Framing narrative sequence asset name, resolved fresh at build time.</summary>
        public string framingName;
        public (string id, string label, string consequenceName)[] optionSpecs;
    }

    /// <summary>
    /// Builds one complete, playable chapter scene: environment, player, interactables, HUD,
    /// narrative panel, pause/settings/guide menus and the ChapterManager tying it together.
    /// </summary>
    public static class ChapterSceneBuilder
    {
        private static readonly Color SkyColor = new Color32(0x1A, 0x22, 0x24, 255);
        private static readonly Color PlayerColor = new Color32(0xE3, 0xB5, 0x7A, 255);
        private static readonly Color PromptPanelColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color TextColor = new Color32(0xF4, 0xEA, 0xD2, 255);
        private static readonly Color MemoryColor = new Color32(0xF2, 0xD2, 0x6B, 255);

        private static readonly Vector2 RoomSize = new Vector2(20f, 14f);

        public static void Build(ChapterId chapterId, InteractablePlacement[] placements, ChoiceContent? choice = null)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Resolve the chapter data fresh, after NewScene, since that call can invalidate any
            // ScriptableObject reference obtained beforehand within the same batch-mode process.
            var data = AssetDatabase.LoadAssetAtPath<ChapterDataSO>($"Assets/Data/Chapters/Chapter_{chapterId}.asset");
            if (data == null)
            {
                Debug.LogError($"ChapterSceneBuilder: could not load ChapterDataSO for {chapterId}.");
                return;
            }

            var camera = SceneBuildUtility.CreateCamera(SkyColor, 7.5f);
            SceneBuildUtility.CreateGlobalLight();
            SceneBuildUtility.CreateEventSystem();

            // Chapters with a generated overworld layout get the full world; the rest keep the original placeholder room.
            var player = WorldLayoutBuilder.Build(chapterId, camera, placements);
            if (player == null)
            {
                BuildEnvironment();
                player = BuildPlayer();

                foreach (var p in placements) BuildInteractable(p);
                BuildProps(chapterId);
            }
            if (choice.HasValue) BuildChoiceInteractable(choice.Value);

            var canvasRoot = SceneBuildUtility.CreateCanvas("HUDCanvas").gameObject;

            var hud = HUDBuilder.Build(canvasRoot.transform);
            var narrativePanel = NarrativePanelBuilder.Build(canvasRoot.transform);
            BuildInteractionPrompt(canvasRoot.transform, player);

            var settingsMenu = SettingsPanelBuilder.Build(canvasRoot.transform);
            var guidePanel = InteractionGuidePanelBuilder.Build(canvasRoot.transform);
            PauseMenuBuilder.Build(canvasRoot.transform, settingsMenu, guidePanel);

            if (choice.HasValue) ChoiceUIBuilder.Build(canvasRoot.transform);

            var transition = BuildTransitionFade(canvasRoot.transform);

            var managerGO = new GameObject("ChapterManager");
            var manager = managerGO.AddComponent<ChapterManager>();
            manager.Wire(data, hud, transition);

            EditorSceneManager.MarkSceneDirty(scene);
            string path = $"Assets/Scenes/{data.sceneName}.unity";
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void BuildEnvironment()
        {
            var floor = new GameObject("Floor", typeof(SpriteRenderer));
            var floorRenderer = floor.GetComponent<SpriteRenderer>();
            floorRenderer.sprite = PlaceholderArtBuilder.FloorTile;
            floorRenderer.drawMode = SpriteDrawMode.Tiled;
            floorRenderer.size = RoomSize;
            floorRenderer.sortingOrder = -10;

            int wallLayer = LayerMask.NameToLayer("Walls");
            BuildWall("Wall_Top", new Vector2(0f, RoomSize.y / 2f + 0.5f), new Vector2(RoomSize.x + 2f, 1f), wallLayer);
            BuildWall("Wall_Bottom", new Vector2(0f, -RoomSize.y / 2f - 0.5f), new Vector2(RoomSize.x + 2f, 1f), wallLayer);
            BuildWall("Wall_Left", new Vector2(-RoomSize.x / 2f - 0.5f, 0f), new Vector2(1f, RoomSize.y), wallLayer);
            BuildWall("Wall_Right", new Vector2(RoomSize.x / 2f + 0.5f, 0f), new Vector2(1f, RoomSize.y), wallLayer);
        }

        private static void BuildWall(string name, Vector2 position, Vector2 size, int layer)
        {
            var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.position = position;
            if (layer >= 0) go.layer = layer;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderArtBuilder.WallTile;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = size;
            renderer.sortingOrder = -5;

            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static GameObject BuildPlayer()
        {
            var go = new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            go.transform.position = Vector3.zero;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderArtBuilder.PlayerBlob;
            renderer.color = PlayerColor;
            renderer.sortingOrder = 0;

            var body = go.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.radius = 0.4f;

            go.AddComponent<LucillesJourney.Player.PlayerController2D>();
            var detector = go.AddComponent<InteractionDetector>();
            detector.Configure(LayerMask.GetMask("Interactable"), LayerMask.GetMask("Walls"), 1.25f);

            return go;
        }

        private static Sprite LoadJourneySprite(string name) =>
            string.IsNullOrEmpty(name) ? null : AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/JourneyUI/{name}.png");

        private static readonly Color DoneTint = new Color(0.62f, 0.6f, 0.58f, 1f);

        private static void BuildInteractable(InteractablePlacement p)
        {
            var go = new GameObject($"Interactable_{p.objectiveId}", typeof(SpriteRenderer), typeof(CircleCollider2D));
            go.transform.position = p.position;
            if (p.scale > 0f) go.transform.localScale = Vector3.one * p.scale;
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            var customSprite = LoadJourneySprite(p.spriteName);
            // Memory objects without bespoke art use the pixel sparkle marker (and its dimmed "found" version).
            string pendingName = p.spriteName, doneName = p.doneSpriteName;
            if (customSprite == null && p.isMemory)
            {
                pendingName = "world_pending";
                doneName = "world_done";
                customSprite = LoadJourneySprite(pendingName);
                if (p.scale <= 0f) go.transform.localScale = Vector3.one * 1.5f;
            }
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = customSprite != null ? customSprite : PlaceholderArtBuilder.MarkerBlob;
            renderer.color = customSprite != null ? Color.white : p.color;
            renderer.sortingOrder = 1;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            var interactable = go.AddComponent<Interactable>();
            interactable.Configure(p.objectiveId, p.prompt);

            var narrativeInteractable = go.AddComponent<NarrativeInteractable>();
            narrativeInteractable.Configure(NarrativeContentBuilder.Load(p.narrativeName), p.isMemory);

            // Visible, persistent consequence: swap/dim the object (or hide it) once its objective is complete.
            var visual = go.AddComponent<EnvironmentStateVisual>();
            var doneSprite = LoadJourneySprite(doneName);
            var doneColor = customSprite != null && doneSprite != null ? Color.white : (customSprite != null ? DoneTint : renderer.color * DoneTint);
            visual.Configure(string.IsNullOrEmpty(p.stateKey) ? $"{p.objectiveId}_done" : p.stateKey, p.objectiveId,
                doneSprite, true, doneColor);
            if (p.hideWhenDone) visual.ConfigureHideSelf();

            go.AddComponent<InteractionPrerequisite>();
        }

        /// <summary>A non-interactive prop that changes appearance when an objective completes (e.g. the bag once packed).</summary>
        private static void BuildStateProp(string name, Vector2 position, string spriteName, string doneSpriteName, string objectiveId, string stateKey)
        {
            var go = new GameObject($"Prop_{name}", typeof(SpriteRenderer));
            go.transform.position = position;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = LoadJourneySprite(spriteName);
            renderer.sortingOrder = 0;

            var visual = go.AddComponent<EnvironmentStateVisual>();
            visual.Configure(stateKey, objectiveId, LoadJourneySprite(doneSpriteName), false, Color.white);
        }

        public static void BuildProps(ChapterId chapterId)
        {
            if (chapterId == ChapterId.Displacement)
                BuildStateProp("Bag", new Vector2(-4f, 0f), "world_bag_open", "world_bag_packed", "ch4_pack", "ch4_bag_packed");
        }

        private static void BuildChoiceInteractable(ChoiceContent c)
        {
            var go = new GameObject($"Interactable_{c.objectiveId}", typeof(SpriteRenderer), typeof(CircleCollider2D));
            go.transform.position = c.position;
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderArtBuilder.MarkerBlob;
            renderer.color = new Color32(0xC0, 0x5A, 0x4A, 255);
            renderer.sortingOrder = 1;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            var interactable = go.AddComponent<Interactable>();
            interactable.Configure(c.objectiveId, c.prompt);

            var options = c.optionSpecs.Select(spec => new ChoiceOption
            {
                id = spec.id,
                label = spec.label,
                consequence = NarrativeContentBuilder.Load(spec.consequenceName)
            }).ToArray();

            // The choice interactable also needs the catalog prerequisite (settle in and find work first).
            go.AddComponent<InteractionPrerequisite>();
            var choiceInteractable = go.AddComponent<ChoiceInteractable>();
            choiceInteractable.Configure(c.choicePrompt, NarrativeContentBuilder.Load(c.framingName), options);
        }

        private static void BuildInteractionPrompt(Transform canvasParent, GameObject player)
        {
            var root = SceneBuildUtility.CreatePanel(canvasParent, "InteractionPrompt", PromptPanelColor);
            SceneBuildUtility.Anchor(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0, 220), new Vector2(560, 60));

            var label = SceneBuildUtility.CreateText(root.transform, "Label", "[E] Interact", 26, TextColor, TextAlignmentOptions.Center);
            SceneBuildUtility.Stretch(label.gameObject);

            SceneBuildUtility.AddCanvasGroup(root);
            var prompt = root.AddComponent<InteractionPromptUI>();
            prompt.Wire(player.GetComponent<InteractionDetector>(), label);
        }

        private static ChapterTransitionUI BuildTransitionFade(Transform canvasParent)
        {
            var go = SceneBuildUtility.CreatePanel(canvasParent, "TransitionFade", Color.black);
            SceneBuildUtility.Stretch(go);
            var group = SceneBuildUtility.AddCanvasGroup(go);
            group.blocksRaycasts = false;
            go.transform.SetAsLastSibling(); // render above every other HUD element

            var fade = go.AddComponent<ChapterTransitionUI>();
            fade.Wire(group);
            return fade;
        }
    }
}
