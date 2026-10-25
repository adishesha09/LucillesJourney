using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Gamification;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Top-level entry point for generating the whole prototype: placeholder art, narrative/
    /// chapter data assets, all seven scenes (Main Menu, six chapters, Ending) and the build
    /// settings scene list. Run via the menu, or in batch mode with
    /// -executeMethod LucillesJourney.EditorTools.GameContentBuilder.BuildAll
    ///
    /// Chapter scenes are generated from JourneyCatalog (objectives, memories, keepsakes, multi-step
    /// tasks) plus the position/art table below, so the catalog stays the single source of truth.
    /// </summary>
    public static class GameContentBuilder
    {
        private static readonly Color Green = new Color32(0x6F, 0xA8, 0x6B, 255);
        private static readonly Color Orange = new Color32(0xD9, 0x8F, 0x4B, 255);
        private static readonly Color Blue = new Color32(0x6B, 0x8F, 0xA8, 255);
        private static readonly Color Terracotta = new Color32(0xC0, 0x5A, 0x4A, 255);
        private static readonly Color Teal = new Color32(0x4B, 0x8F, 0x8A, 255);

        private struct Spot
        {
            public Vector2 Position;
            public Color Color;
            public string Sprite, DoneSprite, StateKey;
            public bool HideWhenDone;
            public float Scale;
        }

        private static Spot At(float x, float y, Color? color = null, string sprite = null, string doneSprite = null,
            string stateKey = null, bool hide = false, float scale = 0f) =>
            new Spot
            {
                Position = new Vector2(x, y), Color = color ?? Color.white, Sprite = sprite, DoneSprite = doneSprite,
                StateKey = stateKey, HideWhenDone = hide, Scale = scale
            };

        // Where each interactable sits in its room, and how it looks before/after (rooms are 20 x 14 units).
        private static readonly Dictionary<string, Spot> Layout = new Dictionary<string, Spot>
        {
            // Chapter 1 - Ordinary Life
            { "ch1_garden", At(-6, 3, Green, "world_planter_dry", "world_planter_watered", "ch1_garden_watered", scale: 2f) },
            { "ch1_breakfast", At(0, -3, Orange, "world_plate_full", "world_plate_empty", "ch1_breakfast_shared", scale: 2f) },
            { "ch1_photo", At(6, 3) },
            { "ch1_shelf", At(8, -3) },
            { "ch1_ks_wateringcan", At(-8, -3, sprite: "ks_watering_can", scale: 1.25f) },

            // Chapter 2 - The Market
            { "ch2_stall", At(-6, 2, Green) },
            { "ch2_greet", At(6, 2, Orange) },
            { "ch2_rumor", At(0, -3) },
            { "ch2_bread", At(-8, -4) },
            { "ch2_ks_basket", At(8, -4, sprite: "ks_basket", scale: 1.25f) },

            // Chapter 3 - Bombing and Hiding
            { "ch3_alarm", At(-5, 3, Terracotta) },
            { "ch3_shelter", At(5, 3, Blue) },
            { "ch3_wait", At(0, -3) },
            { "ch3_counting", At(-8, -3) },
            { "ch3_ks_blanket", At(8, -3, sprite: "ks_blanket", scale: 1.25f) },

            // Chapter 4 - Displacement: three items are packed (and disappear from the room), then the way out unlocks
            { "ch4_pack_documents", At(-6, 3, sprite: "world_doc", stateKey: "ch4_documents_packed", hide: true, scale: 1.5f) },
            { "ch4_pack_food", At(-3, 5, sprite: "world_food", stateKey: "ch4_food_packed", hide: true, scale: 1.5f) },
            { "ch4_pack_photo", At(0, 3, sprite: "world_frame", stateKey: "ch4_photo_packed", hide: true, scale: 1.5f) },
            { "ch4_depart", At(5, -3, Teal) },
            { "ch4_farewell", At(7, 3) },
            { "ch4_shelf", At(-8, -3) },
            { "ch4_ks_clothbag", At(8, -5, sprite: "ks_bag", scale: 1.25f) },

            // Chapter 5 - Survival
            { "ch5_water", At(-6, 2, Blue) },
            { "ch5_shelter", At(0, -3, Teal) },
            { "ch5_share", At(6, 2) },
            { "ch5_neighbours", At(8, -4) },
            { "ch5_ks_jug", At(-8, -4, sprite: "ks_jug", scale: 1.25f) },

            // Chapter 6 - A Second Chance (the decision itself is placed by the choice content below)
            { "ch6_settle", At(-6, 3, Teal) },
            { "ch6_work", At(6, 3, Orange) },
            { "ch6_lock", At(-8, -3) },
            { "ch6_window", At(8, -3) },
            { "ch6_ks_key", At(-3, 5, sprite: "ks_key", scale: 1.25f) },
        };

        [MenuItem("Lucille's Journey/Build All")]
        public static void BuildAll()
        {
            AssetDatabase.Refresh(); // import freshly generated art/layout files (and apply their importers) before scenes reference them

            Debug.Log("Lucille's Journey: building placeholder art...");
            PlaceholderArtBuilder.EnsureAll();

            Debug.Log("Lucille's Journey: building narrative + chapter data...");
            NarrativeContentBuilder.BuildAll();

            var allChapterIds = JourneyCatalog.Chapters.Select(c => c.Id).ToArray();

            Debug.Log("Lucille's Journey: building Main Menu scene...");
            MainMenuSceneBuilder.Build(allChapterIds);

            foreach (var chapterId in allChapterIds)
            {
                Debug.Log($"Lucille's Journey: building {chapterId}...");
                BuildChapter(chapterId);
            }

            Debug.Log("Lucille's Journey: building Ending scene...");
            EndingSceneBuilder.Build();

            Debug.Log("Lucille's Journey: configuring build settings scene list...");
            ConfigureBuildSettings(allChapterIds);

            Debug.Log("Lucille's Journey: repairing UI layout groups (second pass)...");
            LayoutRepairTool.RepairAll();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Lucille's Journey: BuildAll complete.");
        }

        private static void BuildChapter(ChapterId chapterId)
        {
            var placements = new List<InteractablePlacement>();
            foreach (var o in JourneyCatalog.Objectives.Where(x => x.Chapter == chapterId))
            {
                // The packing objective is completed by its three steps; the decision has its own choice object.
                if (o.HasSteps || o.Kind == ObjectiveKind.Decision) continue;

                if (!Layout.TryGetValue(o.Id, out var spot))
                {
                    Debug.LogError($"GameContentBuilder: no layout entry for objective '{o.Id}'.");
                    continue;
                }

                placements.Add(new InteractablePlacement
                {
                    position = spot.Position,
                    objectiveId = o.Id,
                    prompt = o.Prompt,
                    narrativeName = o.SequenceName,
                    isMemory = o.Kind == ObjectiveKind.Memory,
                    color = spot.Color,
                    spriteName = spot.Sprite,
                    doneSpriteName = spot.DoneSprite,
                    hideWhenDone = spot.HideWhenDone,
                    stateKey = spot.StateKey,
                    scale = spot.Scale,
                });
            }

            ChoiceContent? choice = null;
            if (chapterId == ChapterId.SecondChance)
            {
                choice = new ChoiceContent
                {
                    position = new Vector2(0, -3),
                    objectiveId = "ch6_decision",
                    prompt = "Decide",
                    choicePrompt = "What does Lucille do with what little money she has?",
                    framingName = "Ch6_Framing",
                    optionSpecs = NarrativeContentBuilder.Ch6OptionSpecs,
                };
            }

            ChapterSceneBuilder.Build(chapterId, placements.ToArray(), choice);
        }

        private static void ConfigureBuildSettings(ChapterId[] chapterIds)
        {
            var chapters = chapterIds
                .Select(id => AssetDatabase.LoadAssetAtPath<ChapterDataSO>($"Assets/Data/Chapters/Chapter_{id}.asset"))
                .Where(c => c != null)
                .ToArray();

            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true)
            };
            foreach (var chapter in chapters)
                scenes.Add(new EditorBuildSettingsScene($"Assets/Scenes/{chapter.sceneName}.unity", true));
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Ending.unity", true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
