using System;
using UnityEngine;
using UnityEngine.Events;

namespace LucillesJourney.Interaction
{
    /// <summary>
    /// Base component for anything the player can interact with (garden, breakfast, memory fragment, bag...).
    /// Attach to an object that has a Collider2D on the "Interactable" layer.
    /// Later systems (memory, dialogue, packing) listen to the Interacted event or the UnityEvent below.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique ID used later for progress saving, e.g. ch1_garden. Must be unique per scene.")]
        [SerializeField] private string interactionId = "";

        [Header("Prompt")]
        [Tooltip("Verb shown to the player, e.g. Water Garden, Inspect Memory, Pack Item.")]
        [SerializeField] private string promptText = "Interact";

        [Header("Behaviour")]
        [SerializeField] private bool oneTimeOnly = true;
        [SerializeField] private bool startsEnabled = true;
        [SerializeField] private bool logToConsole = false;

        [Header("Reactions (visible feedback goes here)")]
        [SerializeField] private UnityEvent<GameObject> onInteracted;

        private bool consumed;
        private bool enabledForInteraction;

        /// <summary>Raised after a successful interaction.</summary>
        public event Action<Interactable> Interacted;

        /// <summary>Raised for every successful interaction, for scene-wide systems such as audio.</summary>
        public static event Action<Interactable> AnyInteracted;

        public string InteractionId => interactionId;
        public string PromptText => promptText;
        public bool CanInteract => enabledForInteraction && !consumed;

        /// <summary>Used by the editor scene builder to assign identity/prompt at build time.</summary>
        public void Configure(string id, string prompt, bool isOneTimeOnly = true, bool isStartsEnabled = true)
        {
            interactionId = id;
            promptText = prompt;
            oneTimeOnly = isOneTimeOnly;
            startsEnabled = isStartsEnabled;
        }

        private void Awake()
        {
            enabledForInteraction = startsEnabled;
            if (string.IsNullOrWhiteSpace(interactionId))
                Debug.LogWarning($"Interactable '{name}' has no interactionId; progress saving will not track it.", this);
        }

        /// <summary>Called by InteractionDetector. Safe to call repeatedly: one-time events fire once.</summary>
        public void Interact(GameObject interactor)
        {
            if (!CanInteract) return;
            if (oneTimeOnly) consumed = true;

            if (logToConsole) Debug.Log($"Interacted with '{interactionId}'", this);
            onInteracted?.Invoke(interactor);
            Interacted?.Invoke(this);
            AnyInteracted?.Invoke(this);
        }

        /// <summary>Lock/unlock, e.g. the garden is only usable after the chapter begins.</summary>
        public void SetInteractionEnabled(bool value) => enabledForInteraction = value;

        /// <summary>Used by the save system to restore an already-completed interaction.</summary>
        public void MarkConsumed() => consumed = true;

        public void SetPrompt(string newPrompt) => promptText = newPrompt;
    }
}