using System;
using System.Collections.Generic;
using System.Linq;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification
{
    public readonly struct NarrationLine
    {
        public readonly string Speaker;
        public readonly string Text;
        public NarrationLine(string speaker, string text) { Speaker = speaker; Text = text; }
    }

    public enum ObjectiveKind { Interaction, Step, Memory, Keepsake, Decision }

    public sealed class ChapterInfo
    {
        public ChapterId Id;
        public string Title;       // e.g. "Chapter 1: Ordinary Life" (matches ChapterDataSO.displayName)
        public string ShortTitle;  // e.g. "Ordinary Life"
        public string Description;
        public string SceneName;
        public string ThumbnailKey;
        public int Index => (int)Id;
    }

    /// <summary>One task in the quest journal. Memory and keepsake objectives share their id with the memory/keepsake they reveal.</summary>
    public sealed class ObjectiveInfo
    {
        public string Id;
        public ChapterId Chapter;
        public string Title;
        public string Hint;
        public string Prompt;               // verb shown by the in-world interaction prompt
        public bool Optional;
        public ObjectiveKind Kind;
        public string[] StepIds = Array.Empty<string>();     // for multi-step objectives
        public string ParentId;                              // for steps
        public string[] RequiresIds = Array.Empty<string>(); // objectives that must be complete before this one can be done
        public string SequenceName;         // NarrativeSequenceSO asset played on interaction (editor builders)
        public NarrationLine[] Lines = Array.Empty<NarrationLine>(); // content for builder-created sequences

        public bool HasSteps => StepIds.Length > 0;
    }

    public sealed class MemoryInfo
    {
        public string Id;
        public ChapterId Chapter;
        public string Title;
        public NarrationLine[] Lines;
        public string ArtKey => "mem_" + Id;
    }

    public sealed class KeepsakeInfo
    {
        public string Id;
        public ChapterId Chapter;
        public string Title;
        public string Description;
        public string IconKey;
    }

    public sealed class ChoiceInfo
    {
        public string Id;
        public string Title;
        public string Covers;
        public string LeavesLess;
        public string ArtKey;
    }

    /// <summary>
    /// Static definitions for the whole journey: chapters, objectives, memories, keepsakes and the
    /// final choice. Runtime progress lives in GameManager/GameProgressData; this class only
    /// describes what exists. Saved ids that are not defined here are ignored everywhere.
    ///
    /// CONTENT NOTE: like the existing narrative placeholders, all connective text here is fictional,
    /// derived only from the chapter structure and existing narrative sequences. It is not Lucille's
    /// testimony and must be reviewed against the concept document before submission.
    /// </summary>
    public static class JourneyCatalog
    {
        public static readonly IReadOnlyList<ChapterInfo> Chapters = new[]
        {
            Chapter(ChapterId.OrdinaryLife, "Chapter 1: Ordinary Life", "Ordinary Life", "Chapter1_OrdinaryLife",
                "Everyday routines: tending the garden and preparing breakfast."),
            Chapter(ChapterId.TheMarket, "Chapter 2: The Market", "The Market", "Chapter2_TheMarket",
                "Ordinary errands, interrupted by a growing sense of danger."),
            Chapter(ChapterId.BombingAndHiding, "Chapter 3: Bombing and Hiding", "Bombing and Hiding", "Chapter3_BombingAndHiding",
                "Seeking shelter for the children amid danger."),
            Chapter(ChapterId.Displacement, "Chapter 4: Displacement", "Displacement", "Chapter4_Displacement",
                "Leaving home, and deciding what can be carried."),
            Chapter(ChapterId.Survival, "Chapter 5: Survival", "Survival", "Chapter5_Survival",
                "Temporary shelter, and the daily search for food and water."),
            Chapter(ChapterId.SecondChance, "Chapter 6: A Second Chance", "A Second Chance", "Chapter6_SecondChance",
                "A new start, and a decision about school fees, electricity and water, and groceries."),
        };

        public static readonly IReadOnlyList<ObjectiveInfo> Objectives = BuildObjectives();
        public static readonly IReadOnlyList<MemoryInfo> Memories = BuildMemories();
        public static readonly IReadOnlyList<KeepsakeInfo> Keepsakes = BuildKeepsakes();
        public static readonly IReadOnlyList<ChoiceInfo> Choices = BuildChoices();

        private static readonly Dictionary<string, ObjectiveInfo> ObjectiveById = Objectives.ToDictionary(o => o.Id);
        private static readonly Dictionary<string, MemoryInfo> MemoryById = Memories.ToDictionary(m => m.Id);
        private static readonly Dictionary<string, KeepsakeInfo> KeepsakeById = Keepsakes.ToDictionary(k => k.Id);

        // --- Lookups ------------------------------------------------------------------------------

        public static ChapterInfo GetChapter(ChapterId id) => Chapters.FirstOrDefault(c => c.Id == id);
        public static ChapterInfo GetChapter(int index) => index >= 0 && index < Chapters.Count ? Chapters[index] : null;

        public static ChapterInfo GetChapterBySceneName(string sceneName) =>
            Chapters.FirstOrDefault(c => c.SceneName == sceneName);

        public static bool TryGetObjective(string id, out ObjectiveInfo info)
        {
            info = null;
            return !string.IsNullOrEmpty(id) && ObjectiveById.TryGetValue(id, out info);
        }

        public static bool TryGetMemory(string id, out MemoryInfo info)
        {
            info = null;
            return !string.IsNullOrEmpty(id) && MemoryById.TryGetValue(id, out info);
        }

        public static bool TryGetKeepsake(string id, out KeepsakeInfo info)
        {
            info = null;
            return !string.IsNullOrEmpty(id) && KeepsakeById.TryGetValue(id, out info);
        }

        public static ChoiceInfo GetChoice(string id) => Choices.FirstOrDefault(c => c.Id == id);

        /// <summary>Objectives shown in the quest journal for a chapter (steps are excluded; they belong to their parent).</summary>
        public static IEnumerable<ObjectiveInfo> ObjectivesFor(ChapterId chapter) =>
            Objectives.Where(o => o.Chapter == chapter && o.Kind != ObjectiveKind.Step);

        public static IEnumerable<ObjectiveInfo> RequiredFor(ChapterId chapter) => ObjectivesFor(chapter).Where(o => !o.Optional);
        public static IEnumerable<ObjectiveInfo> OptionalFor(ChapterId chapter) => ObjectivesFor(chapter).Where(o => o.Optional);
        public static IEnumerable<MemoryInfo> MemoriesFor(ChapterId chapter) => Memories.Where(m => m.Chapter == chapter);
        public static IEnumerable<KeepsakeInfo> KeepsakesFor(ChapterId chapter) => Keepsakes.Where(k => k.Chapter == chapter);

        public static IEnumerable<ObjectiveInfo> StepsOf(string parentId) =>
            Objectives.Where(o => o.ParentId == parentId);

        // --- Definitions --------------------------------------------------------------------------

        private static ChapterInfo Chapter(ChapterId id, string title, string shortTitle, string scene, string description) =>
            new ChapterInfo
            {
                Id = id, Title = title, ShortTitle = shortTitle, SceneName = scene, Description = description,
                ThumbnailKey = "chapter_" + ((int)id + 1)
            };

        private static NarrationLine N(string text) => new NarrationLine("Narrator", text);
        private static NarrationLine L(string text) => new NarrationLine("Lucille", text);

        private static ObjectiveInfo Req(string id, ChapterId ch, string title, string hint, string prompt, string seq,
            ObjectiveKind kind = ObjectiveKind.Interaction, params string[] requires) =>
            new ObjectiveInfo
            {
                Id = id, Chapter = ch, Title = title, Hint = hint, Prompt = prompt, SequenceName = seq,
                Kind = kind, Optional = false, RequiresIds = requires ?? Array.Empty<string>()
            };

        private static ObjectiveInfo Opt(string id, ChapterId ch, string title, string hint, string prompt, ObjectiveKind kind) =>
            new ObjectiveInfo
            {
                Id = id, Chapter = ch, Title = title, Hint = hint, Prompt = prompt,
                Kind = kind, Optional = true
            };

        private static ObjectiveInfo Step(string id, string parent, ChapterId ch, string title, string prompt, string seq,
            params NarrationLine[] lines) =>
            new ObjectiveInfo
            {
                Id = id, ParentId = parent, Chapter = ch, Title = title, Hint = "", Prompt = prompt, SequenceName = seq,
                Kind = ObjectiveKind.Step, Optional = false, Lines = lines
            };

        // The five original memory objectives keep their existing narrative asset names.
        private static string LegacySequenceName(string id)
        {
            switch (id)
            {
                case "ch1_photo": return "Ch1_Photo";
                case "ch2_rumor": return "Ch2_Rumor";
                case "ch3_wait": return "Ch3_Wait";
                case "ch4_farewell": return "Ch4_Farewell";
                case "ch5_share": return "Ch5_Share";
                default: return null;
            }
        }

        private static List<ObjectiveInfo> BuildObjectives()
        {
            var list = new List<ObjectiveInfo>
            {
                // Chapter 1 - Ordinary Life
                Req("ch1_garden", ChapterId.OrdinaryLife, "Water the garden", "Find the planter in the garden.", "Water Garden", "Ch1_Garden"),
                Req("ch1_breakfast", ChapterId.OrdinaryLife, "Share breakfast with the children", "Breakfast is waiting at the table.", "Share Breakfast", "Ch1_Breakfast"),
                Opt("ch1_photo", ChapterId.OrdinaryLife, "Look at the family photograph", "Something is kept on the wall.", "Look at Photograph", ObjectiveKind.Memory),
                Opt("ch1_shelf", ChapterId.OrdinaryLife, "Look over the kitchen shelf", "Everyday things are kept on the shelf.", "Look at Shelf", ObjectiveKind.Memory),
                Opt("ch1_ks_wateringcan", ChapterId.OrdinaryLife, "Pick up the watering can", "Kept by the door.", "Examine Watering Can", ObjectiveKind.Keepsake),

                // Chapter 2 - The Market
                Req("ch2_stall", ChapterId.TheMarket, "Buy vegetables from the market stall", "Visit the vegetable stall.", "Buy Vegetables", "Ch2_Stall"),
                Req("ch2_greet", ChapterId.TheMarket, "Greet a familiar face", "A neighbour is nearby.", "Greet Neighbour", "Ch2_Greet"),
                Opt("ch2_rumor", ChapterId.TheMarket, "Overhear worried talk among neighbours", "Voices are lowered near the grain stall.", "Listen Closely", ObjectiveKind.Memory),
                Opt("ch2_bread", ChapterId.TheMarket, "Stop at the bread stall", "Something warm is baking.", "Look at Bread", ObjectiveKind.Memory),
                Opt("ch2_ks_basket", ChapterId.TheMarket, "Look at the market basket", "Left near the entrance.", "Examine Basket", ObjectiveKind.Keepsake),

                // Chapter 3 - Bombing and Hiding
                Req("ch3_alarm", ChapterId.BombingAndHiding, "React to the first explosion", "Take cover.", "Take Cover", "Ch3_Alarm"),
                Req("ch3_shelter", ChapterId.BombingAndHiding, "Find shelter for the children", "Somewhere safer must be close.", "Find Shelter", "Ch3_Shelter"),
                Opt("ch3_wait", ChapterId.BombingAndHiding, "Wait out the attack in hiding", "Stay still, stay quiet.", "Wait It Out", ObjectiveKind.Memory),
                Opt("ch3_counting", ChapterId.BombingAndHiding, "Steady the children's breathing", "Stay close to them.", "Stay Close", ObjectiveKind.Memory),
                Opt("ch3_ks_blanket", ChapterId.BombingAndHiding, "Look at the folded blanket", "Within reach of the shelter.", "Examine Blanket", ObjectiveKind.Keepsake),

                // Chapter 4 - Displacement (packing is multi-step; departing needs the bag packed)
                Req("ch4_pack", ChapterId.Displacement, "Pack only what can be carried", "Gather what matters most.", "Pack Bag", null),
                Req("ch4_depart", ChapterId.Displacement, "Begin the journey away from danger", "Once the bag is packed, head out.", "Head Out", "Ch4_Depart",
                    ObjectiveKind.Interaction, "ch4_pack"),
                Opt("ch4_farewell", ChapterId.Displacement, "Say goodbye to home", "One last look before leaving.", "Say Goodbye", ObjectiveKind.Memory),
                Opt("ch4_shelf", ChapterId.Displacement, "Look at the empty shelf", "A mark is left on the wall.", "Look at Shelf", ObjectiveKind.Memory),
                Opt("ch4_ks_clothbag", ChapterId.Displacement, "Look at the small cloth bag", "Left behind a chair.", "Examine Cloth Bag", ObjectiveKind.Keepsake),

                // Chapter 5 - Survival
                Req("ch5_water", ChapterId.Survival, "Find clean water", "Water is far, but not impossible.", "Find Water", "Ch5_Water"),
                Req("ch5_shelter", ChapterId.Survival, "Build a temporary shelter for the night", "Find a place to rest.", "Build Shelter", "Ch5_Shelter"),
                Opt("ch5_share", ChapterId.Survival, "Share what little food remains", "Another family is nearby.", "Share Food", ObjectiveKind.Memory),
                Opt("ch5_neighbours", ChapterId.Survival, "Look around the camp", "People are gathered nearby.", "Look Around", ObjectiveKind.Memory),
                Opt("ch5_ks_jug", ChapterId.Survival, "Look at the water jug", "Set down by the stream.", "Examine Jug", ObjectiveKind.Keepsake),

                // Chapter 6 - A Second Chance
                Req("ch6_settle", ChapterId.SecondChance, "Settle into the new, unfamiliar room", "Take a moment to settle.", "Settle In", "Ch6_Settle"),
                Req("ch6_work", ChapterId.SecondChance, "Find work to support the family", "Look for work nearby.", "Find Work", "Ch6_Work"),
                Req("ch6_decision", ChapterId.SecondChance, "Decide how to spend what little money remains", "Once settled and working, the decision waits.", "Decide", null,
                    ObjectiveKind.Decision, "ch6_settle", "ch6_work"),
                Opt("ch6_lock", ChapterId.SecondChance, "Try the lock on the door", "The door has a lock.", "Try Lock", ObjectiveKind.Memory),
                Opt("ch6_window", ChapterId.SecondChance, "Look at the desk by the window", "A small desk sits by the window.", "Look at Desk", ObjectiveKind.Memory),
                Opt("ch6_ks_key", ChapterId.SecondChance, "Look at the room key", "Placed on the table.", "Examine Key", ObjectiveKind.Keepsake),
            };

            list.First(o => o.Id == "ch4_pack").StepIds = new[] { "ch4_pack_documents", "ch4_pack_food", "ch4_pack_photo" };
            list.Add(Step("ch4_pack_documents", "ch4_pack", ChapterId.Displacement, "Pack the documents", "Pack Documents", "Ch4_Pack_Documents",
                N("Papers that prove who they are, folded flat and placed at the bottom of the bag."),
                L("These come first.")));
            list.Add(Step("ch4_pack_food", "ch4_pack", ChapterId.Displacement, "Pack some food", "Pack Food", "Ch4_Pack_Food",
                N("A little bread and a tin, wrapped in cloth. It will have to last."),
                L("Only what we can carry.")));
            list.Add(Step("ch4_pack_photo", "ch4_pack", ChapterId.Displacement, "Pack the photograph", "Pack Photograph", "Ch4_Pack_Photo",
                N("The photograph from the shelf, slid carefully between two pages so it won't crease."),
                L("There is room for this.")));

            foreach (var m in BuildMemories())
            {
                var o = list.First(x => x.Id == m.Id);
                o.Lines = m.Lines;
                o.SequenceName = LegacySequenceName(m.Id) ?? "Mem_" + m.Id;
            }
            foreach (var k in BuildKeepsakes())
            {
                var o = list.First(x => x.Id == k.Id);
                o.Lines = new[] { N(k.Description) };
                o.SequenceName = "Ks_" + k.Id;
            }

            return list;
        }

        private static List<MemoryInfo> BuildMemories() => new List<MemoryInfo>
        {
            M("ch1_photo", ChapterId.OrdinaryLife, "The Family Photograph",
                N("A photograph, a little creased at the corner. A celebration, long before any of this."),
                L("I keep meaning to find a frame for this one.")),
            M("ch1_shelf", ChapterId.OrdinaryLife, "The Kitchen Shelf",
                N("Cups, a jar of spices, a bowl with a chipped edge. Nothing precious, and all of it used every day."),
                N("Ordinary things are what a home is made from.")),

            M("ch2_rumor", ChapterId.TheMarket, "Talk by the Grain Stall",
                N("Two voices, lowered, near the grain stall - talk of fighting on the road from the north."),
                N("Lucille tells herself it is only talk.")),
            M("ch2_bread", ChapterId.TheMarket, "The Smell of Bread",
                N("Warm bread, stacked at the front of the stall, pulls people in from across the market."),
                N("For a moment the market feels exactly the way it always has.")),

            M("ch3_wait", ChapterId.BombingAndHiding, "Waiting in Hiding",
                N("Minutes stretch into what feels like hours. Nobody speaks."),
                N("Lucille holds on to the only plan that matters: keep them safe, one minute at a time.")),
            M("ch3_counting", ChapterId.BombingAndHiding, "Counting Breaths",
                N("Counting breaths is something to do with the mind when the body can do nothing else."),
                N("Slow in. Slow out. The children's breathing slowly follows.")),

            M("ch4_farewell", ChapterId.Displacement, "A Hand on the Doorframe",
                N("Lucille rests a hand on the doorframe for a moment longer than she means to."),
                L("We'll come back. Somehow, we'll come back.")),
            M("ch4_shelf", ChapterId.Displacement, "The Empty Shelf",
                N("A pale rectangle on the wall where the photograph used to hang."),
                N("A home looks different the moment its things start to leave it.")),

            M("ch5_share", ChapterId.Survival, "Sharing What Remains",
                N("Another family nearby has even less. Lucille splits what remains without being asked."),
                L("We'll manage. You take this.")),
            M("ch5_neighbours", ChapterId.Survival, "Strangers Nearby",
                N("Tents in uneven rows, a small fire, quiet conversations between people who met only days ago."),
                N("Nobody here is where they planned to be, and for now, that is something they share.")),

            M("ch6_lock", ChapterId.SecondChance, "A Lock That Works",
                N("The key turns smoothly. The door closes. For the first time in a long while, the sound is reassuring."),
                N("Safety can be as small as a door that stays shut.")),
            M("ch6_window", ChapterId.SecondChance, "A Place to Do Homework",
                N("A small desk by the window, a lamp, a few school books lined up."),
                N("Whatever else is uncertain, this corner is for learning.")),
        };

        private static MemoryInfo M(string id, ChapterId ch, string title, params NarrationLine[] lines) =>
            new MemoryInfo { Id = id, Chapter = ch, Title = title, Lines = lines };

        private static List<KeepsakeInfo> BuildKeepsakes() => new List<KeepsakeInfo>
        {
            K("ch1_ks_wateringcan", ChapterId.OrdinaryLife, "Watering Can", "A dented watering can kept by the door, used for the garden almost every morning.", "ks_watering_can"),
            K("ch2_ks_basket", ChapterId.TheMarket, "Market Basket", "A woven basket carried to the market and home again, week after week.", "ks_basket"),
            K("ch3_ks_blanket", ChapterId.BombingAndHiding, "Folded Blanket", "A blanket for the children while they waited. Soft, and a little worn.", "ks_blanket"),
            K("ch4_ks_clothbag", ChapterId.Displacement, "Cloth Bag", "A small cloth bag with a drawstring, light enough to carry a long way.", "ks_bag"),
            K("ch5_ks_jug", ChapterId.Survival, "Water Jug", "A jug for carrying water from wherever it can be found.", "ks_jug"),
            K("ch6_ks_key", ChapterId.SecondChance, "Room Key", "The key to a single room. Small, plain, and theirs for now.", "ks_key"),
        };

        private static KeepsakeInfo K(string id, ChapterId ch, string title, string description, string icon) =>
            new KeepsakeInfo { Id = id, Chapter = ch, Title = title, Description = description, IconKey = icon };

        private static List<ChoiceInfo> BuildChoices() => new List<ChoiceInfo>
        {
            new ChoiceInfo
            {
                Id = "school_fees", Title = "School fees", ArtKey = "choice_school_fees",
                Covers = "The children's school fees, so they can stay in class.",
                LeavesLess = "Electricity and water, and groceries."
            },
            new ChoiceInfo
            {
                Id = "utilities", Title = "Electricity and water", ArtKey = "choice_utilities",
                Covers = "Light, and running water in the room.",
                LeavesLess = "School fees, and groceries."
            },
            new ChoiceInfo
            {
                Id = "groceries", Title = "Groceries", ArtKey = "choice_groceries",
                Covers = "Food for the week.",
                LeavesLess = "School fees, and electricity and water."
            },
        };
    }
}
