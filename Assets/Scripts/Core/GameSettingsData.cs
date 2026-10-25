using System;

namespace LucillesJourney.Core
{
    /// <summary>Plain data for the adjustable settings. Persisted via PlayerPrefs.</summary>
    [Serializable]
    public struct GameSettingsData
    {
        public float masterVolume;
        public float textScale;

        /// <summary>Background music and ambience level, applied on top of the master volume.</summary>
        public float musicVolume;

        /// <summary>Sound effects and interface sounds level, applied on top of the master volume.</summary>
        public float sfxVolume;

        /// <summary>Replaces UI motion (slides, pops, fades) with instant state changes.</summary>
        public bool reducedMotion;

        public static GameSettingsData Default => new GameSettingsData
        {
            masterVolume = 1f,
            textScale = 1f,
            musicVolume = 0.7f,
            sfxVolume = 1f,
            reducedMotion = false
        };
    }
}
