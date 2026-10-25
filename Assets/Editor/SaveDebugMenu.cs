using UnityEditor;
using UnityEngine;
using LucillesJourney.Core;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Safe save-file helpers for development. "Back up and reset" copies the current save to a separate
    /// *.testbackup file first (and refuses to overwrite an earlier backup); "Restore" puts it back.
    /// </summary>
    public static class SaveDebugMenu
    {
        [MenuItem("Lucille's Journey/Save/Back Up and Reset Progress")]
        public static void BackUpAndReset()
        {
            if (!SaveSystem.BackupAndDeleteProgressForTesting())
                Debug.LogWarning("A test backup already exists. Restore it first (Lucille's Journey > Save > Restore Backed-Up Progress).");
            else
                Debug.Log("Progress backed up and cleared. Settings are untouched.");
        }

        [MenuItem("Lucille's Journey/Save/Restore Backed-Up Progress")]
        public static void Restore()
        {
            Debug.Log(SaveSystem.RestoreTestBackup() ? "Backed-up progress restored." : "No backup found.");
        }

        [MenuItem("Lucille's Journey/Save/Open Save Folder")]
        public static void OpenFolder() => EditorUtility.RevealInFinder(SaveSystem.SaveFolder);
    }
}
