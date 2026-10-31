using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>Resume, the journal sections, settings, controls, main menu and quit.</summary>
    public class PauseScreen : JourneyScreen
    {
        private const int S = UIFactory.S;
        private Button resume;
        private TMP_Text chapterLabel;

        protected override Selectable DefaultSelection => resume;

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "Paused", "icon_pause", new Vector2(1040f, 800f), out _);
            UIFactory.VBox(body.gameObject, spacing: 4 * S);

            chapterLabel = UIFactory.Text(body, "Chapter", "", TextStyle.Body, PixelTheme.Sand, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(chapterLabel.gameObject, height: 14 * S);

            resume = UIFactory.MakeButton(body, "Resume", "Resume", () => Ui.CloseAll(), ButtonKind.Primary, "icon_play", height: 22 * S);

            var grid = UIFactory.Rect(body, "Grid");
            UIFactory.Grid(grid.gameObject, new Vector2(486f, 20 * S), new Vector2(4 * S, 3 * S), 2);
            UIFactory.Size(grid.gameObject, height: 4 * (20 * S) + 3 * (3 * S));

            UIFactory.MakeButton(grid, "Quests", "Quest Journal", () => Ui.OpenHub(HubTab.Quests), ButtonKind.Secondary, "icon_scroll");
            UIFactory.MakeButton(grid, "Memories", "Memory Album", () => Ui.OpenHub(HubTab.Memories), ButtonKind.Secondary, "icon_album");
            UIFactory.MakeButton(grid, "Keepsakes", "Keepsakes", () => Ui.OpenHub(HubTab.Keepsakes), ButtonKind.Secondary, "icon_keepsake");
            UIFactory.MakeButton(grid, "Achievements", "Achievements", () => Ui.OpenHub(HubTab.Achievements), ButtonKind.Secondary, "icon_trophy");
            UIFactory.MakeButton(grid, "Journey", "Journey", () => Ui.OpenHub(HubTab.Journey), ButtonKind.Secondary, "icon_journey");
            UIFactory.MakeButton(grid, "Statistics", "Statistics", () => Ui.OpenHub(HubTab.Statistics), ButtonKind.Secondary, "icon_stats");
            UIFactory.MakeButton(grid, "Settings", "Settings", () => Ui.OpenSettings(), ButtonKind.Secondary, "icon_settings");
            UIFactory.MakeButton(grid, "Controls", "Controls", () => Ui.OpenControls(), ButtonKind.Secondary, "icon_help");

            UIFactory.Spacer(body, flexible: 1f);

            var row = UIFactory.Rect(body, "Exit");
            UIFactory.Size(row.gameObject, height: 20 * S);
            UIFactory.HBox(row.gameObject, spacing: 4 * S, expandWidth: true);
            UIFactory.MakeButton(row, "MainMenu", "Main Menu", () => Ui.Confirm("Return to the main menu?",
                "Your progress is saved automatically. You can continue from your latest chapter.", "Main Menu",
                () => GameManager.Instance.ReturnToMainMenu()), ButtonKind.Secondary, "icon_door");

            var quit = UIFactory.MakeButton(row, "Quit", "Quit", () => Ui.Confirm("Quit the game?",
                "Your progress is saved automatically.", "Quit", () => GameManager.Instance.QuitGame()), ButtonKind.Secondary, "icon_close");
            quit.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }

        public override void Refresh()
        {
            var chapter = JourneyCatalog.GetChapter(GameManager.Instance.CurrentChapterId);
            chapterLabel.text = chapter != null ? chapter.Title : "";
        }
    }

    /// <summary>Master volume, text size and reduced motion. Every control changes real behaviour and is saved.</summary>
    public class SettingsScreen : JourneyScreen
    {
        private const int S = UIFactory.S;
        private Slider volume, musicSlider, sfxSlider, textScale;
        private Toggle reduced;
        private TMP_Text volumeValue, musicValue, sfxValue, textValue, preview;
        private Button back;
        private bool syncing;

        protected override Selectable DefaultSelection => volume;

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "Settings", "icon_settings", new Vector2(1100f, 900f), out _);
            UIFactory.VBox(body.gameObject, spacing: 4 * S);
            var content = UIFactory.ScrollView(body, "Scroll", out _);
            UIFactory.VBox(content.gameObject, spacing: 6 * S, padding: 2 * S);
            UIFactory.FitHeight(content.gameObject);

            volume = SliderRow(content, "icon_sound", "Master volume", 0f, 1f, v =>
            {
                if (syncing) return;
                SettingsManager.SetMasterVolume(v);
                UpdateLabels();
                PreviewClick();
            }, out volumeValue);

            musicSlider = SliderRow(content, "icon_sound", "Music volume", 0f, 1f, v =>
            {
                if (syncing) return;
                SettingsManager.SetMusicVolume(v);
                UpdateLabels();
            }, out musicValue);

            sfxSlider = SliderRow(content, "icon_sound", "Effects volume", 0f, 1f, v =>
            {
                if (syncing) return;
                SettingsManager.SetSfxVolume(v);
                UpdateLabels();
                PreviewClick();
            }, out sfxValue);

            // Changing the interface size zooms the whole screen under the pointer, so while the handle is being
            // dragged only the label follows; the size is applied when the handle is released (or at once for keyboard changes).
            textScale = SliderRow(content, "icon_text", "Text size", SettingsManager.MinTextScale, SettingsManager.MaxTextScale, v =>
            {
                if (syncing) return;
                pendingTextScale = v;
                if (!textDrag.IsDragging) CommitTextScale();
                UpdateLabels();
            }, out textValue);
            textDrag = textScale.gameObject.AddComponent<SliderDragState>();
            textDrag.Released = CommitTextScale;

            var toggleRow = UIFactory.Rect(content, "MotionRow");
            UIFactory.Size(toggleRow.gameObject, height: 20 * S);
            UIFactory.HBox(toggleRow.gameObject, spacing: 4 * S, align: TextAnchor.MiddleLeft);
            UIFactory.Icon(toggleRow, "Icon", "icon_motion", S);
            reduced = UIFactory.MakeToggle(toggleRow, "ReducedMotion", "Reduced motion", false, v =>
            {
                if (syncing) return;
                SettingsManager.SetReducedMotion(v);
            });

            var note = UIFactory.Text(content, "Note",
                "Reduced motion replaces sliding, popping and fading with instant changes. Text size scales the whole interface.", TextStyle.Meta, PixelTheme.Sand);
            UIFactory.Size(note.gameObject, flexWidth: 1f);

            var sampleBox = UIFactory.Panel(content, "Sample", "ui_slot_open");
            UIFactory.VBox(sampleBox.gameObject, padding: 5 * S, align: TextAnchor.MiddleLeft);
            preview = UIFactory.Text(sampleBox.transform, "Preview",
                "Ordinary things are what a home is made from.", TextStyle.Body, PixelTheme.DeepBrown);

            back = UIFactory.MakeButton(body, "Back", "Back", () => Ui.Back(), ButtonKind.Primary, "icon_arrow_left", 90 * S, 22 * S);
        }

        private float pendingTextScale;
        private float lastPreview;
        private SliderDragState textDrag;

        private void CommitTextScale() => SettingsManager.SetTextScale(pendingTextScale);

        // Audible feedback while dragging a volume slider, throttled so it does not buzz.
        private void PreviewClick()
        {
            if (Time.unscaledTime - lastPreview < 0.12f) return;
            lastPreview = Time.unscaledTime;
            UISound.Click();
        }

        private void UpdateLabels()
        {
            volumeValue.text = $"{Mathf.RoundToInt(SettingsManager.MasterVolume * 100f)}%";
            musicValue.text = $"{Mathf.RoundToInt(SettingsManager.MusicVolume * 100f)}%";
            sfxValue.text = $"{Mathf.RoundToInt(SettingsManager.SfxVolume * 100f)}%";
            float shownScale = textDrag != null && textDrag.IsDragging ? pendingTextScale : SettingsManager.TextScale;
            textValue.text = $"{shownScale:0.0}x";
        }

        private Slider SliderRow(RectTransform parent, string icon, string label, float min, float max, Action<float> onChanged, out TMP_Text valueLabel)
        {
            var row = UIFactory.Rect(parent, label.Replace(" ", "") + "Row");
            UIFactory.Size(row.gameObject, minHeight: 22 * S);
            var rowLayout = UIFactory.HBox(row.gameObject, spacing: 4 * S, align: TextAnchor.MiddleLeft);
            rowLayout.padding = new RectOffset(0, 6 * S, 0, 0); // keeps the value label clear of the scroll clipping edge
            UIFactory.Icon(row, "Icon", icon, S);

            var name = UIFactory.Text(row, "Label", label, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(name.gameObject, 130 * S);

            var slider = UIFactory.MakeSlider(row, "Slider", min, max, min, onChanged);
            valueLabel = UIFactory.Text(row, "Value", "", TextStyle.Body, PixelTheme.Gold, TextAlignmentOptions.MidlineRight);
            UIFactory.Size(valueLabel.gameObject, 40 * S);
            return slider;
        }

        public override void OnOpen() => Refresh();

        public override void Refresh()
        {
            if (textDrag != null && textDrag.IsDragging) return; // do not fight the handle while it is being dragged
            syncing = true;
            volume.value = SettingsManager.MasterVolume;
            musicSlider.value = SettingsManager.MusicVolume;
            sfxSlider.value = SettingsManager.SfxVolume;
            textScale.value = SettingsManager.TextScale;
            pendingTextScale = SettingsManager.TextScale;
            reduced.isOn = SettingsManager.ReducedMotion;
            syncing = false;
            UpdateLabels();
        }
    }

    /// <summary>Reports whether a slider's handle is currently being dragged, and signals when it is released.</summary>
    public class SliderDragState : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public bool IsDragging { get; private set; }
        public System.Action Released;

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) => IsDragging = true;

        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!IsDragging) return;
            IsDragging = false;
            Released?.Invoke();
        }

        private void OnDisable() => IsDragging = false;
    }

    /// <summary>The real controls, taken from the bindings created in code by the player, detector, dialogue and JourneyUI.</summary>
    public class ControlsScreen : JourneyScreen
    {
        private const int S = UIFactory.S;
        private Button back;
        protected override Selectable DefaultSelection => back;

        private static readonly (string action, string keys)[] Rows =
        {
            ("Move", "W A S D, Arrow keys, or left stick"),
            ("Interact / examine", "E, Space, gamepad A, or click a nearby object"),
            ("Continue narrative", "Space, Enter, gamepad A, or the Continue button"),
            ("Quest Journal", JourneyInput.Journal + " or gamepad Y"),
            ("Memory Album", JourneyInput.Album),
            ("Keepsakes", JourneyInput.Keepsakes),
            ("Switch journal section", JourneyInput.NextTab + " / Shift+" + JourneyInput.NextTab + ", or bumpers"),
            ("Pause / go back", JourneyInput.Pause + ", gamepad Start, or gamepad B to go back"),
            ("Move around menus", "Arrow keys, W A S D, d-pad, or the mouse"),
        };

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "Controls", "icon_help", new Vector2(1500f, 900f), out _);
            UIFactory.VBox(body.gameObject, spacing: 3 * S);
            var scrollContent = UIFactory.ScrollView(body, "Scroll", out _);
            UIFactory.VBox(scrollContent.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(scrollContent.gameObject);

            foreach (var (action, keys) in Rows)
            {
                var row = UIFactory.Rect(scrollContent, "Row");
                UIFactory.Size(row.gameObject, minHeight: 12 * S);
                UIFactory.HBox(row.gameObject, spacing: 4 * S, align: TextAnchor.UpperLeft, expandHeight: false);
                var a = UIFactory.Text(row, "Action", action, TextStyle.Body, PixelTheme.Gold, TextAlignmentOptions.TopLeft);
                UIFactory.Size(a.gameObject, 150 * S, flexWidth: 0.6f, minWidth: 60 * S);
                var k = UIFactory.Text(row, "Keys", keys, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.TopLeft);
                UIFactory.Size(k.gameObject, flexWidth: 1f);
            }

            var tips = UIFactory.Text(scrollContent, "Tips",
                "Walk up to a glowing object to see its prompt, then interact. Optional objects hold memories and keepsakes: " +
                "they are never required to finish a chapter. The top-left note always shows the one task that moves the story on.",
                TextStyle.BodySmall, PixelTheme.Sand);
            UIFactory.Size(tips.gameObject, flexWidth: 1f);

            back = UIFactory.MakeButton(body, "Back", "Back", () => Ui.Back(), ButtonKind.Primary, "icon_arrow_left", 90 * S, 22 * S);
        }
    }

    /// <summary>Who made the game, opened from the main menu's Credits button.</summary>
    public class CreditsScreen : JourneyScreen
    {
        private const int S = UIFactory.S;
        private Button back;
        protected override Selectable DefaultSelection => back;

        private static readonly (string name, string roles)[] Team =
        {
            ("Adishesha Nandkoomar", "Game Designer, Programmer and AR Developer"),
            ("Ayden Willett", "Art Director, Pixel Artist and Designer"),
            ("Mikayla Aucamp", "UX Designer and AR Designer"),
        };

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "Credits", "icon_star", new Vector2(1300f, 800f), out _);
            UIFactory.VBox(body.gameObject, spacing: 3 * S);
            var scrollContent = UIFactory.ScrollView(body, "Scroll", out _);
            UIFactory.VBox(scrollContent.gameObject, spacing: 5 * S, padding: 2 * S);
            UIFactory.FitHeight(scrollContent.gameObject);

            foreach (var (name, roles) in Team)
            {
                var entry = UIFactory.Rect(scrollContent, "Credit");
                UIFactory.VBox(entry.gameObject, spacing: S, expandHeight: false);
                UIFactory.FitHeight(entry.gameObject);
                var n = UIFactory.Text(entry, "Name", name, TextStyle.Heading, PixelTheme.Gold, TextAlignmentOptions.TopLeft);
                UIFactory.Size(n.gameObject, flexWidth: 1f);
                var r = UIFactory.Text(entry, "Roles", roles, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.TopLeft);
                UIFactory.Size(r.gameObject, flexWidth: 1f);
            }

            back = UIFactory.MakeButton(body, "Back", "Back", () => Ui.Back(), ButtonKind.Primary, "icon_arrow_left", 90 * S, 22 * S);
        }
    }

    /// <summary>A small yes/no confirmation used before leaving to the main menu, quitting or starting a new game.</summary>
    public class ConfirmScreen : JourneyScreen
    {
        private const int S = UIFactory.S;
        private TMP_Text title, message;
        private Button confirm, cancel;
        private TMP_Text confirmLabel;
        private Action onConfirm;

        protected override Selectable DefaultSelection => cancel;

        protected override void Build(RectTransform root)
        {
            var frame = UIFactory.Panel(root, "Frame", "ui_panel", raycast: true);
            UIFactory.Anchor(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 0f));
            var clamp = frame.gameObject.AddComponent<ClampSize>();
            clamp.Design = new Vector2(1000f, 0f);
            clamp.ClampHeight = false;
            UIFactory.VBox(frame.gameObject, spacing: 5 * S, padding: 8 * S);
            UIFactory.FitHeight(frame.gameObject);

            title = UIFactory.Text(frame.transform, "Title", "", TextStyle.Heading, PixelTheme.Gold);
            message = UIFactory.Text(frame.transform, "Message", "", TextStyle.Body, PixelTheme.Cream);

            var row = UIFactory.Rect(frame.transform, "Buttons");
            UIFactory.Size(row.gameObject, height: 22 * S);
            UIFactory.HBox(row.gameObject, spacing: 4 * S, expandWidth: true);
            cancel = UIFactory.MakeButton(row, "Cancel", "Cancel", () => Ui.Pop(), ButtonKind.Secondary);
            confirm = UIFactory.MakeButton(row, "Confirm", "Confirm", () =>
            {
                var action = onConfirm;
                Ui.Pop();
                action?.Invoke();
            }, ButtonKind.Primary);
            confirmLabel = confirm.GetComponentInChildren<TMP_Text>();
        }

        public void Setup(string heading, string text, string confirmText, Action confirmed)
        {
            title.text = heading;
            message.text = text;
            confirmLabel.text = confirmText;
            onConfirm = confirmed;
        }
    }
}