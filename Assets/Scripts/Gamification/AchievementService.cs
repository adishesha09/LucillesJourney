using System;
using System.Collections.Generic;
using System.Linq;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification
{
    public sealed class AchievementInfo
    {
        public string Id;
        public string Title;
        public string Description;
        public string IconKey;

        /// <summary>Returns the current measurable progress and the target that unlocks the achievement.</summary>
        public Func<(int current, int target)> Measure;
    }

    /// <summary>
    /// Static achievement definitions and their evaluation. Conditions read only real saved progress
    /// (via JourneyStats/GameManager); an achievement is awarded once and then persisted by GameManager.
    /// Achievements recognise curiosity and engagement; none rewards hardship or ranks the player's choices.
    /// </summary>
    public static class AchievementService
    {
        private static GameManager Game => GameManager.Instance;

        public static readonly IReadOnlyList<AchievementInfo> All = new List<AchievementInfo>
        {
            new AchievementInfo
            {
                Id = "first_memory", Title = "First Memory", IconKey = "ach_first_memory",
                Description = "Discover your first memory.",
                Measure = () => (JourneyStats.MemoriesDone, 1)
            },
            new AchievementInfo
            {
                Id = "first_keepsake", Title = "Something to Hold", IconKey = "ach_first_keepsake",
                Description = "Collect your first keepsake.",
                Measure = () => (JourneyStats.KeepsakesDone, 1)
            },
            new AchievementInfo
            {
                Id = "curious_observer", Title = "Curious Observer", IconKey = "ach_curious_observer",
                Description = "Look closely at six optional objects or moments.",
                Measure = () => (Math.Min(JourneyStats.OptionalDone, 6), 6)
            },
            new AchievementInfo
            {
                Id = "everyday_rituals", Title = "Everyday Rituals", IconKey = "ach_everyday_rituals",
                Description = "Water the garden and share breakfast.",
                Measure = () => (new[] { "ch1_garden", "ch1_breakfast" }.Count(Game.IsObjectiveComplete), 2)
            },
            new AchievementInfo
            {
                Id = "keeper_of_memories", Title = "Keeper of Memories", IconKey = "ach_keeper_of_memories",
                Description = "Discover every memory in a single chapter.",
                Measure = BestChapterMemories
            },
            new AchievementInfo
            {
                Id = "ready_to_leave", Title = "Ready to Leave", IconKey = "ach_ready_to_leave",
                Description = "Finish packing for the journey.",
                Measure = () => (Game.IsObjectiveComplete("ch4_pack") ? 1 : 0, 1)
            },
            new AchievementInfo
            {
                Id = "treasured_things", Title = "Treasured Things", IconKey = "ach_treasured",
                Description = "Collect every keepsake.",
                Measure = () => (JourneyStats.KeepsakesDone, JourneyStats.KeepsakesTotal)
            },
            new AchievementInfo
            {
                Id = "life_remembered", Title = "A Life Remembered", IconKey = "ach_life_remembered",
                Description = "Complete the Memory Album.",
                Measure = () => (JourneyStats.MemoriesDone, JourneyStats.MemoriesTotal)
            },
            new AchievementInfo
            {
                Id = "full_picture", Title = "The Full Picture", IconKey = "ach_full_picture",
                Description = "Complete every optional discovery.",
                Measure = () => (JourneyStats.OptionalDone, JourneyStats.OptionalTotal)
            },
            new AchievementInfo
            {
                Id = "journey_complete", Title = "Journey Complete", IconKey = "ach_journey_complete",
                Description = "Reach the end of Lucille's story.",
                Measure = () => (JourneyStats.IsJourneyComplete ? 1 : 0, 1)
            },
        };

        private static (int, int) BestChapterMemories()
        {
            (int done, int total) best = (0, 1);
            foreach (var chapter in JourneyCatalog.Chapters)
            {
                int total = JourneyCatalog.MemoriesFor(chapter.Id).Count();
                if (total == 0) continue;
                int done = JourneyCatalog.MemoriesFor(chapter.Id).Count(m => Game.IsMemoryDiscovered(m.Id));
                if ((float)done / total > (float)best.done / best.total) best = (done, total);
            }
            return best;
        }

        public static AchievementInfo Get(string id) => All.FirstOrDefault(a => a.Id == id);

        public static (int current, int target) Progress(AchievementInfo info)
        {
            var (current, target) = info.Measure();
            return (Math.Min(current, target), target);
        }

        /// <summary>Awards every achievement whose condition is now satisfied. Idempotent.</summary>
        public static void Evaluate()
        {
            foreach (var info in All)
            {
                if (Game.IsAchievementUnlocked(info.Id)) continue;
                var (current, target) = info.Measure();
                if (target > 0 && current >= target) Game.UnlockAchievement(info.Id);
            }
        }
    }
}
