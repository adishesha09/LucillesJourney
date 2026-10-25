using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using LucillesJourney.Gamification;

namespace LucillesJourney.Core
{
    /// <summary>
    /// Persistent (DontDestroyOnLoad) singleton that owns save-game progress, scene transitions
    /// and pause state. Self-creates on first access so it exists no matter which scene is opened
    /// first (the Main Menu in a normal playthrough, or any chapter scene during testing).
    ///
    /// All progression (objectives, memories, keepsakes, chapters, achievements, environment state,
    /// the final choice) is mutated only through this class so that one save file stays the single
    /// source of truth; changes are announced through JourneyEvents.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private GameProgressData progress;
        private int batchDepth;
        private bool dirty;
        private float unsavedPlayTime;

        public ChapterId CurrentChapterId { get; set; } = ChapterId.None;
        public bool IsPaused { get; private set; }

        public delegate void PauseStateHandler(bool isPaused);
        public event PauseStateHandler PauseStateChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            progress ??= SaveSystem.LoadProgress();
        }

        private void Update()
        {
            // Playtime counts only gameplay scenes, and not while paused.
            if (CurrentChapterId == ChapterId.None || IsPaused) return;
            float dt = Time.unscaledDeltaTime;
            progress.playTimeSeconds += dt;
            unsavedPlayTime += dt;
            if (unsavedPlayTime >= 30f) FlushPlayTime();
        }

        private void OnApplicationPause(bool paused) { if (paused) FlushPlayTime(); }
        private void OnApplicationQuit() => FlushPlayTime();

        private void FlushPlayTime()
        {
            if (unsavedPlayTime <= 0f || progress == null) return;
            unsavedPlayTime = 0f;
            SaveSystem.SaveProgress(progress);
        }

        // --- Saving ---------------------------------------------------------------------------

        private void Save()
        {
            if (batchDepth > 0) { dirty = true; return; }
            unsavedPlayTime = 0f;
            SaveSystem.SaveProgress(progress);
        }

        /// <summary>
        /// Runs several progress changes as one unit: the save file is written once at the end,
        /// achievements are evaluated once, and ProgressChanged is raised once.
        /// </summary>
        public void Batch(Action changes)
        {
            batchDepth++;
            try { changes?.Invoke(); }
            finally
            {
                if (batchDepth == 1) AchievementService.Evaluate();
                batchDepth--;
                if (batchDepth == 0)
                {
                    if (dirty)
                    {
                        dirty = false;
                        Save();
                    }
                    JourneyEvents.RaiseProgressChanged();
                }
            }
        }

        // --- Objectives -----------------------------------------------------------------------

        public bool IsObjectiveComplete(string objectiveId) =>
            !string.IsNullOrEmpty(objectiveId) && progress.completedObjectiveIds.Contains(objectiveId);

        /// <summary>Records an objective (or objective step) as complete. Returns false if it was already complete.</summary>
        public bool CompleteObjective(string objectiveId)
        {
            if (string.IsNullOrEmpty(objectiveId) || progress.completedObjectiveIds.Contains(objectiveId)) return false;

            Batch(() =>
            {
                progress.completedObjectiveIds.Add(objectiveId);
                Save();
                JourneyEvents.RaiseObjectiveCompleted(objectiveId);

                // Finishing the last step of a multi-step objective completes the parent objective.
                if (JourneyCatalog.TryGetObjective(objectiveId, out var info) && !string.IsNullOrEmpty(info.ParentId)
                    && JourneyCatalog.TryGetObjective(info.ParentId, out var parent)
                    && parent.StepIds.All(IsObjectiveComplete))
                {
                    CompleteObjective(parent.Id);
                }
            });
            return true;
        }

        /// <summary>Kept for existing callers; identical to CompleteObjective.</summary>
        public void MarkObjectiveComplete(string objectiveId) => CompleteObjective(objectiveId);

        // --- Memories / keepsakes ---------------------------------------------------------------

        public bool IsMemoryDiscovered(string memoryId) =>
            !string.IsNullOrEmpty(memoryId) && progress.discoveredMemoryIds.Contains(memoryId);

        public bool DiscoverMemory(string memoryId)
        {
            if (string.IsNullOrEmpty(memoryId) || progress.discoveredMemoryIds.Contains(memoryId)) return false;
            Batch(() =>
            {
                progress.discoveredMemoryIds.Add(memoryId);
                Save();
                JourneyEvents.RaiseMemoryDiscovered(memoryId);
            });
            return true;
        }

        /// <summary>Kept for existing callers; identical to DiscoverMemory.</summary>
        public void MarkMemoryDiscovered(string memoryId) => DiscoverMemory(memoryId);

        public bool IsKeepsakeCollected(string keepsakeId) =>
            !string.IsNullOrEmpty(keepsakeId) && progress.discoveredKeepsakeIds.Contains(keepsakeId);

        public bool CollectKeepsake(string keepsakeId)
        {
            if (string.IsNullOrEmpty(keepsakeId) || progress.discoveredKeepsakeIds.Contains(keepsakeId)) return false;
            Batch(() =>
            {
                progress.discoveredKeepsakeIds.Add(keepsakeId);
                Save();
                JourneyEvents.RaiseKeepsakeCollected(keepsakeId);
            });
            return true;
        }

        /// <summary>Entry keys look like "memory:ch1_photo". Used for the "new" indicators in the collections.</summary>
        public bool IsEntryViewed(string entryKey) => progress.viewedEntryIds.Contains(entryKey);

        public void MarkEntryViewed(string entryKey)
        {
            if (string.IsNullOrEmpty(entryKey) || progress.viewedEntryIds.Contains(entryKey)) return;
            progress.viewedEntryIds.Add(entryKey);
            Save();
            if (batchDepth == 0) JourneyEvents.RaiseProgressChanged();
        }

        // --- Chapters ---------------------------------------------------------------------------

        public bool IsChapterCompleted(ChapterId chapterId) =>
            progress.completedChapterIds.Contains(chapterId.ToString());

        /// <summary>Marks the chapter's story complete and unlocks the next chapter. Safe to call repeatedly.</summary>
        public bool CompleteChapter(ChapterId chapterId)
        {
            if (chapterId == ChapterId.None || IsChapterCompleted(chapterId)) return false;
            Batch(() =>
            {
                progress.completedChapterIds.Add(chapterId.ToString());
                int next = (int)chapterId + 1;
                if (next <= (int)ChapterId.SecondChance && next > progress.highestUnlockedChapterIndex)
                    progress.highestUnlockedChapterIndex = next;
                Save();
                JourneyEvents.RaiseChapterCompleted(chapterId);
            });
            return true;
        }

        // --- Achievements -----------------------------------------------------------------------

        public bool IsAchievementUnlocked(string achievementId) =>
            !string.IsNullOrEmpty(achievementId) && progress.unlockedAchievementIds.Contains(achievementId);

        /// <summary>Called by AchievementService only; awards each achievement exactly once.</summary>
        internal bool UnlockAchievement(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId) || progress.unlockedAchievementIds.Contains(achievementId)) return false;
            progress.unlockedAchievementIds.Add(achievementId);
            Save();
            JourneyEvents.RaiseAchievementUnlocked(achievementId);
            return true;
        }

        // --- Environment state --------------------------------------------------------------------

        public bool IsEnvironmentState(string key) =>
            !string.IsNullOrEmpty(key) && progress.environmentStates.Contains(key);

        public void SetEnvironmentState(string key, bool value = true)
        {
            if (string.IsNullOrEmpty(key) || IsEnvironmentState(key) == value) return;
            if (value) progress.environmentStates.Add(key); else progress.environmentStates.Remove(key);
            Save();
            JourneyEvents.RaiseEnvironmentStateChanged(key, value);
        }

        // --- Statistics -------------------------------------------------------------------------

        public float PlayTimeSeconds => progress.playTimeSeconds;

        // --- Chapter intros / unlocks / final choice ---------------------------------------------------

        public bool HasChapterIntroPlayed(ChapterId chapterId) =>
            progress.introShownForChapter.Contains(chapterId.ToString());

        public void MarkChapterIntroPlayed(ChapterId chapterId)
        {
            string key = chapterId.ToString();
            if (progress.introShownForChapter.Contains(key)) return;
            progress.introShownForChapter.Add(key);
            Save();
        }

        public int HighestUnlockedChapterIndex => progress.highestUnlockedChapterIndex;

        public void UnlockChapter(int chapterIndex)
        {
            if (chapterIndex <= progress.highestUnlockedChapterIndex) return;
            progress.highestUnlockedChapterIndex = Mathf.Min(chapterIndex, (int)ChapterId.SecondChance);
            Save();
            if (batchDepth == 0) JourneyEvents.RaiseProgressChanged();
        }

        public string FinalChoiceId => progress.finalChoiceId;
        public string FinalChoiceLabel => progress.finalChoiceLabel;
        public bool HasFinalChoice => !string.IsNullOrEmpty(progress.finalChoiceId);

        public void SetFinalChoice(string choiceId, string choiceLabel = "")
        {
            Batch(() =>
            {
                progress.finalChoiceId = choiceId;
                progress.finalChoiceLabel = choiceLabel;
                Save();
                JourneyEvents.RaiseFinalChoiceRecorded();
            });
        }

        /// <summary>Wipes all saved progress and starts again from Chapter 1. Settings are untouched.</summary>
        public void StartNewGame()
        {
            progress = new GameProgressData { saveVersion = GameProgressData.CurrentSaveVersion };
            SaveSystem.DeleteProgress();
            CurrentChapterId = ChapterId.None;
            JourneyEvents.RaiseProgressChanged();
        }

        /// <summary>
        /// Test helper: backs the current save up to a separate file (never overwriting an earlier
        /// backup) and then starts from a clean slate. Use RestoreProgressAfterTesting to get it back.
        /// </summary>
        public bool ResetProgressForTesting()
        {
            if (!SaveSystem.BackupAndDeleteProgressForTesting()) return false;
            progress = new GameProgressData { saveVersion = GameProgressData.CurrentSaveVersion };
            CurrentChapterId = ChapterId.None;
            JourneyEvents.RaiseProgressChanged();
            return true;
        }

        public bool RestoreProgressAfterTesting()
        {
            if (!SaveSystem.RestoreTestBackup()) return false;
            progress = SaveSystem.LoadProgress();
            JourneyEvents.RaiseProgressChanged();
            return true;
        }

        // --- Scene flow --------------------------------------------------------------------

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("GameManager.LoadScene called with an empty scene name.");
                return;
            }
            SetPaused(false);
            Time.timeScale = 1f;
            FlushPlayTime();
            CurrentChapterId = ChapterId.None; // ChapterManager sets it again when a chapter scene starts
            SceneManager.LoadScene(sceneName);
        }

        public void ReturnToMainMenu() => LoadScene("MainMenu");

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // --- Pause ---------------------------------------------------------------------------

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused) return;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            PauseStateChanged?.Invoke(paused);
        }

        public void TogglePause() => SetPaused(!IsPaused);
    }
}
