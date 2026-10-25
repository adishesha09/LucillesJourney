using System;
using UnityEngine;

namespace LucillesJourney.Narrative
{
    /// <summary>
    /// One line of narrative/dialogue text shown in the NarrativePanelUI.
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Who is 'speaking', e.g. Lucille, Narrator. Leave blank for pure narration.")]
        public string speakerName = "";

        [TextArea(2, 6)]
        public string text = "";
    }
}
