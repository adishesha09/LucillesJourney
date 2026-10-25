using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.UI
{
    /// <summary>
    /// The two required adjustable settings: master volume and text size. Both apply live
    /// through SettingsManager and persist across sessions. Self-contained panel - call Show()/Hide()
    /// from whichever menu (main menu or pause menu) owns the button that opens it.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeValueLabel;
        [SerializeField] private Slider textScaleSlider;
        [SerializeField] private TMP_Text textScaleValueLabel;
        [SerializeField] private Button closeButton;

        private bool syncing;

        private void Awake() => WireListeners();

        /// <summary>Used by the editor scene builder to assign references created at build time.</summary>
        public void Wire(GameObject root, Slider volume, TMP_Text volumeLabel, Slider textScale, TMP_Text textScaleLabel, Button close)
        {
            panelRoot = root;
            volumeSlider = volume;
            volumeValueLabel = volumeLabel;
            textScaleSlider = textScale;
            textScaleValueLabel = textScaleLabel;
            closeButton = close;
            WireListeners();
        }

        private void WireListeners()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
            if (textScaleSlider != null)
            {
                textScaleSlider.minValue = 0f;
                textScaleSlider.maxValue = 1f;
                textScaleSlider.onValueChanged.AddListener(HandleTextScaleChanged);
            }
        }

        private void OnEnable()
        {
            SyncFromSettings();
            SettingsManager.SettingsChanged += HandleSettingsChanged;
        }

        private void OnDisable() => SettingsManager.SettingsChanged -= HandleSettingsChanged;

        private void HandleSettingsChanged(GameSettingsData data) => SyncFromSettings();

        private void SyncFromSettings()
        {
            syncing = true;
            if (volumeSlider != null) volumeSlider.value = SettingsManager.MasterVolume;
            UpdateVolumeLabel(SettingsManager.MasterVolume);

            if (textScaleSlider != null) textScaleSlider.value = InverseLerpTextScale(SettingsManager.TextScale);
            UpdateTextScaleLabel(SettingsManager.TextScale);
            syncing = false;
        }

        private void HandleVolumeChanged(float value)
        {
            if (syncing) return;
            SettingsManager.SetMasterVolume(value);
            UpdateVolumeLabel(value);
        }

        private void HandleTextScaleChanged(float sliderValue)
        {
            if (syncing) return;
            float scale = Mathf.Lerp(SettingsManager.MinTextScale, SettingsManager.MaxTextScale, sliderValue);
            SettingsManager.SetTextScale(scale);
            UpdateTextScaleLabel(scale);
        }

        private static float InverseLerpTextScale(float scale) =>
            Mathf.InverseLerp(SettingsManager.MinTextScale, SettingsManager.MaxTextScale, scale);

        private void UpdateVolumeLabel(float value)
        {
            if (volumeValueLabel != null) volumeValueLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateTextScaleLabel(float scale)
        {
            if (textScaleValueLabel != null) textScaleValueLabel.text = $"{scale:0.0}x";
        }

        public void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            SyncFromSettings();
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
