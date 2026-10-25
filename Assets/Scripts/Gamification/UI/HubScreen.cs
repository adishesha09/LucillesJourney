using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>One section of the journal hub (quests, memories, ...). Pages are built once and refreshed when shown.</summary>
    public abstract class HubPage
    {
        protected JourneyUI Ui { get; private set; }
        public RectTransform Root { get; private set; }
        public abstract HubTab Tab { get; }
        public abstract Selectable DefaultSelection { get; }

        public void Create(JourneyUI ui, Transform parent)
        {
            Ui = ui;
            Root = UIFactory.Rect(parent, Tab + "Page");
            UIFactory.Stretch(Root);
            Build(Root);
        }

        protected abstract void Build(RectTransform root);
        public virtual void OnShow() { }
        public virtual void Refresh() { }
        public virtual bool HandleBack() => false;
    }

    /// <summary>
    /// The journal hub: a sidebar of sections (quest journal, memory album, keepsakes, achievements,
    /// journey overview, statistics, journal) and the page for the selected section.
    /// </summary>
    public class HubScreen : JourneyScreen
    {
        private const int S = UIFactory.S;

        private sealed class TabInfo
        {
            public HubTab Tab;
            public string Label;
            public string Icon;
            public Button Button;
            public HubPage Page;
        }

        private readonly List<TabInfo> tabs = new List<TabInfo>
        {
            new TabInfo { Tab = HubTab.Quests, Label = "Quests", Icon = "icon_scroll" },
            new TabInfo { Tab = HubTab.Memories, Label = "Memories", Icon = "icon_album" },
            new TabInfo { Tab = HubTab.Keepsakes, Label = "Keepsakes", Icon = "icon_keepsake" },
            new TabInfo { Tab = HubTab.Achievements, Label = "Achievements", Icon = "icon_trophy" },
            new TabInfo { Tab = HubTab.Journey, Label = "Journey", Icon = "icon_journey" },
            new TabInfo { Tab = HubTab.Statistics, Label = "Statistics", Icon = "icon_stats" },
            new TabInfo { Tab = HubTab.Journal, Label = "Journal", Icon = "icon_journal" },
        };

        private RectTransform pageHost;
        private TMP_Text title;
        private Image titleIcon;
        private TabInfo current;

        public HubTab CurrentTab => current != null ? current.Tab : HubTab.Quests;

        protected override Selectable DefaultSelection
        {
            get
            {
                var page = current?.Page?.DefaultSelection;
                return page != null && page.IsInteractable() ? page : current?.Button;
            }
        }

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "Journey", "icon_journey", new Vector2(1780f, 960f), out var titleRow);
            title = titleRow.Find("Title").GetComponent<TMP_Text>();
            titleIcon = titleRow.Find("Icon").GetComponent<Image>();
            UIFactory.HBox(body.gameObject, spacing: 6 * S, expandWidth: false, expandHeight: true);

            var sidebar = UIFactory.Rect(body, "Sidebar");
            UIFactory.Size(sidebar.gameObject, 132 * S, flexHeight: 1f);
            UIFactory.VBox(sidebar.gameObject, spacing: 2 * S);

            foreach (var tab in tabs)
            {
                var t = tab;
                t.Button = UIFactory.MakeButton(sidebar, t.Label + "Tab", t.Label, () => SelectTab(t.Tab, focusButton: true),
                    ButtonKind.Tab, t.Icon, height: 17 * S, align: TextAlignmentOptions.MidlineLeft);
            }

            UIFactory.Spacer(sidebar, flexible: 1f);
            UIFactory.MakeButton(sidebar, "Back", "Back", () => Ui.Back(), ButtonKind.Secondary, "icon_arrow_left", height: 17 * S);
            AddHint(sidebar, JourneyInput.NextTab + " switches section");

            var divider = UIFactory.Solid(body, "Divider", PixelTheme.Tan);
            UIFactory.Size(divider.gameObject, width: S, flexHeight: 1f);

            pageHost = UIFactory.Rect(body, "Pages");
            UIFactory.Size(pageHost.gameObject, flexWidth: 1f, flexHeight: 1f);

            current = tabs[0];
            EnsurePage(current);
            ApplyTabVisuals();
        }

        private void EnsurePage(TabInfo tab)
        {
            if (tab.Page != null) return;
            switch (tab.Tab)
            {
                case HubTab.Quests: tab.Page = new QuestsPage(); break;
                case HubTab.Memories: tab.Page = new MemoriesPage(); break;
                case HubTab.Keepsakes: tab.Page = new KeepsakesPage(); break;
                case HubTab.Achievements: tab.Page = new AchievementsPage(); break;
                case HubTab.Journey: tab.Page = new JourneyPage(); break;
                case HubTab.Statistics: tab.Page = new StatisticsPage(); break;
                default: tab.Page = new JournalPage(); break;
            }
            tab.Page.Create(Ui, pageHost);
            tab.Page.Root.gameObject.SetActive(false);
        }

        public void SelectTab(HubTab tab, bool focusButton = false)
        {
            var target = tabs.Find(t => t.Tab == tab);
            if (target == null) return;
            EnsurePage(target);

            if (current != null && current.Page != null) current.Page.Root.gameObject.SetActive(false);
            current = target;
            current.Page.Root.gameObject.SetActive(true);
            if (focusButton) UISound.Play("ui_tab");
            current.Page.OnShow();
            current.Page.Refresh();
            ApplyTabVisuals();

            if (Root != null && Root.activeInHierarchy && focusButton)
            {
                UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(current.Button.gameObject);
            }
        }

        public void CycleIfOpen(int direction, JourneyUI ui)
        {
            if (ui.CurrentScreen != this) return;
            int index = tabs.IndexOf(current);
            int next = (index + direction + tabs.Count) % tabs.Count;
            SelectTab(tabs[next].Tab, focusButton: true);
        }

        private void ApplyTabVisuals()
        {
            foreach (var tab in tabs)
                UIFactory.SetTabActive(tab.Button, tab == current);
            title.text = current.Label == "Journey" ? "Journey Overview" : current.Label;
            var icon = PixelTheme.Sprite(current.Icon);
            if (icon != null) titleIcon.sprite = icon;
        }

        public override void OnOpen() => current?.Page?.OnShow();

        public override void Refresh() => current?.Page?.Refresh();

        public override bool HandleBack() => current?.Page != null && current.Page.HandleBack();
    }
}
