using System;
using System.Collections.Generic;
using System.Linq;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification
{
    public enum ChapterState { Locked, Available, InProgress, Completed, FullyExplored }

    /// <summary>A read-only snapshot of one chapter's progress, derived from saved gameplay data.</summary>
    public sealed class ChapterProgress
    {
        public ChapterInfo Info;
        public ChapterState State;
        public bool IsCurrent;
        public int RequiredDone, RequiredTotal;
        public int OptionalDone, OptionalTotal;
        public int MemoriesDone, MemoriesTotal;
        public int KeepsakesDone, KeepsakesTotal;

        public float Story => RequiredTotal == 0 ? 0f : (float)RequiredDone / RequiredTotal;
        public float Exploration => OptionalTotal == 0 ? 0f : (float)OptionalDone / OptionalTotal;
        public bool IsUnlocked => State != ChapterState.Locked;
        public int OptionalRemaining => OptionalTotal - OptionalDone;
    }

    /// <summary>
    /// Every number shown by the journey overview, statistics, HUD and chapter summaries is computed
    /// here from GameManager's saved progress and JourneyCatalog's definitions: there are no
    /// separate counters that could drift out of sync.
    /// </summary>
    public static class JourneyStats
    {
        private static GameManager Game => GameManager.Instance;

        public static bool IsOptionalDone(ObjectiveInfo o) =>
            Game.IsObjectiveComplete(o.Id)
            || (o.Kind == ObjectiveKind.Memory && Game.IsMemoryDiscovered(o.Id))
            || (o.Kind == ObjectiveKind.Keepsake && Game.IsKeepsakeCollected(o.Id));

        public static bool IsObjectiveDone(ObjectiveInfo o) => o.Optional ? IsOptionalDone(o) : Game.IsObjectiveComplete(o.Id);

        public static bool IsChapterUnlocked(ChapterId id) =>
            (int)id <= Game.HighestUnlockedChapterIndex || Game.IsChapterCompleted(id);

        public static ChapterProgress GetChapter(ChapterId id)
        {
            var info = JourneyCatalog.GetChapter(id);
            var p = new ChapterProgress { Info = info };
            if (info == null) return p;

            var required = JourneyCatalog.RequiredFor(id).ToList();
            var optional = JourneyCatalog.OptionalFor(id).ToList();
            p.RequiredTotal = required.Count;
            p.RequiredDone = required.Count(IsObjectiveDone);
            p.OptionalTotal = optional.Count;
            p.OptionalDone = optional.Count(IsOptionalDone);
            p.MemoriesTotal = JourneyCatalog.MemoriesFor(id).Count();
            p.MemoriesDone = JourneyCatalog.MemoriesFor(id).Count(m => Game.IsMemoryDiscovered(m.Id));
            p.KeepsakesTotal = JourneyCatalog.KeepsakesFor(id).Count();
            p.KeepsakesDone = JourneyCatalog.KeepsakesFor(id).Count(k => Game.IsKeepsakeCollected(k.Id));
            p.IsCurrent = Game.CurrentChapterId == id;

            bool completed = Game.IsChapterCompleted(id);
            if (!IsChapterUnlocked(id)) p.State = ChapterState.Locked;
            else if (completed) p.State = p.OptionalRemaining <= 0 ? ChapterState.FullyExplored : ChapterState.Completed;
            else if (p.RequiredDone > 0 || p.OptionalDone > 0 || p.IsCurrent) p.State = ChapterState.InProgress;
            else p.State = ChapterState.Available;
            return p;
        }

        public static IEnumerable<ChapterProgress> AllChapters() => JourneyCatalog.Chapters.Select(c => GetChapter(c.Id));

        public static int ChaptersCompleted => JourneyCatalog.Chapters.Count(c => Game.IsChapterCompleted(c.Id));
        public static int ChaptersTotal => JourneyCatalog.Chapters.Count;

        public static int MainDone => AllChapters().Sum(c => c.RequiredDone);
        public static int MainTotal => AllChapters().Sum(c => c.RequiredTotal);
        public static int OptionalDone => AllChapters().Sum(c => c.OptionalDone);
        public static int OptionalTotal => AllChapters().Sum(c => c.OptionalTotal);
        public static int MemoriesDone => JourneyCatalog.Memories.Count(m => Game.IsMemoryDiscovered(m.Id));
        public static int MemoriesTotal => JourneyCatalog.Memories.Count;
        public static int KeepsakesDone => JourneyCatalog.Keepsakes.Count(k => Game.IsKeepsakeCollected(k.Id));
        public static int KeepsakesTotal => JourneyCatalog.Keepsakes.Count;
        public static int AchievementsDone => AchievementService.All.Count(a => Game.IsAchievementUnlocked(a.Id));
        public static int AchievementsTotal => AchievementService.All.Count;

        public static float StoryProgress => MainTotal == 0 ? 0f : (float)MainDone / MainTotal;
        public static float ExplorationProgress => OptionalTotal == 0 ? 0f : (float)OptionalDone / OptionalTotal;

        public static bool IsJourneyComplete =>
            Game.IsChapterCompleted(ChapterId.SecondChance) && Game.HasFinalChoice;

        /// <summary>The objective the HUD should foreground: the first unfinished required objective whose prerequisites are met.</summary>
        public static ObjectiveInfo NextMainObjective(ChapterId chapter)
        {
            var pending = JourneyCatalog.RequiredFor(chapter).Where(o => !Game.IsObjectiveComplete(o.Id)).ToList();
            return pending.FirstOrDefault(o => o.RequiresIds.All(Game.IsObjectiveComplete)) ?? pending.FirstOrDefault();
        }

        /// <summary>(done, total) for multi-step objectives; (0, 0) for ordinary ones.</summary>
        public static (int done, int total) StepProgress(ObjectiveInfo objective)
        {
            if (objective == null || !objective.HasSteps) return (0, 0);
            return (objective.StepIds.Count(Game.IsObjectiveComplete), objective.StepIds.Length);
        }

        public static string FormatPlayTime(float seconds)
        {
            var t = TimeSpan.FromSeconds(Math.Max(0f, seconds));
            return t.TotalHours >= 1d ? $"{(int)t.TotalHours}h {t.Minutes:00}m" : $"{t.Minutes}m {t.Seconds:00}s";
        }

        public static string Percent(float value) => $"{(int)Math.Round(value * 100f)}%";
    }
}
