using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LucillesJourney.Core;
using LucillesJourney.Interaction;
using LucillesJourney.Narrative;
using LucillesJourney.UI;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Opens every scene in the build settings and checks for missing scripts and unwired
    /// references on the key gameplay components. Run via the menu, or in batch mode with
    /// -executeMethod LucillesJourney.EditorTools.SceneValidationTool.ValidateAll
    /// </summary>
    public static class SceneValidationTool
    {
        [MenuItem("Lucille's Journey/Validate All Scenes")]
        public static void ValidateAll()
        {
            int totalIssues = 0;
            foreach (var scenePath in EditorBuildSettings.scenes.Select(s => s.path))
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int issues = ValidateScene(scene);
                totalIssues += issues;
                Debug.Log($"[Validate] {scenePath}: {(issues == 0 ? "OK" : issues + " issue(s)")}");
            }

            Debug.Log(totalIssues == 0
                ? "[Validate] All scenes passed with no issues."
                : $"[Validate] Completed with {totalIssues} total issue(s). See warnings above.");
        }

        private static int ValidateScene(Scene scene)
        {
            int issues = 0;
            string name = scene.name;

            foreach (var root in scene.GetRootGameObjects())
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    Debug.LogWarning($"[Validate] {name}: missing script on a GameObject under '{root.name}'.");
                    issues++;
                }
            }

            var chapterManager = Object.FindFirstObjectByType<ChapterManager>(FindObjectsInactive.Include);
            if (chapterManager != null)
            {
                if (chapterManager.ChapterData == null)
                {
                    Debug.LogWarning($"[Validate] {name}: ChapterManager has no ChapterDataSO assigned.");
                    issues++;
                }

                var hud = Object.FindFirstObjectByType<HUDController>(FindObjectsInactive.Include);
                if (hud == null) { Debug.LogWarning($"[Validate] {name}: no HUDController found."); issues++; }

                var narrativePanel = Object.FindFirstObjectByType<NarrativePanelUI>(FindObjectsInactive.Include);
                if (narrativePanel == null) { Debug.LogWarning($"[Validate] {name}: no NarrativePanelUI found."); issues++; }

                var player = Object.FindFirstObjectByType<LucillesJourney.Player.PlayerController2D>(FindObjectsInactive.Include);
                if (player == null) { Debug.LogWarning($"[Validate] {name}: no PlayerController2D found."); issues++; }

                var detector = Object.FindFirstObjectByType<InteractionDetector>(FindObjectsInactive.Include);
                if (detector == null) { Debug.LogWarning($"[Validate] {name}: no InteractionDetector found."); issues++; }

                var pauseMenu = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
                if (pauseMenu == null) { Debug.LogWarning($"[Validate] {name}: no PauseMenuController found."); issues++; }

                var transition = Object.FindFirstObjectByType<ChapterTransitionUI>(FindObjectsInactive.Include);
                if (transition == null)
                {
                    Debug.LogWarning($"[Validate] {name}: no ChapterTransitionUI found.");
                    issues++;
                }
                else
                {
                    var transitionSo = new SerializedObject(transition);
                    var fadeGroupProp = transitionSo.FindProperty("fadeGroup");
                    if (fadeGroupProp.objectReferenceValue == null)
                    {
                        Debug.LogWarning($"[Validate] {name}: ChapterTransitionUI has no fadeGroup assigned - the black fade-in will never clear.");
                        issues++;
                    }
                }

                foreach (var ni in Object.FindObjectsByType<NarrativeInteractable>(FindObjectsSortMode.None))
                {
                    var so = new SerializedObject(ni);
                    var seqProp = so.FindProperty("narrativeSequence");
                    if (seqProp.objectReferenceValue == null)
                    {
                        Debug.LogWarning($"[Validate] {name}: NarrativeInteractable on '{ni.name}' has no narrativeSequence.");
                        issues++;
                    }
                }

                foreach (var interactable in Object.FindObjectsByType<Interactable>(FindObjectsSortMode.None))
                {
                    if (string.IsNullOrEmpty(interactable.InteractionId))
                    {
                        Debug.LogWarning($"[Validate] {name}: Interactable on '{interactable.name}' has no interactionId.");
                        issues++;
                    }
                }
            }

            var mainMenu = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (mainMenu != null)
            {
                var so = new SerializedObject(mainMenu);
                var chaptersProp = so.FindProperty("chapters");
                if (chaptersProp == null || chaptersProp.arraySize == 0)
                {
                    Debug.LogWarning($"[Validate] {name}: MainMenuController has no chapters assigned.");
                    issues++;
                }
            }

            return issues;
        }
    }
}
