using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Interaction;
using LucillesJourney.Narrative;
using LucillesJourney.UI;

namespace LucillesJourney.Gamification.UI
{
    public enum HubTab { Quests, Memories, Keepsakes, Achievements, Journey, Statistics, Journal }

    /// <summary>
    /// The persistent (DontDestroyOnLoad) home of every gamification interface. It owns one overlay
    /// canvas containing the HUD, the progress notifications and a stack of full-screen screens
    /// (pause, journal hub, settings, controls, chapter summary, final choice), plus the keyboard and
    /// gamepad shortcuts. It is created automatically after the first scene loads, so existing scenes
    /// need no changes. Opening any screen pauses the game; closing the last one resumes it.
    /// </summary>
    public class JourneyUI : MonoBehaviour
    {
        public static JourneyUI Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("JourneyUI");
            go.AddComponent<JourneyUI>();
        }

        private Canvas canvas;
        private RectTransform screenLayer;
        private Image dimmer;
        private JourneyHUD hud;
        private ProgressNotificationUI toasts;
        private GameObject ownEventSystem;

        private PauseScreen pauseScreen;
        private HubScreen hubScreen;
        private SettingsScreen settingsScreen;
        private ControlsScreen controlsScreen;
        private ChapterSummaryScreen summaryScreen;
        private FinalChoiceScreen choiceScreen;
        private ConfirmScreen confirmScreen;

        private readonly List<JourneyScreen> stack = new List<JourneyScreen>();
        private readonly List<InputAction> actions = new List<InputAction>();
        private Coroutine transition;

        public bool IsAnyScreenOpen => stack.Count > 0;
        public JourneyScreen CurrentScreen => stack.Count > 0 ? stack[stack.Count - 1] : null;

        /// <summary>Redraws the HUD from current progress (called by ChapterManager when a chapter starts).</summary>
        public void RefreshHud() => hud?.Refresh();

        /// <summary>True while the scene-built Chapter 6 choice panel is showing (only when the new choice screen is unavailable).</summary>
        public bool IsLegacyChoiceVisible => ChoiceUI.Instance != null && ChoiceUI.Instance.IsVisible;

        private bool InChapter => GameManager.Instance.CurrentChapterId != ChapterId.None;
        private bool NarrativeShowing => NarrativePanelUI.Instance != null && NarrativePanelUI.Instance.IsShowing;

