using UnityEngine;
using LucillesJourney.Core;

namespace LucillesJourney.World
{
    /// <summary>
    /// Shows or hides an object depending on a persistent environment-state flag. The flag is the same one
    /// GameManager saves, so the right version of the world (dry or watered beds, a laid or cleared table) is
    /// restored on load and changes the moment the player completes the matching objective.
    /// </summary>
    public class EnvironmentStateGate : MonoBehaviour
    {
        [SerializeField] private string stateKey = "";
        [Tooltip("True: visible while the flag is set. False: visible while the flag is NOT set.")]
        [SerializeField] private bool visibleWhenSet = true;

        public string StateKey => stateKey;
        public bool VisibleWhenSet => visibleWhenSet;

        public void Configure(string key, bool whenSet)
        {
            stateKey = key;
            visibleWhenSet = whenSet;
        }

        /// <summary>Parses "key" or "!key" as written in a layout file.</summary>
        public void Configure(string showWhen)
        {
            bool negated = showWhen.StartsWith("!");
            Configure(negated ? showWhen.Substring(1) : showWhen, !negated);
        }

        private void OnEnable()
        {
            JourneyEvents.EnvironmentStateChanged += HandleChanged;
            JourneyEvents.ProgressChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            JourneyEvents.EnvironmentStateChanged -= HandleChanged;
            JourneyEvents.ProgressChanged -= Refresh;
        }

        private void HandleChanged(string key, bool value) => Refresh();

        // The gate keeps its own object active so it can listen; visibility is driven through the renderers.
        private void Refresh()
        {
            if (string.IsNullOrEmpty(stateKey)) return;
            bool visible = GameManager.Instance.IsEnvironmentState(stateKey) == visibleWhenSet;
            foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
        }

        private void Start() => Refresh();

        public bool IsShowing => GetComponent<Renderer>() != null && GetComponent<Renderer>().enabled;
    }
}
