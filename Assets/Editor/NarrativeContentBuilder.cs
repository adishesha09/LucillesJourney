using System.Linq;
using UnityEditor;
using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Narrative;
using LucillesJourney.Interaction;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Creates the data-driven narrative/objective/chapter assets for all six chapters.
    ///
    /// IMPORTANT CONTENT NOTE: every line of dialogue/narration created here is PLACEHOLDER
    /// text written only from the high-level chapter structure (Ordinary Life, The Market,
    /// Bombing and Hiding, Displacement, Survival, A Second Chance) and the "impossible choice"
    /// ending described in the team's concept document. It deliberately avoids inventing direct
    /// quotations or claiming to be Lucille's real testimony. Before submission this text MUST
    /// be reviewed and, where appropriate, replaced with material grounded in the actual concept
    /// document and The Refugee Archive's recorded testimony.
    /// </summary>
    public static class NarrativeContentBuilder
    {
        private const string NarrativeFolder = "Assets/Data/Narrative";
        private const string ChapterFolder = "Assets/Data/Chapters";

        /// <summary>
        /// Resolves a narrative sequence by name via a fresh AssetDatabase lookup. Must be used
        /// (instead of holding a reference across scene changes) anywhere a sequence is needed
        /// after EditorSceneManager.NewScene has run, since that call can invalidate previously
        /// held ScriptableObject references within the same batch-mode process.
        /// </summary>
        public static NarrativeSequenceSO Load(string name) =>
            string.IsNullOrEmpty(name) ? null : AssetDatabase.LoadAssetAtPath<NarrativeSequenceSO>($"{NarrativeFolder}/{name}.asset");

        public static ChapterDataSO[] BuildAll()
        {
            EnsureFolders();

            var ch1 = BuildChapter1();
            var ch2 = BuildChapter2();
            var ch3 = BuildChapter3();
            var ch4 = BuildChapter4();
            var ch5 = BuildChapter5();
            var ch6 = BuildChapter6();

            ch1.nextSceneName = ch2.sceneName;
            ch2.nextSceneName = ch3.sceneName;
            ch3.nextSceneName = ch4.sceneName;
            ch4.nextSceneName = ch5.sceneName;
            ch5.nextSceneName = ch6.sceneName;
            ch6.nextSceneName = "Ending";
            ch6.isFinalChapter = true;

            // Force-create every per-interactable/choice narrative sequence now, while it is
            // still safe to do so (before any scene is built). They are then resolved again by
            // name via Load() at scene-build time rather than reusing these references.
            _ = Ch1Garden; _ = Ch1Breakfast;
            _ = Ch2Stall; _ = Ch2Greet;
            _ = Ch3Alarm; _ = Ch3Shelter;
            _ = Ch4Depart;
            _ = Ch5Water; _ = Ch5Shelter;
            _ = Ch6Settle; _ = Ch6Work; _ = Ch6Framing; _ = Ch6Options;

            // Memories, keepsakes and multi-step objective steps come from JourneyCatalog (single source of
            // truth shared with the Memory Album and Journal), and each chapter's objective list mirrors the catalog.
            BuildCatalogSequences();
            foreach (var chapter in new[] { ch1, ch2, ch3, ch4, ch5, ch6 }) chapter.objectives = ObjectivesFromCatalog(chapter.chapterId);

            foreach (var c in new[] { ch1, ch2, ch3, ch4, ch5, ch6 }) EditorUtility.SetDirty(c);
            AssetDatabase.SaveAssets();

            return new[] { ch1, ch2, ch3, ch4, ch5, ch6 };
        }

        private static void BuildCatalogSequences()
        {
            foreach (var objective in LucillesJourney.Gamification.JourneyCatalog.Objectives)
            {
                if (string.IsNullOrEmpty(objective.SequenceName) || objective.Lines.Length == 0) continue;
                Seq(objective.SequenceName, objective.Lines.Select(l => (l.Speaker, l.Text)).ToArray());
            }
        }

        private static ObjectiveDefinition[] ObjectivesFromCatalog(ChapterId chapter) =>
            LucillesJourney.Gamification.JourneyCatalog.ObjectivesFor(chapter)
                .Select(o => new ObjectiveDefinition
                {
                    objectiveId = o.Id,
                    description = o.Title,
                    isOptional = o.Optional,
                    isMemoryFragment = o.Kind == LucillesJourney.Gamification.ObjectiveKind.Memory
                })
                .ToArray();

        private static void EnsureFolders()
        {
            foreach (var path in new[] { NarrativeFolder, ChapterFolder })
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'), System.IO.Path.GetFileName(path));
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static NarrativeSequenceSO Seq(string name, params (string speaker, string text)[] lines)
        {
            string path = $"{NarrativeFolder}/{name}.asset";
            var seq = CreateOrLoad<NarrativeSequenceSO>(path);
            seq.sequenceName = name;
            seq.lines = lines.Select(l => new DialogueLine { speakerName = l.speaker, text = l.text }).ToArray();
            EditorUtility.SetDirty(seq);
            return seq;
        }

        private static ChapterDataSO Chapter(ChapterId id, string displayName, string sceneName)
        {
            string path = $"{ChapterFolder}/Chapter_{id}.asset";
            var data = CreateOrLoad<ChapterDataSO>(path);
            data.chapterId = id;
            data.displayName = displayName;
            data.sceneName = sceneName;
            return data;
        }

        // --- Chapter 1: Ordinary Life -------------------------------------------------------

        private static ChapterDataSO BuildChapter1()
        {
            var data = Chapter(ChapterId.OrdinaryLife, "Chapter 1: Ordinary Life", "Chapter1_OrdinaryLife");

            data.introNarrative = Seq("Ch1_Intro",
                ("Narrator", "Before everything changed, there were ordinary mornings like this one."),
                ("Narrator", "A small home. A garden. Children's voices in the next room."));

            data.outroNarrative = Seq("Ch1_Outro",
                ("Narrator", "The day passed the way most days did - full, tiring, and unremarkable."),
                ("Narrator", "Tomorrow there would be the market to visit, as there always was."));

            data.objectives = new[]
            {
                new ObjectiveDefinition { objectiveId = "ch1_garden", description = "Water the garden" },
                new ObjectiveDefinition { objectiveId = "ch1_breakfast", description = "Share breakfast with the children" },
                new ObjectiveDefinition { objectiveId = "ch1_photo", description = "Look at the family photograph", isMemoryFragment = true },
            };
            return data;
        }

        public static NarrativeSequenceSO Ch1Garden => Seq("Ch1_Garden",
            ("Lucille", "A few more minutes of watering, and these will make it through the week."),
            ("Narrator", "Small, steady work. The kind that makes a house feel like a home."));

        public static NarrativeSequenceSO Ch1Breakfast => Seq("Ch1_Breakfast",
            ("Narrator", "The children argue over who gets the last of the bread, the way children do."),
            ("Lucille", "Eat. There will be more tomorrow."));

        // Ch1_Photo, Ch2_Rumor, Ch3_Wait, Ch4_Farewell and Ch5_Share (and the other memories/keepsakes) are
        // generated from JourneyCatalog by BuildCatalogSequences().

        // --- Chapter 2: The Market -----------------------------------------------------------

        private static ChapterDataSO BuildChapter2()
        {
            var data = Chapter(ChapterId.TheMarket, "Chapter 2: The Market", "Chapter2_TheMarket");

            data.introNarrative = Seq("Ch2_Intro",
                ("Narrator", "The market is loud and familiar - stalls, haggling, the smell of fresh bread."),
                ("Narrator", "Lately, though, the conversations carry a different weight."));

            data.outroNarrative = Seq("Ch2_Outro",
                ("Narrator", "Everyone insists it is nothing, the way people do when they are afraid it is something."),
                ("Narrator", "Lucille walks home a little faster than usual."));

            data.objectives = new[]
            {
                new ObjectiveDefinition { objectiveId = "ch2_stall", description = "Buy vegetables from the market stall" },
                new ObjectiveDefinition { objectiveId = "ch2_greet", description = "Greet a familiar face" },
                new ObjectiveDefinition { objectiveId = "ch2_rumor", description = "Overhear worried talk among neighbours", isMemoryFragment = true },
            };
            return data;
        }

        public static NarrativeSequenceSO Ch2Stall => Seq("Ch2_Stall",
            ("Vendor", "Same price as last week, Lucille. Can't promise that next week."),
            ("Lucille", "Let's hope next week looks like this one."));

        public static NarrativeSequenceSO Ch2Greet => Seq("Ch2_Greet",
            ("Neighbour", "Morning! The children keeping you busy?"),
            ("Lucille", "Always. Wouldn't trade it, though."));


        // --- Chapter 3: Bombing and Hiding ----------------------------------------------------

        private static ChapterDataSO BuildChapter3()
        {
            var data = Chapter(ChapterId.BombingAndHiding, "Chapter 3: Bombing and Hiding", "Chapter3_BombingAndHiding");

            data.introNarrative = Seq("Ch3_Intro",
                ("Narrator", "The first sound is distant, almost nothing. The second is not."),
                ("Narrator", "There is no time to understand what is happening - only to move."));

            data.outroNarrative = Seq("Ch3_Outro",
                ("Narrator", "When it is quiet again, the street outside looks nothing like it did yesterday."),
                ("Lucille", "We can't stay here. Not anymore."));

            data.objectives = new[]
            {
                new ObjectiveDefinition { objectiveId = "ch3_alarm", description = "React to the first explosion" },
                new ObjectiveDefinition { objectiveId = "ch3_shelter", description = "Find shelter for the children" },
                new ObjectiveDefinition { objectiveId = "ch3_wait", description = "Wait out the attack in hiding", isMemoryFragment = true },
            };
            return data;
        }

        public static NarrativeSequenceSO Ch3Alarm => Seq("Ch3_Alarm",
            ("Narrator", "The windows rattle. Somewhere close, something falls."),
            ("Lucille", "Get down - now, get down!"));

        public static NarrativeSequenceSO Ch3Shelter => Seq("Ch3_Shelter",
            ("Narrator", "Under the table, arms around both children, Lucille counts her own breaths to steady them."),
            ("Lucille", "I'm here. I'm not going anywhere."));


        // --- Chapter 4: Displacement -----------------------------------------------------------

        private static ChapterDataSO BuildChapter4()
        {
            var data = Chapter(ChapterId.Displacement, "Chapter 4: Displacement", "Chapter4_Displacement");

            data.introNarrative = Seq("Ch4_Intro",
                ("Narrator", "There is no good way to decide what a life fits into one bag."),
                ("Narrator", "Lucille decides anyway, because the road will not wait."));

            data.outroNarrative = Seq("Ch4_Outro",
                ("Narrator", "The house grows smaller behind them, then disappears around a bend in the road."),
                ("Narrator", "Ahead: distance, uncertainty, and each other."));

            data.objectives = new[]
            {
                new ObjectiveDefinition { objectiveId = "ch4_pack", description = "Pack only what can be carried" },
                new ObjectiveDefinition { objectiveId = "ch4_farewell", description = "Say goodbye to home", isMemoryFragment = true },
                new ObjectiveDefinition { objectiveId = "ch4_depart", description = "Begin the journey away from danger" },
            };
            return data;
        }



        public static NarrativeSequenceSO Ch4Depart => Seq("Ch4_Depart",
            ("Narrator", "The road ahead is long, and crowded with others carrying the same kind of nothing."),
            ("Lucille", "Stay close to me. Both of you, stay close."));

        // --- Chapter 5: Survival -----------------------------------------------------------------

        private static ChapterDataSO BuildChapter5()
        {
            var data = Chapter(ChapterId.Survival, "Chapter 5: Survival", "Chapter5_Survival");

            data.introNarrative = Seq("Ch5_Intro",
                ("Narrator", "Days on the road blur together: walking, resting, walking again."),
                ("Narrator", "Survival becomes a list of small, urgent tasks."));

            data.outroNarrative = Seq("Ch5_Outro",
                ("Narrator", "A camp appears on the horizon - tents, smoke, strangers who might, eventually, be neighbours."),
                ("Lucille", "It isn't home. But it might be a start."));

            data.objectives = new[]
            {
                new ObjectiveDefinition { objectiveId = "ch5_water", description = "Find clean water" },
                new ObjectiveDefinition { objectiveId = "ch5_shelter", description = "Build a temporary shelter for the night" },
                new ObjectiveDefinition { objectiveId = "ch5_share", description = "Share what little food remains", isMemoryFragment = true },
            };
            return data;
        }

        public static NarrativeSequenceSO Ch5Water => Seq("Ch5_Water",
            ("Narrator", "A thin stream, further than it looked. Lucille fills what containers they have left."),
            ("Lucille", "Small victories. We'll take them."));

        public static NarrativeSequenceSO Ch5Shelter => Seq("Ch5_Shelter",
            ("Narrator", "A lean-to of branches and a tarp won't keep out much, but it will keep out enough."),
            ("Lucille", "One more night. We can manage one more night."));


        // --- Chapter 6: A Second Chance ------------------------------------------------------------

        private static ChapterDataSO BuildChapter6()
        {
            var data = Chapter(ChapterId.SecondChance, "Chapter 6: A Second Chance", "Chapter6_SecondChance");

            data.introNarrative = Seq("Ch6_Intro",
                ("Narrator", "A new city. A single room. A chance to begin again - on much less than before."),
                ("Narrator", "Every choice here has a cost that wasn't there before."));

            data.outroNarrative = Seq("Ch6_Outro",
                ("Narrator", "There is no version of this decision that costs nothing."),
                ("Narrator", "Lucille makes it anyway, the way she has made every hard decision since this began - for them."));

            data.objectives = new[]
            {
                new ObjectiveDefinition { objectiveId = "ch6_settle", description = "Settle into the new, unfamiliar room" },
                new ObjectiveDefinition { objectiveId = "ch6_work", description = "Find work to support the family" },
                new ObjectiveDefinition { objectiveId = "ch6_decision", description = "Decide how to spend what little money remains" },
            };
            return data;
        }

        public static NarrativeSequenceSO Ch6Settle => Seq("Ch6_Settle",
            ("Narrator", "A bare room, borrowed furniture, a lock on the door that actually works."),
            ("Lucille", "It's small. It's ours for now. That's enough."));

        public static NarrativeSequenceSO Ch6Work => Seq("Ch6_Work",
            ("Narrator", "Long hours for little pay, in a job far below what she once trained for."),
            ("Lucille", "It isn't what I imagined. It keeps us fed."));

        public static NarrativeSequenceSO Ch6Framing => Seq("Ch6_Framing",
            ("Narrator", "The money on the table is not enough for everything it needs to cover this month."),
            ("Lucille", "Choose. There's no way to choose that doesn't cost something."));

        public static ChoiceOption[] Ch6Options => new[]
        {
            new ChoiceOption
            {
                id = "school_fees",
                label = "Pay the children's school fees",
                consequence = Seq("Ch6_Choice_School",
                    ("Narrator", "The children stay in school. The lights go out early to save on the bill, and dinner is smaller than it should be."),
                    ("Lucille", "An education is something nobody can take from them. We'll find the rest somehow."))
            },
            new ChoiceOption
            {
                id = "utilities",
                label = "Pay for electricity and water",
                consequence = Seq("Ch6_Choice_Utilities",
                    ("Narrator", "The lights stay on and the water runs warm, but the school notice about unpaid fees stays pinned to the door."),
                    ("Lucille", "We need light to do homework by. I'll explain to the school. Again."))
            },
            new ChoiceOption
            {
                id = "groceries",
                label = "Buy groceries for the week",
                consequence = Seq("Ch6_Choice_Groceries",
                    ("Narrator", "Nobody goes hungry this week. The electricity is cut two days later, and the school letter arrives the day after that."),
                    ("Lucille", "They need to eat today. Tomorrow's problems can wait until tomorrow."))
            },
        };

        /// <summary>
        /// Plain (id, label, consequence-asset-name) data for the three choice options, safe to
        /// pass across a scene change. Use Load(consequenceName) to resolve the actual sequence
        /// after the scene has been built, rather than holding the ChoiceOption[] from Ch6Options.
        /// </summary>
        public static (string id, string label, string consequenceName)[] Ch6OptionSpecs => new[]
        {
            ("school_fees", "Pay the children's school fees", "Ch6_Choice_School"),
            ("utilities", "Pay for electricity and water", "Ch6_Choice_Utilities"),
            ("groceries", "Buy groceries for the week", "Ch6_Choice_Groceries"),
        };
    }
}