        // --- Lifecycle ----------------------------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildCanvas();
            BindInput();
        }

        private void OnEnable()
        {
            JourneyEvents.ObjectiveCompleted += HandleObjectiveCompleted;
            JourneyEvents.MemoryDiscovered += HandleMemoryDiscovered;
            JourneyEvents.KeepsakeCollected += HandleKeepsakeCollected;
            JourneyEvents.AchievementUnlocked += HandleAchievementUnlocked;
            JourneyEvents.ProgressChanged += HandleProgressChanged;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SettingsManager.SettingsChanged += HandleSettingsChanged;
            foreach (var a in actions) a.Enable();
        }

        private void OnDisable()
        {
            JourneyEvents.ObjectiveCompleted -= HandleObjectiveCompleted;
            JourneyEvents.MemoryDiscovered -= HandleMemoryDiscovered;
            JourneyEvents.KeepsakeCollected -= HandleKeepsakeCollected;
            JourneyEvents.AchievementUnlocked -= HandleAchievementUnlocked;
            JourneyEvents.ProgressChanged -= HandleProgressChanged;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SettingsManager.SettingsChanged -= HandleSettingsChanged;
            foreach (var a in actions) a.Disable();
        }

        private float appliedScale = -1f;

        private void HandleSettingsChanged(GameSettingsData data) => ApplyInterfaceScale();

        /// <summary>
        /// The text-size setting zooms the whole interface (text, icons and spacing together) by shrinking
        /// every canvas's reference resolution, so layouts keep their proportions. Panels clamp themselves to
        /// the smaller canvas (ClampSize) and grids reflow (ResponsiveGrid).
        /// </summary>
        public void ApplyInterfaceScale()
        {
            float scale = Mathf.Clamp(SettingsManager.TextScale, SettingsManager.MinTextScale, SettingsManager.MaxTextScale);
            appliedScale = scale;
            var reference = new Vector2(1920f, 1080f) / scale;

            foreach (var scaler in FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) continue;
                if (scaler.referenceResolution != reference) scaler.referenceResolution = reference;
            }
        }

        public float InterfaceScale => appliedScale;

        private void OnDestroy()
        {
            foreach (var a in actions) a.Dispose();
            actions.Clear();
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            EnsureEventSystem();
            LegacyUISkin.ApplyToLoadedScenes();
            ApplyInterfaceScale();
            hud.Refresh();
        }

        private void Update() => hud?.Tick();

        // --- Canvas ----------------------------------------------------------------------------------

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // above every scene-built canvas
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var root = (RectTransform)canvasGo.transform;
            hud = new JourneyHUD();
            hud.Create(this, root);

            toasts = new ProgressNotificationUI(this, root); // created before the screen layer so full-screen screens cover toasts

            dimmer = UIFactory.Solid(root, "Dimmer", PixelTheme.Dim);
            UIFactory.Stretch(dimmer.rectTransform);
            dimmer.raycastTarget = true;
            dimmer.gameObject.SetActive(false);

            screenLayer = UIFactory.Rect(root, "Screens");
            UIFactory.Stretch(screenLayer);
        }

        private void EnsureEventSystem()
        {
            var systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            var foreign = systems.Where(s => s.gameObject != ownEventSystem).ToList();

            if (foreign.Count > 0)
            {
                if (ownEventSystem != null) { Destroy(ownEventSystem); ownEventSystem = null; }
                return;
            }
            if (ownEventSystem != null) return;

            ownEventSystem = new GameObject("JourneyEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            ownEventSystem.transform.SetParent(transform, false);
            ownEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        // --- Input --------------------------------------------------------------------------------------

        private void BindInput()
        {
            AddAction("Back", OnBackOrPause, "<Keyboard>/escape", "<Gamepad>/start");
            AddAction("GamepadBack", ctx => { if (IsAnyScreenOpen) Back(); }, "<Gamepad>/buttonEast");
            AddAction("Journal", _ => ToggleHub(HubTab.Quests), "<Keyboard>/j", "<Gamepad>/buttonNorth");
            AddAction("Album", _ => ToggleHub(HubTab.Memories), "<Keyboard>/m");
            AddAction("Keepsakes", _ => ToggleHub(HubTab.Keepsakes), "<Keyboard>/k");
            AddAction("NextTab", ctx =>
            {
                bool shift = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
                hubScreen?.CycleIfOpen(shift ? -1 : 1, this);
            }, "<Keyboard>/tab");
            AddAction("NextTabPad", _ => hubScreen?.CycleIfOpen(1, this), "<Gamepad>/rightShoulder");
            AddAction("PrevTabPad", _ => hubScreen?.CycleIfOpen(-1, this), "<Gamepad>/leftShoulder");
        }

        private void AddAction(string name, Action<InputAction.CallbackContext> callback, params string[] bindings)
        {
            var action = new InputAction(name, InputActionType.Button);
            foreach (var b in bindings) action.AddBinding(b);
            action.performed += callback;
            actions.Add(action);
        }

        private void OnBackOrPause(InputAction.CallbackContext _)
        {
            if (IsAnyScreenOpen) { Back(); return; }
            if (InChapter) OpenPause();
        }

        private void ToggleHub(HubTab tab)
        {
            if (hubScreen != null && CurrentScreen == hubScreen && hubScreen.CurrentTab == tab) { CloseAll(); return; }
            if (IsAnyScreenOpen && CurrentScreen != hubScreen && CurrentScreen != pauseScreen) return; // not over decisions or settings
            if (!IsAnyScreenOpen && (!InChapter || NarrativeShowing || IsLegacyChoiceVisible)) return;
            OpenHub(tab);
        }

        // --- Screen stack ---------------------------------------------------------------------------------

        public void OpenPause() => Push(pauseScreen ??= CreateScreen(new PauseScreen()));

        public void OpenHub(HubTab tab)
        {
            hubScreen ??= CreateScreen(new HubScreen());
            hubScreen.SelectTab(tab);
            if (CurrentScreen == hubScreen) return;
            Push(hubScreen);
        }

        public void OpenSettings() => Push(settingsScreen ??= CreateScreen(new SettingsScreen()));
        public void OpenControls() => Push(controlsScreen ??= CreateScreen(new ControlsScreen()));

        public void Confirm(string title, string message, string confirmLabel, Action onConfirm)
        {
            confirmScreen ??= CreateScreen(new ConfirmScreen());
            confirmScreen.Setup(title, message, confirmLabel, onConfirm);
            Push(confirmScreen);
        }

        public void ShowChapterSummary(ChapterId chapter, List<AchievementInfo> newAchievements, Action onContinue)
        {
            summaryScreen ??= CreateScreen(new ChapterSummaryScreen());
            summaryScreen.Setup(chapter, newAchievements, onContinue);
            Push(summaryScreen);
        }

        public void ShowFinalChoice(string prompt, IReadOnlyList<ChoiceOption> options, Action<ChoiceOption> onChosen)
        {
            choiceScreen ??= CreateScreen(new FinalChoiceScreen());
            choiceScreen.Setup(prompt, options, onChosen);
            Push(choiceScreen);
        }

        private T CreateScreen<T>(T screen) where T : JourneyScreen
        {
            screen.Create(this, screenLayer);
            return screen;
        }

        private void Push(JourneyScreen screen)
        {
            if (stack.Contains(screen)) return;

            if (stack.Count == 0)
            {
                GameManager.Instance.SetPaused(true);
                dimmer.gameObject.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                stack[stack.Count - 1].Root.SetActive(false);
            }

            stack.Add(screen);
            UISound.Suppress(0.3f);
            UISound.Play("ui_panel_open");
            ShowScreen(screen);
        }

        /// <summary>Handles Back/Escape: lets the screen consume it (detail views), otherwise closes the screen.</summary>
        public void Back()
        {
            var top = CurrentScreen;
            if (top == null) return;
            if (top.HandleBack()) return;
            if (!top.CanDismiss) return;
            Pop();
        }

        public void Pop()
        {
            if (stack.Count == 0) return;
            var top = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            top.Root.SetActive(false);
            UISound.Suppress(0.3f);
            UISound.Play("ui_panel_close");

            if (stack.Count == 0)
            {
                dimmer.gameObject.SetActive(false);
                EventSystem.current?.SetSelectedGameObject(null);
                GameManager.Instance.SetPaused(false);
                return;
            }
            ShowScreen(stack[stack.Count - 1]);
        }

        public void CloseAll()
        {
            while (stack.Count > 0) Pop();
        }

        private void ShowScreen(JourneyScreen screen)
        {
            screen.Root.SetActive(true);
            screen.Root.transform.SetAsLastSibling();
            screen.OnOpen();
            screen.Refresh();
            Canvas.ForceUpdateCanvases();
            screen.Focus();

            if (transition != null) StopCoroutine(transition);
            transition = StartCoroutine(FadeIn(screen));
        }

        private IEnumerator FadeIn(JourneyScreen screen)
        {
            var group = screen.Group;
            var rt = (RectTransform)screen.Root.transform;
            if (SettingsManager.ReducedMotion)
            {
                group.alpha = 1f;
                rt.localScale = Vector3.one;
                yield break;
            }

            const float duration = 0.14f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = t / duration;
                group.alpha = k;
                rt.localScale = Vector3.one * Mathf.Lerp(0.97f, 1f, k);
                yield return null;
            }
            group.alpha = 1f;
            rt.localScale = Vector3.one;
        }

        // --- Gameplay event handlers -----------------------------------------------------------------------

        private void HandleObjectiveCompleted(string id)
        {
            if (!JourneyCatalog.TryGetObjective(id, out var info)) return;
            // Memories and keepsakes announce themselves; their objective toast would be a duplicate.
            if (info.Optional) return;
            toasts.Objective(info);
        }

        private void HandleMemoryDiscovered(string id)
        {
            if (JourneyCatalog.TryGetMemory(id, out var memory)) toasts.Memory(memory);
        }

        private void HandleKeepsakeCollected(string id)
        {
            if (JourneyCatalog.TryGetKeepsake(id, out var keepsake)) toasts.Keepsake(keepsake);
        }

        private void HandleAchievementUnlocked(string id)
        {
            var info = AchievementService.Get(id);
            if (info != null) toasts.Achievement(info);
        }

        private void HandleProgressChanged()
        {
            hud?.Refresh();
            foreach (var screen in stack) screen.Refresh();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CloseAll();
            EnsureEventSystem();
            LegacyUISkin.Apply(scene);
            ApplyInterfaceScale();
            hud?.Refresh();
        }
    }
}
