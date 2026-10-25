using System;
using System.Collections.Generic;

namespace LucillesJourney.Core
{
    /// <summary>
    /// Serialized via JsonUtility, saved as one file so a player can resume progress.
    /// Every field added after the first release has a safe default, so older save files load
    /// unchanged (JsonUtility leaves missing fields at their initialised value) and are then
    /// normalised by SaveSystem.Sanitize.
    /// </summary>
    [Serializable]
    public class GameProgressData
    {
        public const int CurrentSaveVersion = 2;

        /// <summary>Stays 0 when the field is absent from the file (original format); SaveProgress stamps the current version.</summary>
        public int saveVersion = 0;

        public int highestUnlockedChapterIndex = 0;
        public List<string> completedObjectiveIds = new List<string>();
        public List<string> discoveredMemoryIds = new List<string>();
        public List<string> introShownForChapter = new List<string>();

        // --- Gamification layer (save version 2) ---------------------------------------------
        public List<string> discoveredKeepsakeIds = new List<string>();
        public List<string> completedChapterIds = new List<string>();
        public List<string> unlockedAchievementIds = new List<string>();

        /// <summary>Persistent world-state flags set by interactions, e.g. "ch1_garden_watered".</summary>
        public List<string> environmentStates = new List<string>();

        /// <summary>Entries (memories, keepsakes, achievements) the player has already opened.</summary>
        public List<string> viewedEntryIds = new List<string>();

        public float playTimeSeconds = 0f;

        /// <summary>Which option the player chose in Chapter 6's impossible choice ("" if not yet decided).</summary>
        public string finalChoiceId = "";

        /// <summary>Human-readable label of the final choice, shown on the ending screen.</summary>
        public string finalChoiceLabel = "";
    }
}
