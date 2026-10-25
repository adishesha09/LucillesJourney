using TMPro;
using UnityEngine;
using LucillesJourney.Core;

namespace LucillesJourney.UI
{
    /// <summary>
    /// Legacy marker for text that follows the player's text-size setting. The setting now scales the whole
    /// interface through the canvas scalers (JourneyUI.ApplyInterfaceScale), so this keeps the authored font
    /// size as the baseline and no longer multiplies it. Rebase adjusts the baseline when a font is swapped.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ScalableText : MonoBehaviour
    {
        private TMP_Text label;
        private float baseFontSize;

        private void Awake()
        {
            label = GetComponent<TMP_Text>();
            baseFontSize = label.fontSize;
        }

        private void OnEnable()
        {
            SettingsManager.SettingsChanged += HandleSettingsChanged;
            ApplyScale(SettingsManager.TextScale);
        }

        private void OnDisable()
        {
            SettingsManager.SettingsChanged -= HandleSettingsChanged;
        }

        private void HandleSettingsChanged(GameSettingsData data) => ApplyScale(data.textScale);

        private void ApplyScale(float scale)
        {
            // The text-size setting now zooms the whole interface through the canvas scalers (see JourneyUI),
            // which keeps every layout proportional. Scaling font sizes here as well would double it.
            if (label == null) label = GetComponent<TMP_Text>();
            label.fontSize = baseFontSize;
        }

        /// <summary>Multiplies the 1x baseline (used when a legacy text's font is swapped for a pixel font with different metrics).</summary>
        public void Rebase(float factor)
        {
            if (label == null) label = GetComponent<TMP_Text>();
            baseFontSize = (baseFontSize <= 0f ? label.fontSize : baseFontSize) * factor;
            ApplyScale(SettingsManager.TextScale);
        }
    }
}
