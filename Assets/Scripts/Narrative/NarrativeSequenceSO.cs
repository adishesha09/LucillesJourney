using UnityEngine;

namespace LucillesJourney.Narrative
{
    /// <summary>
    /// A reusable chunk of narrative content (an interaction's story beat, a chapter intro/outro,
    /// a memory fragment, an ending). Kept as data so writers can edit it without touching code,
    /// and so the real oral-testimony-derived text can be dropped in later without new scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "NarrativeSequence", menuName = "Lucille's Journey/Narrative Sequence")]
    public class NarrativeSequenceSO : ScriptableObject
    {
        [Tooltip("Editor-only identifier, not shown to the player.")]
        public string sequenceName = "";

        public DialogueLine[] lines = new DialogueLine[0];
    }
}
