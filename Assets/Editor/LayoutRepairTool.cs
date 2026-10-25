using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// A "second pass" fix applied after the main scene-build pipeline. The very first
    /// ForceRebuildLayout call in a long batch-mode session - immediately after heavy
    /// AssetDatabase activity such as the placeholder art import, within the same Editor
    /// process - has been observed to silently compute all-zero RectTransform sizes for
    /// VerticalLayoutGroup children. Reopening the affected scene afterwards and re-running
    /// ForceRebuildLayout reliably produces correct values, so this repairs every scene's
    /// layout groups as a guaranteed-correct final step. Run via the menu, or in batch mode
    /// with -executeMethod LucillesJourney.EditorTools.LayoutRepairTool.RepairAll
    /// </summary>
    public static class LayoutRepairTool
    {
        private static readonly string[] ChapterSceneNames =
        {
            "Chapter1_OrdinaryLife", "Chapter2_TheMarket", "Chapter3_BombingAndHiding",
            "Chapter4_Displacement", "Chapter5_Survival", "Chapter6_SecondChance"
        };

        [MenuItem("Lucille's Journey/Repair All Scene Layouts")]
        public static void RepairAll()
        {
            RepairMenuScene("Assets/Scenes/MainMenu.unity", "ButtonColumn");
            RepairMenuScene("Assets/Scenes/Ending.unity", "ButtonColumn");

            foreach (var name in ChapterSceneNames)
                RepairChapterScene($"Assets/Scenes/{name}.unity");

            Debug.Log("[LayoutRepair] All scenes repaired.");
        }

        private static void RepairMenuScene(string path, string layoutRootName)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var root = GameObject.Find(layoutRootName);
            if (root != null)
                SceneBuildUtility.ForceRebuildLayout(root);
            else
                Debug.LogWarning($"[LayoutRepair] {path}: could not find '{layoutRootName}'.");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RepairChapterScene(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var canvas = GameObject.Find("HUDCanvas");
            if (canvas == null)
            {
                Debug.LogWarning($"[LayoutRepair] {path}: no HUDCanvas found.");
                return;
            }

            RepairChild(canvas.transform, "HUD");
            RepairChild(canvas.transform, "SettingsPanel/Box");
            RepairChild(canvas.transform, "InteractionGuidePanel/Box");
            RepairChild(canvas.transform, "PauseMenuPanel/Box");
            RepairChild(canvas.transform, "ChoicePanel/Box"); // only present in Chapter 6; no-op elsewhere

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void RepairChild(Transform parent, string path)
        {
            var child = parent.Find(path);
            if (child != null) SceneBuildUtility.ForceRebuildLayout(child.gameObject);
        }
    }
}
