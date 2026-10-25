using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LucillesJourney.Core;
using LucillesJourney.Gamification;
using LucillesJourney.Gamification.UI;
using LucillesJourney.Interaction;
using LucillesJourney.Player;

namespace LucillesJourney.Sound
{
    /// <summary>
    /// Persistent music, ambience and world-sound player. It follows the active scene (menu, chapter,
    /// ending) and the final-choice screen, crossfading between music tracks, and plays footsteps and
    /// interaction sounds from real gameplay events. Clips are loaded by name from Resources/Audio; a
    /// missing clip is skipped (one warning). Levels follow Master volume (AudioListener) and the
    /// separate Music and Effects settings. Music eases down while a menu is open.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public string CurrentMusic => currentMusic;
        public string CurrentAmbience => currentAmbience;
        public bool IsMusicAudible => musicSources != null && musicSources[activeMusic].isPlaying && musicSources[activeMusic].volume > 0.001f;
        public bool IsAmbienceAudible => ambienceSource != null && ambienceSource.isPlaying && ambienceSource.volume > 0.001f;

        /// <summary>Actual output volume of the active music source (crossfade x Music setting x menu ducking), for testing.</summary>
        public float MusicOutputVolume => musicSources != null ? musicSources[activeMusic].volume : 0f;
        public float AmbienceOutputVolume => ambienceSource != null ? ambienceSource.volume : 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            new GameObject("AudioManager").AddComponent<AudioManager>();
        }

        private const float FadeSeconds = 1.8f;
        private const float MenuDuck = 0.55f;
        private const float AmbienceLevel = 0.55f;

        // Sound played when an interactable with this id is used. Anything not listed gets the soft touch.
        private static readonly Dictionary<string, string> InteractionSounds = new Dictionary<string, string>
        {
            { "ch1_garden", "sfx_water_pour" },
            { "ch1_breakfast", "sfx_plate" },
            { "ch1_ks_wateringcan", "sfx_water_pour" },
            { "ch3_alarm", "sfx_distant_boom" },
            { "ch3_ks_blanket", "sfx_cloth" },
            { "ch3_shelter", "sfx_cloth" },
            { "ch4_pack_documents", "sfx_paper" },
            { "ch4_pack_food", "sfx_bag" },
            { "ch4_pack_photo", "sfx_paper" },
            { "ch4_ks_clothbag", "sfx_bag" },
            { "ch5_water", "sfx_water_jug" },
            { "ch5_ks_jug", "sfx_water_jug" },
            { "ch6_lock", "sfx_key_lock" },
            { "ch6_ks_key", "sfx_key_lock" },
        };

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly HashSet<string> missing = new HashSet<string>();

        private AudioSource[] musicSources;   // two sources for crossfading
        private AudioSource ambienceSource;
        private AudioSource sfxSource;
        private int activeMusic;
        private readonly float[] musicGain = new float[2];
        private float ambienceGain;
        private string currentMusic = "";
        private string currentAmbience = "";
        private string sceneMusic = "";
        private float duck = 1f;
        private float stepTimer;
        private int stepIndex;
        private PlayerController2D player;
        private Rigidbody2D playerBody;
        private float playerSearchTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSources = new[] { MakeSource("MusicA", true), MakeSource("MusicB", true) };
            ambienceSource = MakeSource("Ambience", true);
            sfxSource = MakeSource("Sfx", false);
        }

        private AudioSource MakeSource(string sourceName, bool loop)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = loop ? 0f : 1f;
            source.ignoreListenerPause = true; // keep playing while the game is paused
            return source;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            Interactable.AnyInteracted += HandleInteracted;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Interactable.AnyInteracted -= HandleInteracted;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start() => ChooseForScene(SceneManager.GetActiveScene());

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            player = null;
            playerBody = null;
            ChooseForScene(scene);
        }

        // --- Choosing tracks -----------------------------------------------------------------------------

        private void ChooseForScene(Scene scene)
        {
            string ambience = "";
            var chapter = JourneyCatalog.GetChapterBySceneName(scene.name);
            if (chapter != null)
            {
                sceneMusic = $"music_ch{chapter.Index + 1}";
                ambience = $"amb_ch{chapter.Index + 1}";
            }
            else if (scene.name == "Ending") sceneMusic = "music_ending";
            else sceneMusic = "music_menu";

            SetAmbience(ambience);
        }

        private void Update()
        {
            var ui = JourneyUI.Instance;
            bool deciding = ui != null && ui.CurrentScreen is FinalChoiceScreen;
            string wantedMusic = deciding ? "music_choice" : sceneMusic;
            if (wantedMusic != currentMusic) CrossfadeTo(wantedMusic);

            // Ease the music down while a menu is open, and back up afterwards.
            float duckTarget = ui != null && ui.IsAnyScreenOpen && !deciding ? MenuDuck : 1f;
            duck = Mathf.MoveTowards(duck, duckTarget, Time.unscaledDeltaTime * 1.5f);

            UpdateLevels();
            UpdateFootsteps();
        }

        private void UpdateLevels()
        {
            // Crossfade gains move over time; the Music setting and menu ducking apply instantly on top of them,
            // so dragging the slider changes the loudness at once.
            float level = SettingsManager.MusicVolume * duck;
            float step = Time.unscaledDeltaTime / FadeSeconds;

            for (int i = 0; i < musicSources.Length; i++)
            {
                var source = musicSources[i];
                float target = (i == activeMusic && source.clip != null) ? 1f : 0f;
                musicGain[i] = Mathf.MoveTowards(musicGain[i], target, step);
                source.volume = musicGain[i] * level;
                if (i != activeMusic && source.isPlaying && musicGain[i] <= 0.001f) source.Stop();
            }

            float ambienceTarget = ambienceSource.clip != null && !string.IsNullOrEmpty(currentAmbience) ? 1f : 0f;
            ambienceGain = Mathf.MoveTowards(ambienceGain, ambienceTarget, step * 0.5f);
            ambienceSource.volume = ambienceGain * AmbienceLevel * level;
            if (string.IsNullOrEmpty(currentAmbience) && ambienceSource.isPlaying && ambienceGain <= 0.001f) ambienceSource.Stop();
        }

        private void CrossfadeTo(string name)
        {
            currentMusic = name;
            var clip = Get(name);
            int next = 1 - activeMusic;
            var source = musicSources[next];
            source.Stop();
            source.clip = clip;
            musicGain[next] = 0f;
            source.volume = 0f;
            if (clip != null) source.Play();
            activeMusic = next; // the previous source fades out in UpdateLevels
        }

        private void SetAmbience(string name)
        {
            if (name == currentAmbience) return;
            currentAmbience = name;
            if (string.IsNullOrEmpty(name)) return; // fades out in UpdateLevels

            var clip = Get(name);
            ambienceSource.Stop();
            ambienceSource.clip = clip;
            ambienceGain = 0f;
            ambienceSource.volume = 0f;
            if (clip != null) ambienceSource.Play();
        }

        // --- World sounds ---------------------------------------------------------------------------------

        private void HandleInteracted(Interactable interactable)
        {
            string id = interactable.InteractionId ?? "";
            PlaySfx(InteractionSounds.TryGetValue(id, out var mapped) ? mapped : "sfx_interact");
        }

        public void PlaySfx(string name, float volume = 1f)
        {
            var clip = Get(name);
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * SettingsManager.SfxVolume);
        }

        private void UpdateFootsteps()
        {
            if (GameManager.Instance.CurrentChapterId == ChapterId.None || GameManager.Instance.IsPaused) return;

            if (player == null)
            {
                playerSearchTimer -= Time.unscaledDeltaTime;
                if (playerSearchTimer > 0f) return;
                playerSearchTimer = 1f;
                player = FindFirstObjectByType<PlayerController2D>();
                playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
                return;
            }

            bool moving = player.MovementEnabled && playerBody != null && playerBody.linearVelocity.sqrMagnitude > 0.25f;
            if (!moving) { stepTimer = 0.12f; return; }

            stepTimer -= Time.deltaTime;
            if (stepTimer > 0f) return;
            stepTimer = 0.42f;
            stepIndex = 1 - stepIndex;
            PlaySfx(stepIndex == 0 ? "sfx_footstep_1" : "sfx_footstep_2", 0.45f);
        }

        // --- Clip loading --------------------------------------------------------------------------------

        private AudioClip Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (clips.TryGetValue(name, out var cached) && cached != null) return cached;
            if (missing.Contains(name)) return null;

            var clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip == null)
            {
                missing.Add(name);
                Debug.LogWarning($"AudioManager: no audio clip 'Resources/Audio/{name}'.");
                return null;
            }
            clips[name] = clip;
            return clip;
        }
    }
}
