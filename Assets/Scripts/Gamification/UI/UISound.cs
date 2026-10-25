using System.Collections.Generic;
using UnityEngine;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// Interface and one-shot sound effects. Clips are loaded by name from Resources/Audio; if a clip is
    /// missing, a small generated tone is used for the core feedback sounds so the game is never silent.
    /// Loudness follows Master volume (AudioListener) and the Effects volume setting.
    /// </summary>
    public static class UISound
    {
        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
        private static readonly HashSet<string> Missing = new HashSet<string>();
        private static AudioSource source;
        private static float suppressUntil;

        public static void Click() => Play("ui_click");
        public static void Hover() => Play("ui_hover", 0.5f, respectSuppression: true);

        /// <summary>Quiets hover/focus ticks briefly, e.g. while a screen opens and focus jumps to its first control.</summary>
        public static void Suppress(float seconds) => suppressUntil = Mathf.Max(suppressUntil, Time.unscaledTime + seconds);

        public static void Play(string name, float volume = 1f, bool respectSuppression = false)
        {
            if (respectSuppression && Time.unscaledTime < suppressUntil) return;
            var clip = Get(name);
            if (clip == null) return;
            EnsureSource();
            source.PlayOneShot(clip, Mathf.Clamp01(volume) * SettingsManager.SfxVolume);
        }

        private static AudioClip Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Clips.TryGetValue(name, out var cached) && cached != null) return cached;
            if (Missing.Contains(name)) return null;

            var clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip == null) clip = Fallback(name);
            if (clip == null) { Missing.Add(name); return null; }

            Clips[name] = clip;
            return clip;
        }

        private static void EnsureSource()
        {
            if (source != null) return;
            var go = new GameObject("UISound");
            Object.DontDestroyOnLoad(go);
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
        }

        // --- Generated fallbacks (only for the core feedback sounds) ------------------------------------

        private static AudioClip Fallback(string name)
        {
            switch (name)
            {
                case "ui_click": return Tone(new[] { 660f }, 0.05f, 0.25f, square: true);
                case "ui_objective":
                case "ui_achievement": return Tone(new[] { 880f, 1320f }, 0.45f, 0.3f);
                case "ui_memory":
                case "ui_keepsake": return Tone(new[] { 660f, 880f, 1320f }, 0.6f, 0.3f);
                default: return null;
            }
        }

        private static AudioClip Tone(float[] frequencies, float duration, float amplitude, bool square = false)
        {
            const int rate = 22050;
            int samples = Mathf.CeilToInt(rate * duration);
            var data = new float[samples];
            int segment = Mathf.Max(1, samples / frequencies.Length);

            for (int i = 0; i < samples; i++)
            {
                int note = Mathf.Min(i / segment, frequencies.Length - 1);
                float t = (i % segment) / (float)rate;
                float phase = 2f * Mathf.PI * frequencies[note] * t;
                float wave = square ? Mathf.Sign(Mathf.Sin(phase)) * 0.6f : Mathf.Sin(phase);
                float attack = Mathf.Clamp01(t / 0.005f);
                float decay = Mathf.Exp(-6f * (i % segment) / segment);
                data[i] = wave * attack * decay * amplitude;
            }

            var clip = AudioClip.Create("ui_tone", samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
