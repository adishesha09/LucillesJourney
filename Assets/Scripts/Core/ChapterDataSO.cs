using UnityEngine;
using LucillesJourney.Narrative;

namespace LucillesJourney.Core
{
    /// <summary>
    /// All the data that defines one chapter: which scene it lives in, its objectives,
    /// and the narrative played at its start/end. Keeping this as data (not code) means
    /// the six chapters share one set of systems instead of six bespoke implementations.
    /// </summary>
    [CreateAssetMenu(fileName = "ChapterData", menuName = "Lucille's Journey/Chapter Data")]
    public class ChapterDataSO : ScriptableObject
    {
        [Header("Identity")]
        public ChapterId chapterId = ChapterId.OrdinaryLife;
        public string displayName = "Chapter";

        [Tooltip("Exact scene name (without .unity) to load for this chapter.")]
        public string sceneName = "";

        [Header("Narrative")]
        [Tooltip("Played automatically when the chapter scene loads.")]
        public NarrativeSequenceSO introNarrative;

        [Tooltip("Played once every objective below is complete, before moving to the next chapter.")]
        public NarrativeSequenceSO outroNarrative;

        [Header("Objectives")]
        [Tooltip("Required objectives must be completed before the chapter advances; optional ones (isOptional) never block it. JourneyCatalog is authoritative at runtime; this list mirrors it for reference.")]
        public ObjectiveDefinition[] objectives = new ObjectiveDefinition[0];

        [Header("Progression")]
        [Tooltip("True only for the final chapter (Second Chance), which ends the game instead of loading a next scene.")]
        public bool isFinalChapter = false;

        [Tooltip("Scene to load once this chapter's outro finishes. Ignored if isFinalChapter is true.")]
        public string nextSceneName = "";
    }
}
