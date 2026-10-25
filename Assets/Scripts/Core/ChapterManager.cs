using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LucillesJourney.Gamification;
using LucillesJourney.Gamification.UI;
using LucillesJourney.Interaction;
using LucillesJourney.Narrative;
using LucillesJourney.UI;

namespace LucillesJourney.Core
{
    /// <summary>
    /// Lives in each chapter scene. Plays the chapter intro, reacts to completed objectives, and
    /// once every REQUIRED objective is done plays the outro, records the chapter as completed,
    /// shows the chapter summary and hands off to GameManager to load the next scene.
    /// Optional objectives (memories, keepsakes) never block progression.
    /// </summary>
    public class ChapterManager : MonoBehaviour
    {
        [SerializeField] private ChapterDataSO chapterData;
        [SerializeField] private HUDController hud;
        [SerializeField] private ChapterTransitionUI transition;

        private bool chapterAdvancing;
        private readonly HashSet<string> achievementsAtStart = new HashSet<string>();

        public ChapterDataSO ChapterData => chapterData;

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(ChapterDataSO data, HUDController hudController, ChapterTransitionUI transitionUI)
        {
            chapterData = data;
            hud = hudController;
            transition = transitionUI;
        }

        private void OnEnable() => JourneyEvents.ObjectiveCompleted += HandleObjectiveCompleted;
        private void OnDisable() => JourneyEvents.ObjectiveCompleted -= HandleObjectiveCompleted;

        private void Start()
        {
            if (chapterData == null)
            {
                Debug.LogError("ChapterManager: no ChapterDataSO assigned.", this);
                return;
            }

            if (hud == null) hud = FindFirstObjectByType<HUDController>();
            if (transition == null) transition = FindFirstObjectByType<ChapterTransitionUI>();

            var game = GameManager.Instance;
            game.CurrentChapterId = chapterData.chapterId;
            game.UnlockChapter((int)chapterData.chapterId);

            foreach (var a in AchievementService.All)
                if (game.IsAchievementUnlocked(a.Id)) achievementsAtStart.Add(a.Id);

            RestoreCompletedInteractables();

            if (hud != null)
            {
                hud.SetChapterTitle(chapterData.displayName);
                hud.Refresh();
            }

            if (!game.HasChapterIntroPlayed(chapterData.chapterId))
            {
                var sequence = chapterData.introNarrative;
                NarrativePanelUI.Instance?.Play(sequence, () => game.MarkChapterIntroPlayed(chapterData.chapterId));
                if (NarrativePanelUI.Instance == null) game.MarkChapterIntroPlayed(chapterData.chapterId);
            }

            // In case every objective was already completed before this scene loaded (e.g. reloaded save).
            CheckChapterComplete();
        }

        private void RestoreCompletedInteractables()
        {
            var all = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
            foreach (var interactable in all)
            {
                if (GameManager.Instance.IsObjectiveComplete(interactable.InteractionId))
                    interactable.MarkConsumed();
            }
        }

        /// <summary>
        /// Required objective ids for this chapter. JourneyCatalog is authoritative; the ChapterDataSO
        /// list is only a fallback for a chapter the catalog does not define.
        /// </summary>
        private List<string> RequiredObjectiveIds()
        {
            var fromCatalog = JourneyCatalog.RequiredFor(chapterData.chapterId).Select(o => o.Id).ToList();
            if (fromCatalog.Count > 0) return fromCatalog;
            return chapterData.objectives.Where(o => !o.isOptional).Select(o => o.objectiveId).ToList();
        }

        private void HandleObjectiveCompleted(string objectiveId)
        {
            if (chapterData == null) return;
            if (hud != null) hud.Refresh();
            if (RequiredObjectiveIds().Contains(objectiveId)) CheckChapterComplete();
        }

        private void CheckChapterComplete()
        {
            if (chapterAdvancing || chapterData == null) return;

            var required = RequiredObjectiveIds();
            if (required.Count == 0) return; // scenes without objectives advance manually only
            if (!required.All(GameManager.Instance.IsObjectiveComplete)) return;

            chapterAdvancing = true;

            // Already finished earlier (e.g. the player returned after the story ended): never replay the outro.
            if (GameManager.Instance.IsChapterCompleted(chapterData.chapterId))
            {
                AdvanceToNextScene();
                return;
            }

            if (NarrativePanelUI.Instance != null) NarrativePanelUI.Instance.Play(chapterData.outroNarrative, FinishChapter);
            else FinishChapter();
        }

        private void FinishChapter()
        {
            GameManager.Instance.CompleteChapter(chapterData.chapterId);

            var ui = JourneyUI.Instance;
            if (ui != null)
            {
                var newAchievements = AchievementService.All
                    .Where(a => GameManager.Instance.IsAchievementUnlocked(a.Id) && !achievementsAtStart.Contains(a.Id))
                    .ToList();
                ui.ShowChapterSummary(chapterData.chapterId, newAchievements, AdvanceToNextScene);
            }
            else
            {
                AdvanceToNextScene();
            }
        }

        private void AdvanceToNextScene()
        {
            if (string.IsNullOrEmpty(chapterData.nextSceneName)) return;

            if (transition != null)
                transition.FadeOut(() => GameManager.Instance.LoadScene(chapterData.nextSceneName));
            else
                GameManager.Instance.LoadScene(chapterData.nextSceneName);
        }
    }
}
