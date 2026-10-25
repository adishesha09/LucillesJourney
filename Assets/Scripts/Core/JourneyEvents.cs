using System;

namespace LucillesJourney.Core
{
    /// <summary>
    /// Broadcast hub for progression changes. GameManager raises these after it has updated and
    /// saved the authoritative progress data; HUD, journal, notification and achievement code
    /// subscribe here instead of referencing each other.
    /// </summary>
    public static class JourneyEvents
    {
        public static event Action<string> ObjectiveCompleted;
        public static event Action<string> MemoryDiscovered;
        public static event Action<string> KeepsakeCollected;
        public static event Action<string> AchievementUnlocked;
        public static event Action<ChapterId> ChapterCompleted;
        public static event Action<string, bool> EnvironmentStateChanged;
        public static event Action FinalChoiceRecorded;

        /// <summary>Raised after any change to saved progress (including the events above) and after New Game.</summary>
        public static event Action ProgressChanged;

        internal static void RaiseObjectiveCompleted(string id) => ObjectiveCompleted?.Invoke(id);
        internal static void RaiseMemoryDiscovered(string id) => MemoryDiscovered?.Invoke(id);
        internal static void RaiseKeepsakeCollected(string id) => KeepsakeCollected?.Invoke(id);
        internal static void RaiseAchievementUnlocked(string id) => AchievementUnlocked?.Invoke(id);
        internal static void RaiseChapterCompleted(ChapterId id) => ChapterCompleted?.Invoke(id);
        internal static void RaiseEnvironmentStateChanged(string key, bool value) => EnvironmentStateChanged?.Invoke(key, value);
        internal static void RaiseFinalChoiceRecorded() => FinalChoiceRecorded?.Invoke();
        internal static void RaiseProgressChanged() => ProgressChanged?.Invoke();
    }
}
