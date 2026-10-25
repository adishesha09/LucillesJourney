using System.IO;
using UnityEngine;

namespace LucillesJourney.Core
{
    /// <summary>
    /// Pure persistence: reading/writing settings (PlayerPrefs) and progress (a JSON file in
    /// Application.persistentDataPath). Does not apply settings to any system itself -
    /// see SettingsManager for that.
    /// </summary>
    public static class SaveSystem
    {
        private const string VolumeKey = "LJ_MasterVolume";
        private const string TextScaleKey = "LJ_TextScale";
        private const string ReducedMotionKey = "LJ_ReducedMotion";
        private const string MusicVolumeKey = "LJ_MusicVolume";
        private const string SfxVolumeKey = "LJ_SfxVolume";
        private static string progressPathOverride;
        private static string ProgressPath
        {
            get
            {
                if (!string.IsNullOrEmpty(progressPathOverride)) return progressPathOverride;
                // LJ_SAVE_PATH lets automated test runs use a throwaway save file.
                string fromEnvironment = System.Environment.GetEnvironmentVariable("LJ_SAVE_PATH");
                return !string.IsNullOrEmpty(fromEnvironment)
                    ? fromEnvironment
                    : Path.Combine(Application.persistentDataPath, "lucillesjourney_save.json");
            }
        }
        private static string BackupPath => ProgressPath + ".bak";
        private static string TestBackupPath => ProgressPath + ".testbackup";

        public static string SaveFolder => Application.persistentDataPath;

        /// <summary>Redirects progress saving to another file (automated tests use this so a real save is never touched). Pass null to restore.</summary>
        public static void UseProgressPathForTesting(string path) => progressPathOverride = path;

        public static GameSettingsData LoadSettings()
        {
            var defaults = GameSettingsData.Default;
            return new GameSettingsData
            {
                masterVolume = PlayerPrefs.GetFloat(VolumeKey, defaults.masterVolume),
                textScale = PlayerPrefs.GetFloat(TextScaleKey, defaults.textScale),
                musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaults.musicVolume),
                sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaults.sfxVolume),
                reducedMotion = PlayerPrefs.GetInt(ReducedMotionKey, defaults.reducedMotion ? 1 : 0) == 1
            };
        }

        public static void SaveSettings(GameSettingsData data)
        {
            PlayerPrefs.SetFloat(VolumeKey, data.masterVolume);
            PlayerPrefs.SetFloat(TextScaleKey, data.textScale);
            PlayerPrefs.SetFloat(MusicVolumeKey, data.musicVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, data.sfxVolume);
            PlayerPrefs.SetInt(ReducedMotionKey, data.reducedMotion ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads progress, falling back to the last good backup if the main file is unreadable.
        /// A corrupted file is preserved (renamed *.corrupt) rather than overwritten, so progress
        /// is never silently destroyed.
        /// </summary>
        public static GameProgressData LoadProgress()
        {
            var loaded = TryRead(ProgressPath, quarantineOnFailure: true) ?? TryRead(BackupPath, quarantineOnFailure: false);
            return Sanitize(loaded ?? new GameProgressData());
        }

        private static GameProgressData TryRead(string path, bool quarantineOnFailure)
        {
            if (!File.Exists(path)) return null;
            try
            {
                var data = JsonUtility.FromJson<GameProgressData>(File.ReadAllText(path));
                if (data != null) return data;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SaveSystem: failed to read '{Path.GetFileName(path)}'. {e.Message}");
            }

            if (quarantineOnFailure)
            {
                try { File.Move(path, path + ".corrupt"); }
                catch (System.Exception) { /* leave it in place */ }
            }
            return null;
        }

        public static void SaveProgress(GameProgressData data)
        {
            try
            {
                data.saveVersion = GameProgressData.CurrentSaveVersion;
                string json = JsonUtility.ToJson(data, true);
                string temp = ProgressPath + ".tmp";
                File.WriteAllText(temp, json);
                if (File.Exists(ProgressPath)) File.Copy(ProgressPath, BackupPath, true);
                File.Copy(temp, ProgressPath, true);
                File.Delete(temp);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SaveSystem: failed to save progress. {e.Message}");
            }
        }

        /// <summary>
        /// Makes any loaded save safe and consistent: null lists replaced, duplicates/blank ids
        /// removed, values clamped, and original (version 0/1) saves migrated by inferring which
        /// chapters were already completed from the furthest chapter reached.
        /// </summary>
        public static GameProgressData Sanitize(GameProgressData data)
        {
            data.completedObjectiveIds = Clean(data.completedObjectiveIds);
            data.discoveredMemoryIds = Clean(data.discoveredMemoryIds);
            data.introShownForChapter = Clean(data.introShownForChapter);
            data.discoveredKeepsakeIds = Clean(data.discoveredKeepsakeIds);
            data.completedChapterIds = Clean(data.completedChapterIds);
            data.unlockedAchievementIds = Clean(data.unlockedAchievementIds);
            data.environmentStates = Clean(data.environmentStates);
            data.viewedEntryIds = Clean(data.viewedEntryIds);
            data.finalChoiceId ??= "";
            data.finalChoiceLabel ??= "";
            data.highestUnlockedChapterIndex = Mathf.Clamp(data.highestUnlockedChapterIndex, 0, (int)ChapterId.SecondChance);
            if (float.IsNaN(data.playTimeSeconds) || data.playTimeSeconds < 0f) data.playTimeSeconds = 0f;

            if (data.saveVersion < 2)
            {
                // Reaching a later chapter means every earlier chapter's story was finished.
                for (int i = 0; i < data.highestUnlockedChapterIndex; i++)
                {
                    string id = ((ChapterId)i).ToString();
                    if (!data.completedChapterIds.Contains(id)) data.completedChapterIds.Add(id);
                }
                data.saveVersion = GameProgressData.CurrentSaveVersion;
            }
            return data;
        }

        private static System.Collections.Generic.List<string> Clean(System.Collections.Generic.List<string> source)
        {
            var result = new System.Collections.Generic.List<string>();
            if (source == null) return result;
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var entry in source)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;
                string trimmed = entry.Trim();
                if (seen.Add(trimmed)) result.Add(trimmed);
            }
            return result;
        }

        /// <summary>Deletes all saved progress (used by New Game).</summary>
        public static void DeleteProgress()
        {
            if (File.Exists(ProgressPath)) File.Delete(ProgressPath);
            if (File.Exists(BackupPath)) File.Delete(BackupPath);
        }

        /// <summary>
        /// Test-only reset: copies the current save to a *.testbackup file first, then clears progress.
        /// Returns false if a test backup already exists (so an earlier backup is never overwritten).
        /// </summary>
        public static bool BackupAndDeleteProgressForTesting()
        {
            if (File.Exists(TestBackupPath)) return false;
            if (File.Exists(ProgressPath)) File.Copy(ProgressPath, TestBackupPath, false);
            DeleteProgress();
            return true;
        }

        /// <summary>Restores the save made by BackupAndDeleteProgressForTesting. Returns false if there is none.</summary>
        public static bool RestoreTestBackup()
        {
            if (!File.Exists(TestBackupPath)) return false;
            File.Copy(TestBackupPath, ProgressPath, true);
            File.Delete(TestBackupPath);
            return true;
        }
    }
}
