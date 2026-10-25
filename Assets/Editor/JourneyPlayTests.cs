using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Gamification;
using LucillesJourney.Gamification.UI;
using LucillesJourney.Interaction;
using LucillesJourney.Narrative;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// End-to-end Play Mode verification of the gamification layer, driven through the real game objects
    /// (interactables, narrative panel, ChapterManager, JourneyUI screens). It uses a throwaway save file
    /// (LJ_SAVE_PATH), restores any settings it touches, and writes screenshots of every major screen.
    /// Run in batch mode WITHOUT -quit and WITHOUT -nographics:
    ///   Unity -batchmode -projectPath P -logFile L -executeMethod LucillesJourney.EditorTools.JourneyPlayTests.Run
    /// Output: Logs/journey_tests.txt and Screenshots/*.png under the project folder.
    /// </summary>
    [InitializeOnLoad]
    public static class JourneyPlayTests
    {
        private const string RunningKey = "LJ_PlayTests_Running";
        private static readonly List<string> Results = new List<string>();
        private static int failures;
        private static IEnumerator routine;
        private static int waitFrames;
        private static string shotDir;

        static JourneyPlayTests()
        {
            if (SessionState.GetBool(RunningKey, false)) EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void Run()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            Environment.SetEnvironmentVariable("LJ_SAVE_PATH", Path.Combine(root, "Temp", "journey_test_save.json"));
            foreach (var f in Directory.GetFiles(Path.Combine(root, "Temp"), "journey_test_save.json*")) File.Delete(f);

            SessionState.SetBool(RunningKey, true);
            EditorSceneManager.OpenScene("Assets/Scenes/Chapter1_OrdinaryLife.unity", OpenSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            string root = Directory.GetParent(Application.dataPath).FullName;
            shotDir = Path.Combine(root, "Screenshots");
            Directory.CreateDirectory(shotDir);
            Application.logMessageReceived += OnLog;
            routine = Steps();
            EditorApplication.update += Tick;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                if (stackTrace != null && stackTrace.Contains("UnityEditor.Search")) return; // editor indexer noise, not the game
                failures++;
                Results.Add($"FAIL [log {type}] {condition}\n{stackTrace}");
            }
        }

        private static readonly Stack<IEnumerator> Stack = new Stack<IEnumerator>();

        private static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            if (waitFrames > 0) { waitFrames--; return; }
            try
            {
                if (Stack.Count == 0) Stack.Push(routine);
                while (Stack.Count > 0)
                {
                    var top = Stack.Peek();
                    if (!top.MoveNext()) { Stack.Pop(); continue; }
                    if (top.Current is IEnumerator nested) { Stack.Push(nested); continue; }
                    if (top.Current is int frames) waitFrames = frames;
                    return;
                }
                Finish();
            }
            catch (Exception e)
            {
                failures++;
                Results.Add($"FAIL [exception] {e}");
                Finish();
            }
        }

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            SessionState.SetBool(RunningKey, false);

            string root = Directory.GetParent(Application.dataPath).FullName;
            Results.Add(failures == 0 ? "ALL CHECKS PASSED" : $"{failures} FAILURE(S)");
            File.WriteAllLines(Path.Combine(root, "Logs", "journey_tests.txt"), Results);
            foreach (var r in Results) Debug.Log("[JourneyTest] " + r);
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        // --- Helpers ---------------------------------------------------------------------------------------

        private static void Check(bool condition, string name)
        {
            if (!condition) failures++;
            Results.Add((condition ? "PASS " : "FAIL ") + name);
        }

        private static Interactable FindInteractable(string id) =>
            UnityEngine.Object.FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(i => i.InteractionId == id);

        private static GameObject Player => GameObject.Find("Player");

        private static void Interact(string id)
        {
            var target = FindInteractable(id);
            if (target == null) { Check(false, $"interactable '{id}' exists in scene"); return; }
            if (!target.CanInteract) { Check(false, $"interactable '{id}' can be interacted with"); return; }
            target.Interact(Player);
        }

        private static IEnumerator ReadNarrative()
        {
            var advance = typeof(NarrativePanelUI).GetMethod("Advance", BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < 12 && NarrativePanelUI.Instance != null && NarrativePanelUI.Instance.IsShowing; i++)
            {
                yield return 2;
                advance.Invoke(NarrativePanelUI.Instance, null);
            }
            yield return 2;
        }

        private static IEnumerator WaitForScene(string sceneName, float maxSeconds = 20f)
        {
            double start = EditorApplication.timeSinceStartup;
            while (SceneManager.GetActiveScene().name != sceneName && EditorApplication.timeSinceStartup - start < maxSeconds) yield return 1;
            yield return 8;
            double settle = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - settle < 1.0) yield return 1; // let the fade-in and Start() calls finish
        }

        private static IEnumerator WaitSeconds(float seconds)
        {
            double start = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - start < seconds) yield return 1;
        }

        // Opens every screen at the given text size, screenshots it, and checks that non-scrolling text stays inside
        // the screen and is not clipped by a box smaller than the text needs.
        private static IEnumerator ScaleSweep(JourneyUI ui, float scale)
        {
            string tag = scale > 1f ? "big" : "small";
            SettingsManager.SetTextScale(scale);
            yield return 4;

            var opens = new List<(string name, Action open)>
            {
                ("pause", () => ui.OpenPause()),
                ("hub_quests", () => ui.OpenHub(HubTab.Quests)),
                ("hub_memories", () => ui.OpenHub(HubTab.Memories)),
                ("hub_keepsakes", () => ui.OpenHub(HubTab.Keepsakes)),
                ("hub_achievements", () => ui.OpenHub(HubTab.Achievements)),
                ("hub_journey", () => ui.OpenHub(HubTab.Journey)),
                ("hub_statistics", () => ui.OpenHub(HubTab.Statistics)),
                ("hub_journal", () => ui.OpenHub(HubTab.Journal)),
                ("settings", () => ui.OpenSettings()),
                ("controls", () => ui.OpenControls()),
                ("confirm", () => ui.Confirm("Return to the main menu?", "Your progress is saved automatically. You can continue from your latest chapter.", "Main Menu", () => { })),
                ("summary", () => ui.ShowChapterSummary(ChapterId.OrdinaryLife, AchievementService.All.Take(3).ToList(), null)),
                ("choice", () => ui.ShowFinalChoice("What does Lucille do with what little money she has?", new[]
                {
                    new ChoiceOption { id = "school_fees", label = "Pay the children's school fees" },
                    new ChoiceOption { id = "utilities", label = "Pay for electricity and water" },
                    new ChoiceOption { id = "groceries", label = "Buy groceries for the week" },
                }, _ => { })),
            };

            foreach (var (name, open) in opens)
            {
                open();
                yield return 6;
                yield return Shot($"scale_{tag}_{name}", () => CheckLayout(ui, $"{tag}/{name}"));
                ui.CloseAll();
                yield return 3;
            }
        }

        private static void CheckLayout(JourneyUI ui, string where)
        {
            var screen = ui.CurrentScreen;
            var canvas = ui.GetComponentInChildren<Canvas>();
            if (screen == null || canvas == null) { Check(false, $"layout {where}: a screen is open"); return; }

            var canvasRect = (RectTransform)canvas.transform;
            var bad = new List<string>();
            var corners = new Vector3[4];
            foreach (var t in screen.Root.GetComponentsInChildren<TMP_Text>(false))
            {
                if (string.IsNullOrWhiteSpace(t.text) || t.GetComponentInParent<ScrollRect>() != null) continue;
                t.rectTransform.GetWorldCorners(corners);
                for (int i = 0; i < 4; i++)
                {
                    var p = canvasRect.InverseTransformPoint(corners[i]);
                    if (p.x < canvasRect.rect.xMin - 2f || p.x > canvasRect.rect.xMax + 2f || p.y < canvasRect.rect.yMin - 2f || p.y > canvasRect.rect.yMax + 2f)
                    {
                        bad.Add($"'{Trim(t.text)}' is off screen");
                        break;
                    }
                }
                if (t.preferredHeight > t.rectTransform.rect.height + 3f) bad.Add($"'{Trim(t.text)}' is taller ({t.preferredHeight:0}) than its box ({t.rectTransform.rect.height:0})");
            }
            Check(bad.Count == 0, $"layout {where}: no clipped or off-screen text" + (bad.Count > 0 ? " -> " + string.Join("; ", bad.Take(4)) : ""));
        }

        private static string Trim(string s) => s.Length > 24 ? s.Substring(0, 24) : s;

        private static GameObject FindActive(string id) =>
            UnityEngine.Object.FindObjectsByType<Interactable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(i => i.InteractionId == id)?.gameObject;

        private static Button FindButton(JourneyScreen screen, string name) =>
            screen.Root.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == name);

        private static IEnumerator Shot(string name, Action whileRendered = null)
        {
            yield return 3;
            double settle = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - settle < 0.7) yield return 1; // let real-time fades finish
            var cam = Camera.main;
            if (cam == null) { Results.Add("WARN no camera for screenshot " + name); yield break; }

            var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            var changed = new List<(Canvas canvas, RenderMode mode, Camera cam)>();
            foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (c.renderMode != RenderMode.ScreenSpaceOverlay || !c.isRootCanvas) continue;
                changed.Add((c, c.renderMode, c.worldCamera));
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 1f;
            }

            var previousTarget = cam.targetTexture;
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            yield return 1;
            cam = Camera.main;
            if (cam == null) { UnityEngine.Object.Destroy(rt); yield break; }
            cam.targetTexture = rt;
            cam.Render();
            whileRendered?.Invoke(); // canvases are laid out for the 1920x1080 capture at this point

            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(shotDir, name + ".png"), tex.EncodeToPNG());

            cam.targetTexture = previousTarget;
            foreach (var (canvas, mode, previousCam) in changed)
            {
                canvas.renderMode = mode;
                canvas.worldCamera = previousCam;
            }
            UnityEngine.Object.Destroy(rt);
            UnityEngine.Object.Destroy(tex);
        }

        // --- The test script -------------------------------------------------------------------------------------

        private static IEnumerator Steps()
        {
            yield return 20;
            for (double waitStart = EditorApplication.timeSinceStartup;
                 GameManager.Instance.CurrentChapterId == ChapterId.None && EditorApplication.timeSinceStartup - waitStart < 15d;)
                yield return 1;
            yield return 10;
            var gm = GameManager.Instance;
            Results.Add($"INFO active scene '{SceneManager.GetActiveScene().name}', interactables: {UnityEngine.Object.FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length}");
            var ui = JourneyUI.Instance;

            Check(ui != null, "JourneyUI bootstraps automatically");
            Check(SceneManager.GetActiveScene().name == "Chapter1_OrdinaryLife", "starts in Chapter 1");
            Check(gm.CurrentChapterId == ChapterId.OrdinaryLife, "ChapterManager registers the current chapter");
            Check(!gm.IsObjectiveComplete("ch1_garden"), "fresh save: nothing completed");
            Check(PixelFonts.Heading != null && PixelFonts.Heading.name.Contains("PressStart2P"), "heading pixel font asset created from Press Start 2P");
            Check(PixelFonts.Body != null && PixelFonts.Body.name.Contains("VT323"), "body pixel font asset created from VT323");
            Check(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .All(t => t.font == PixelFonts.Heading || t.font == PixelFonts.Body), "every text in the scene uses a pixel font");
            Check(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length == 1, "exactly one EventSystem");

            // New-game defaults and intro
            yield return StartCoroutine_ReadIntro();
            yield return Shot("01_hud_chapter1");

            // Audio: chapter music + ambience start, follow the scene, and respond to settings
            yield return WaitSeconds(2.5f);
            var audio = LucillesJourney.Sound.AudioManager.Instance;
            Check(audio != null, "AudioManager bootstraps automatically");
            Check(audio != null && audio.CurrentMusic == "music_ch1", "chapter 1 music selected");
            Check(audio != null && audio.CurrentAmbience == "amb_ch1", "chapter 1 ambience selected");
            Check(audio != null && audio.IsMusicAudible, "music is playing and audible");
            Check(audio != null && audio.IsAmbienceAudible, "ambience is playing and audible");

            // Optional interaction: memory discovery updates everything at once
            Interact("ch1_photo");
            yield return ReadNarrative();
            Check(gm.IsObjectiveComplete("ch1_photo"), "optional objective ch1_photo recorded");
            Check(gm.IsMemoryDiscovered("ch1_photo"), "memory ch1_photo discovered");
            Check(gm.IsAchievementUnlocked("first_memory"), "First Memory achievement unlocked by real discovery");
            Check(!gm.IsAchievementUnlocked("life_remembered"), "A Life Remembered NOT unlocked early");
            Check(!gm.IsChapterCompleted(ChapterId.OrdinaryLife), "optional discovery does not complete the chapter");
            yield return 30;
            yield return Shot("02_toasts_memory");

            // Keepsake
            Interact("ch1_ks_wateringcan");
            yield return ReadNarrative();
            Check(gm.IsKeepsakeCollected("ch1_ks_wateringcan"), "keepsake collected");
            Check(gm.IsAchievementUnlocked("first_keepsake"), "Something to Hold achievement unlocked");

            // Visible environmental consequence
            var gardenSprite = FindInteractable("ch1_garden").GetComponent<SpriteRenderer>();
            string before = gardenSprite.sprite != null ? gardenSprite.sprite.name : "";
            Interact("ch1_garden");
            yield return ReadNarrative();
            yield return 30;
            Check(gm.IsObjectiveComplete("ch1_garden"), "ch1_garden completed");
            Check(gm.IsEnvironmentState("ch1_garden_watered"), "garden watered state persisted");
            Check(gardenSprite.sprite != null && gardenSprite.sprite.name != before && gardenSprite.sprite.name.Contains("watered"),
                "garden visibly changed to the watered sprite");
            Check(!FindInteractable("ch1_garden").CanInteract, "completed interactable cannot repeat");

            // Duplicate completion is prevented
            int completedBefore = gm.IsObjectiveComplete("ch1_garden") ? 1 : 0;
            Check(!gm.CompleteObjective("ch1_garden"), "duplicate objective completion returns false");
            Check(!gm.DiscoverMemory("ch1_photo"), "duplicate memory discovery returns false");

            // Pause behaviour
            ui.OpenPause();
            yield return 5;
            Check(gm.IsPaused && Time.timeScale == 0f, "opening pause pauses time");
            Check(ui.CurrentScreen is PauseScreen, "pause screen is current");
            yield return Shot("03_pause");
            ui.Back();
            yield return 5;
            Check(!gm.IsPaused && Time.timeScale == 1f, "closing pause resumes time");

            // Finish the chapter with the last required objective -> outro -> summary
            Interact("ch1_breakfast");
            yield return ReadNarrative(); // breakfast narrative
            yield return 5;
            yield return ReadNarrative(); // chapter outro
            yield return 10;
            Check(gm.IsChapterCompleted(ChapterId.OrdinaryLife), "chapter completed once required objectives are done");
            Check(gm.IsAchievementUnlocked("everyday_rituals"), "Everyday Rituals unlocked (garden + breakfast)");
            Check(ui.CurrentScreen is ChapterSummaryScreen, "chapter summary shown after the outro");
            Check(gm.HighestUnlockedChapterIndex >= 1, "next chapter unlocked");
            yield return Shot("04_chapter_summary");

            var continueButton = FindButton(ui.CurrentScreen, "Continue");
            Check(continueButton != null, "summary has a Continue button");
            continueButton?.onClick.Invoke();
            yield return WaitForScene("Chapter2_TheMarket");
            Check(SceneManager.GetActiveScene().name == "Chapter2_TheMarket", "Continue loads the next chapter");
            Check(!gm.IsPaused && Time.timeScale == 1f, "game is unpaused in the new chapter");
            Check(gm.CurrentChapterId == ChapterId.TheMarket, "current chapter updated");

            // Save persistence: a fresh read of the file matches in-memory progress
            var saved = SaveSystem.LoadProgress();
            Check(saved.completedObjectiveIds.Contains("ch1_garden") && saved.completedObjectiveIds.Contains("ch1_breakfast"), "save file contains completed objectives");
            Check(saved.discoveredMemoryIds.Contains("ch1_photo") && saved.discoveredKeepsakeIds.Contains("ch1_ks_wateringcan"), "save file contains memory and keepsake");
            Check(saved.completedChapterIds.Contains("OrdinaryLife"), "save file contains completed chapter");
            Check(saved.unlockedAchievementIds.Contains("first_memory") && saved.environmentStates.Contains("ch1_garden_watered"), "save file contains achievements and environment state");

            // Populate some more progress for the UI screenshots
            gm.Batch(() =>
            {
                gm.DiscoverMemory("ch2_rumor"); gm.CompleteObjective("ch2_rumor");
                gm.CollectKeepsake("ch2_ks_basket"); gm.CompleteObjective("ch2_ks_basket");
            });
            yield return 40;
            yield return Shot("05_hud_chapter2");

            foreach (var tab in new[] { HubTab.Quests, HubTab.Memories, HubTab.Keepsakes, HubTab.Achievements, HubTab.Journey, HubTab.Statistics, HubTab.Journal })
            {
                ui.OpenHub(tab);
                yield return 6;
                Check(ui.CurrentScreen is HubScreen && gm.IsPaused, $"hub opens on {tab}");
                yield return Shot("06_hub_" + tab);
            }

            // Memory detail view and Back handling
            ui.OpenHub(HubTab.Memories);
            yield return 4;
            var cards = ui.CurrentScreen.Root.GetComponentsInChildren<Button>(true).Where(b => b.name == "ch1_photo").ToList();
            Check(cards.Count == 1, "discovered memory card exists in album");
            cards[0].onClick.Invoke();
            yield return 5;
            Check(gm.IsEntryViewed("memory:ch1_photo"), "opening a memory marks it viewed (new badge cleared)");
            yield return Shot("07_memory_detail");
            ui.Back();
            yield return 4;
            Check(ui.CurrentScreen is HubScreen, "Back from the memory detail returns to the album, not the game");
            ui.CloseAll();
            yield return 4;

            // Settings screen and live effects
            var originalSettings = SaveSystem.LoadSettings();
            ui.OpenSettings();
            yield return 5;
            yield return Shot("08_settings");
            // Settings sliders: dragging a slider must update its label AND the real behaviour immediately
            var sliders = ui.CurrentScreen.Root.GetComponentsInChildren<Slider>(true);
            Check(sliders.Length == 4, "settings screen has master, music, effects and text-size sliders");
            TMP_Text ValueOf(Slider s) => s.transform.parent.Find("Value").GetComponent<TMP_Text>();

            sliders[0].value = 0.3f;
            Check(ValueOf(sliders[0]).text == "30%", "master volume label follows the slider (30%)");
            Check(Mathf.Approximately(SettingsManager.MasterVolume, 0.3f) && Mathf.Approximately(AudioListener.volume, 0.3f), "master volume slider changes AudioListener.volume");
            sliders[0].value = 1f;
            Check(Mathf.Approximately(AudioListener.volume, 1f), "master volume slider raises it again");

            var audio2 = LucillesJourney.Sound.AudioManager.Instance;
            yield return WaitSeconds(1.0f); // let the menu ducking settle
            sliders[1].value = 1f;
            yield return 3;
            float loud = audio2.MusicOutputVolume;
            sliders[1].value = 0.5f;
            yield return 3;
            float half = audio2.MusicOutputVolume;
            sliders[1].value = 0f;
            yield return 3;
            Check(ValueOf(sliders[1]).text == "0%", "music volume label follows the slider (0%)");
            Check(loud > 0.1f && Mathf.Abs(half - loud * 0.5f) < loud * 0.1f && audio2.MusicOutputVolume < 0.0001f,
                $"music output follows the music slider immediately (100%={loud:0.00}, 50%={half:0.00}, 0%={audio2.MusicOutputVolume:0.00})");
            Check(audio2.AmbienceOutputVolume < 0.0001f, "ambience follows the music slider too");
            sliders[1].value = 0.7f;

            sliders[2].value = 0.4f;
            Check(ValueOf(sliders[2]).text == "40%" && Mathf.Approximately(SettingsManager.SfxVolume, 0.4f), "effects volume label and setting follow the slider");
            sliders[2].value = 1f;

            sliders[3].value = 1.4f;
            yield return 3;
            Check(ValueOf(sliders[3]).text == "1.4x", "text size label follows the slider (1.4x)");
            Check(Mathf.Approximately(SettingsManager.TextScale, 1.4f), "text size slider changes the setting");
            var scalers = UnityEngine.Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Check(scalers.Length > 0 && scalers.All(c => Mathf.Abs(c.referenceResolution.x - 1920f / 1.4f) < 0.5f), "text size scales every canvas uniformly");
            sliders[3].value = 1f;
            yield return 3;

            SettingsManager.SetReducedMotion(true);
            Check(SettingsManager.ReducedMotion, "reduced motion setting stored");
            SettingsManager.SetMusicVolume(originalSettings.musicVolume);
            SettingsManager.SetSfxVolume(originalSettings.sfxVolume);
            SettingsManager.SetMasterVolume(originalSettings.masterVolume);
            SettingsManager.SetTextScale(originalSettings.textScale);
            SettingsManager.SetReducedMotion(originalSettings.reducedMotion);
            ui.CloseAll();

            // Layout must hold at the smallest and largest text sizes
            yield return ScaleSweep(ui, 1.4f);
            yield return ScaleSweep(ui, 0.8f);
            SettingsManager.SetTextScale(originalSettings.textScale);
            yield return 4;

            ui.OpenControls();
            yield return 4;
            yield return Shot("09_controls");
            ui.Back();
            ui.Confirm("Return to the main menu?", "Your progress is saved automatically.", "Main Menu", () => { });
            yield return 4;
            yield return Shot("10_confirm");
            ui.CloseAll();
            yield return 4;

            // Chapter 4: multi-step packing, prerequisite unlock and visible removal of packed items
            gm.LoadScene("Chapter4_Displacement");
            yield return WaitForScene("Chapter4_Displacement");
            yield return ReadNarrative();
            var depart = FindInteractable("ch4_depart");
            Check(depart != null && !depart.CanInteract, "departing is locked until the bag is packed");

            // Dialogue box at the largest text size
            var longSequence = ScriptableObject.CreateInstance<NarrativeSequenceSO>();
            longSequence.lines = new[]
            {
                new DialogueLine { speakerName = "Lucille", text = "Documents. A little food. The photograph from the shelf. No room for anything else, and nothing that cannot be carried by two tired hands for a very long way." }
            };
            SettingsManager.SetTextScale(1.4f);
            yield return 6;
            NarrativePanelUI.Instance.Play(longSequence);
            yield return 6;
            {
                var panelRt = (RectTransform)NarrativePanelUI.Instance.transform;
                var panelGroup = NarrativePanelUI.Instance.GetComponent<CanvasGroup>();
                Results.Add($"INFO narrative panel rect={panelRt.rect.size} alpha={panelGroup.alpha} active={NarrativePanelUI.Instance.gameObject.activeInHierarchy} parentRect={((RectTransform)panelRt.parent).rect.size}");
                Check(panelRt.rect.width > 600f && panelRt.rect.height > 150f && panelRt.rect.width <= ((RectTransform)panelRt.parent).rect.width,
                    "dialogue box keeps a sensible size at the largest text size and fits the screen");
                var bodyText = NarrativePanelUI.Instance.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "Body");
                Check(bodyText != null && bodyText.preferredHeight <= bodyText.rectTransform.rect.height + 2f,
                    $"dialogue text fits its box at the largest text size ({(bodyText != null ? bodyText.preferredHeight : 0):0} vs {(bodyText != null ? bodyText.rectTransform.rect.height : 0):0})");
            }
            yield return Shot("scale_big_narrative");
            yield return ReadNarrative();
            SettingsManager.SetTextScale(originalSettings.textScale);
            yield return 6;
            Check(LucillesJourney.Sound.AudioManager.Instance.CurrentMusic == "music_ch4" && LucillesJourney.Sound.AudioManager.Instance.CurrentAmbience == "amb_ch4", "music and ambience follow the chapter (chapter 4)");
            Interact("ch4_pack_documents");
            yield return ReadNarrative();
            Check(!gm.IsObjectiveComplete("ch4_pack"), "packing is not complete after one of three steps");
            Check(JourneyStats.StepProgress(JourneyCatalog.Objectives.First(o => o.Id == "ch4_pack")) == (1, 3), "step progress reads 1 of 3");
            yield return Shot("11_packing_step");
            Interact("ch4_pack_food");
            yield return ReadNarrative();
            Interact("ch4_pack_photo");
            yield return ReadNarrative();
            yield return 10;
            Check(gm.IsObjectiveComplete("ch4_pack"), "parent objective completes when all steps are done");
            Check(gm.IsAchievementUnlocked("ready_to_leave"), "Ready to Leave unlocked");
            Check(FindActive("ch4_pack_food") == null, "packed items are removed from the room");
            Check(depart.CanInteract, "departing unlocks once packing is complete");
            var bag = GameObject.Find("Prop_Bag")?.GetComponent<SpriteRenderer>();
            Check(bag != null && bag.sprite != null && bag.sprite.name.Contains("packed"), "bag prop shows the packed state");

            // Reload the scene: environment state must be restored
            gm.LoadScene("Chapter4_Displacement");
            yield return WaitForScene("Chapter4_Displacement");
            yield return 20;
            Check(FindActive("ch4_pack_food") == null, "packed items stay removed after reloading the scene");
            bag = GameObject.Find("Prop_Bag")?.GetComponent<SpriteRenderer>();
            Check(bag != null && bag.sprite != null && bag.sprite.name.Contains("packed"), "bag state restored after reload");
            Check(FindInteractable("ch4_depart")?.CanInteract == true, "prerequisite state restored after reload");

            // Chapter 6: the final decision
            gm.Batch(() => { gm.CompleteObjective("ch6_settle"); gm.CompleteObjective("ch6_work"); });
            gm.LoadScene("Chapter6_SecondChance");
            yield return WaitForScene("Chapter6_SecondChance");
            yield return ReadNarrative();
            yield return 5;
            Interact("ch6_decision");
            yield return ReadNarrative(); // framing narrative
            yield return 8;
            Check(ui.CurrentScreen is FinalChoiceScreen, "final choice screen opens");
            Check(LucillesJourney.Sound.AudioManager.Instance.CurrentMusic == "music_choice", "the quiet decision track plays during the final choice");
            var confirm = FindButton(ui.CurrentScreen, "Confirm");
            Check(confirm != null && !confirm.interactable, "confirm is disabled until an option is selected");
            yield return Shot("12_final_choice");
            FindButton(ui.CurrentScreen, "utilities")?.onClick.Invoke();
            yield return 4;
            Check(confirm.interactable, "selecting an option enables Confirm");
            Check(!gm.HasFinalChoice, "selecting alone does not record the decision");
            yield return Shot("13_final_choice_selected");
            confirm.onClick.Invoke();
            yield return 6;
            Check(gm.FinalChoiceId == "utilities", "confirmed decision is saved");
            Check(SaveSystem.LoadProgress().finalChoiceId == "utilities", "decision persisted to the save file");
            yield return ReadNarrative(); // consequence
            yield return 6;
            yield return ReadNarrative(); // chapter outro
            yield return 10;
            Check(gm.IsObjectiveComplete("ch6_decision"), "decision objective completed after the consequence");
            Check(ui.CurrentScreen is ChapterSummaryScreen, "final chapter summary shown");
            yield return Shot("14_final_summary");
            FindButton(ui.CurrentScreen, "Continue")?.onClick.Invoke();
            yield return WaitForScene("Ending");
            Check(SceneManager.GetActiveScene().name == "Ending", "decision leads to the ending scene");
            Check(gm.IsAchievementUnlocked("journey_complete"), "Journey Complete unlocked at the end");
            Check(JourneyStats.IsJourneyComplete, "journey reports complete");
            yield return 30;
            yield return Shot("15_ending");

            // Journey overview with real data
            ui.OpenHub(HubTab.Journey);
            yield return 6;
            yield return Shot("16_journey_final");
            ui.CloseAll();

            // Main menu
            gm.LoadScene("MainMenu");
            yield return WaitForScene("MainMenu");
            yield return 30;
            Check(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length >= 6, "main menu shows Begin, Journey, How To Play, Settings, New Journey, Quit");
            yield return Shot("17_main_menu");
            SettingsManager.SetTextScale(1.4f);
            yield return 6;
            yield return Shot("scale_big_main_menu");
            SettingsManager.SetTextScale(0.8f);
            yield return 6;
            yield return Shot("scale_small_main_menu");
            SettingsManager.SetTextScale(SaveSystem.LoadSettings().textScale <= 0f ? 1f : originalSettings.textScale);
            yield return 4;
            ui.OpenHub(HubTab.Journey);
            yield return 6;
            Check(ui.CurrentScreen is HubScreen, "main menu can open the journey overview");
            ui.CloseAll();

            // Save robustness
            yield return SaveRobustness();
        }

        private static IEnumerator StartCoroutine_ReadIntro()
        {
            yield return ReadNarrative();
            yield return 5;
        }

        private static IEnumerator SaveRobustness()
        {
            string path = Environment.GetEnvironmentVariable("LJ_SAVE_PATH");

            // Original-format save (no gamification fields): must load and migrate.
            File.WriteAllText(path, "{\"highestUnlockedChapterIndex\":3,\"completedObjectiveIds\":[\"ch1_garden\",\"ch1_garden\",\"\"],\"discoveredMemoryIds\":[\"ch1_photo\"],\"introShownForChapter\":[]}");
            var migrated = SaveSystem.LoadProgress();
            Check(migrated.completedChapterIds.SequenceEqual(new[] { "OrdinaryLife", "TheMarket", "BombingAndHiding" }), "legacy save migrates: earlier chapters marked complete");
            Check(migrated.completedObjectiveIds.Count == 1, "duplicate and blank ids are removed");
            Check(migrated.discoveredKeepsakeIds != null && migrated.unlockedAchievementIds != null, "missing lists default to empty");
            Check(migrated.saveVersion == GameProgressData.CurrentSaveVersion, "migrated save is stamped with the current version");

            // Corrupted file falls back safely and is preserved, not overwritten.
            File.WriteAllText(path, "{ this is not json");
            File.WriteAllText(path + ".bak", "{\"discoveredMemoryIds\":[\"ch2_rumor\"]}");
            var recovered = SaveSystem.LoadProgress();
            Check(recovered.discoveredMemoryIds.Contains("ch2_rumor"), "corrupted save recovers from the backup");
            Check(File.Exists(path + ".corrupt"), "corrupted save is kept as .corrupt, not deleted");

            // Test-reset helper never destroys the existing save without a backup.
            File.WriteAllText(path, "{\"highestUnlockedChapterIndex\":2}");
            Check(SaveSystem.BackupAndDeleteProgressForTesting(), "test reset creates a backup");
            Check(!File.Exists(path), "test reset clears the save");
            Check(!SaveSystem.BackupAndDeleteProgressForTesting(), "a second reset will not overwrite the first backup");
            Check(SaveSystem.RestoreTestBackup() && SaveSystem.LoadProgress().highestUnlockedChapterIndex == 2, "backup restores the original save");

            // Unknown ids in a save are ignored by statistics.
            File.WriteAllText(path, "{\"discoveredMemoryIds\":[\"ch1_photo\",\"not_a_real_memory\"]}");
            Check(JourneyStats.MemoriesDone <= JourneyStats.MemoriesTotal, "memory count never exceeds the number of defined memories");
            yield return 2;
        }
    }
}
