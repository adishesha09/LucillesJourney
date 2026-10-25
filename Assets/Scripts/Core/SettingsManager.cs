using System;
using UnityEngine;

namespace LucillesJourney.Core
{
    /// <summary>
    /// Runtime owner of the two required adjustable settings. Applies master volume immediately
    /// via AudioListener.volume (works with every AudioSource with no mixer setup required) and
    /// broadcasts text-scale changes so any ScalableText component can resize itself live.
    /// </summary>
    public static class SettingsManager
    {
        private static bool initialized;
        private static GameSettingsData current;

        public const float MinTextScale = 0.8f;
        public const float MaxTextScale = 1.4f;

        public static event Action<GameSettingsData> SettingsChanged;

        public static float MasterVolume => Current.masterVolume;
        public static float TextScale => Current.textScale;
        public static bool ReducedMotion => Current.reducedMotion;
        public static float MusicVolume => Current.musicVolume;
        public static float SfxVolume => Current.sfxVolume;

        private static GameSettingsData Current
        {
            get
            {
                EnsureInitialized();
                return current;
            }
        }

        private static void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;
            current = SaveSystem.LoadSettings();
            Apply(current);
        }

        public static void SetMasterVolume(float value)
        {
            EnsureInitialized();
            current.masterVolume = Mathf.Clamp01(value);
            Apply(current);
            SaveSystem.SaveSettings(current);
            SettingsChanged?.Invoke(current);
        }

        public static void SetTextScale(float value)
        {
            EnsureInitialized();
            current.textScale = Mathf.Clamp(value, MinTextScale, MaxTextScale);
            Apply(current);
            SaveSystem.SaveSettings(current);
            SettingsChanged?.Invoke(current);
        }

        public static void SetMusicVolume(float value)
        {
            EnsureInitialized();
            current.musicVolume = Mathf.Clamp01(value);
            SaveSystem.SaveSettings(current);
            SettingsChanged?.Invoke(current);
        }

        public static void SetSfxVolume(float value)
        {
            EnsureInitialized();
            current.sfxVolume = Mathf.Clamp01(value);
            SaveSystem.SaveSettings(current);
            SettingsChanged?.Invoke(current);
        }

        public static void SetReducedMotion(bool value)
        {
            EnsureInitialized();
            current.reducedMotion = value;
            SaveSystem.SaveSettings(current);
            SettingsChanged?.Invoke(current); 
        }

        /// <summary>Re-broadcasts the current settings. Call from OnEnable of UI that needs to sync to live values.</summary>
        public static void RefreshListeners()
        {
            EnsureInitialized();
            SettingsChanged?.Invoke(current);
        }

        private static void Apply(GameSettingsData data)
        {
            AudioListener.volume = data.masterVolume;
        }
    }
}
