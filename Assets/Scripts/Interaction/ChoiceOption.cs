using System;
using UnityEngine;
using LucillesJourney.Narrative;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// One option in Lucille's "impossible choice" (school fees / utilities / groceries).
    /// Deliberately has no "correct" option - every choice has a cost, matching the concept
    /// document's intent that this decision should be emotionally difficult, not a puzzle.
    /// </summary>
    [Serializable]
    public class ChoiceOption
    {
        [Tooltip("Stable id saved to the player's progress file, e.g. 'school_fees'.")]
        public string id = "";

        [Tooltip("Shown on the choice button, e.g. 'Pay the school fees'.")]
        public string label = "";

        [Tooltip("Played after the player picks this option, describing the consequence.")]
        public NarrativeSequenceSO consequence;
    }
}
