using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Enters Play Mode and steps through every scene in Build Settings for a few frames each,
    /// capturing any exceptions/errors logged along the way. Run via the menu, or in batch mode
    /// WITHOUT -quit (this script exits the editor itself once finished) with
    /// -executeMethod LucillesJourney.EditorTools.PlayModeSmokeTest.Run
    /// </summary>
    public static class PlayModeSmokeTest
    {
        private const int FramesPerScene = 6;
        private static readonly List<string> Errors = new List<string>();
        private static int sceneIndex;
        private static int framesInScene;
        private static bool loadRequested;

        [MenuItem("Lucille's Journey/Run Play Mode Smoke Test")]
        public static void Run()
        {
            Errors.Clear();
            sceneIndex = 0;
            framesInScene = 0;
            loadRequested = false;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += OnUpdate;
            EditorApplication.isPlaying = true;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
                Errors.Add($"[{type}] {condition}");
        }

        private static void OnUpdate()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;

            if (!loadRequested)
            {
                // First frame after play mode actually starts (scene 0 loads automatically).
                loadRequested = true;
                Debug.Log($"[SmokeTest] Scene {sceneIndex}: {SceneManager.GetActiveScene().name}");
                return;
            }

            framesInScene++;
            if (framesInScene < FramesPerScene) return;

            sceneIndex++;
            framesInScene = 0;

            if (sceneIndex >= EditorBuildSettings.scenes.Length)
            {
                Finish();
                return;
            }

            Debug.Log($"[SmokeTest] Scene {sceneIndex}: loading...");
            SceneManager.LoadScene(sceneIndex);
            Debug.Log($"[SmokeTest] Scene {sceneIndex}: {SceneManager.GetActiveScene().name}");
        }

        private static void Finish()
        {
            EditorApplication.update -= OnUpdate;
            Application.logMessageReceived -= OnLog;
            EditorApplication.isPlaying = false;

            foreach (var e in Errors) Debug.LogError("[SmokeTest] " + e);
            Debug.Log(Errors.Count == 0
                ? "[SmokeTest] PASSED - no errors or exceptions across all scenes."
                : $"[SmokeTest] FAILED - {Errors.Count} error(s)/exception(s). See above.");

            EditorApplication.Exit(Errors.Count == 0 ? 0 : 1);
        }
    }
}
