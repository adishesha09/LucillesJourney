using System;
using UnityEngine;

namespace LucillesJourney.Core
{
    /// <summary>
    /// A single task the player must complete within a chapter, e.g. "Water the garden".
    /// Plain serializable data (not a ScriptableObject) because objectives only make sense
    /// in the context of the chapter that owns them.
    /// </summary>
    [Serializable]
    public class ObjectiveDefinition
    {
        [Tooltip("Must match the interactionId on the Interactable/NarrativeInteractable that completes this objective.")]
        public string objectiveId = "";

        [Tooltip("Shown to the player in the HUD objective list, e.g. 'Water the garden'.")]
        public string description = "";

        [Tooltip("If true this objective also counts as a discoverable memory fragment.")]
        public bool isMemoryFragment = false;

        [Tooltip("Optional objectives (memories, keepsakes) never block chapter progression.")]
        public bool isOptional = false;
    }
}
