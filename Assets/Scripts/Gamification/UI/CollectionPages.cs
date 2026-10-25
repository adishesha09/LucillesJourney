using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// Shared behaviour of the Memory Album and Keepsake Collection: chapter filter, discovery counts,
    /// a grid of cards (open for discovered entries, a closed silhouette card otherwise), a "new" marker
    /// and a reading view. Undiscovered entries never expose their title or text.
    /// </summary>
    public abstract class CollectionPage : HubPage
    {
        protected const int S = UIFactory.S;

        protected struct Entry
        {
            public string Id;
            public ChapterId Chapter;
            public string Title;
            public string ArtKey;
            public string Body;
            public bool Discovered;
        }

        protected abstract string Noun { get; }          // "Memories"
        protected abstract string EntryPrefix { get; }   // "memory"
        protected abstract string ClosedArtKey { get; }
        protected abstract bool TintClosedArt { get; }
        protected abstract int ArtScale { get; }
        protected abstract int DetailArtScale { get; }
        protected abstract Vector2 CardSize { get; }
        protected abstract IEnumerable<Entry> Entries();

        private int filter = -1; // -1 = all chapters
        private RectTransform gridView, detailView, grid, detailBody;
        private TMP_Text countLabel, emptyLabel;
        private Image detailArt;
        private TMP_Text detailTitle, detailMeta;
        private Button backButton;
        private readonly Dictionary<Button, int> filterButtons = new Dictionary<Button, int>();
        private Selectable firstCard;
        private string openEntryId;

        protected virtual int Columns => 5;

        public override Selectable DefaultSelection => firstCard != null && firstCard.IsInteractable() ? firstCard : filterButtons.Keys.FirstOrDefault();

        protected override void Build(RectTransform root)
        {
            // Grid view -------------------------------------------------------------------------------
            gridView = UIFactory.Rect(root, "GridView");
            UIFactory.Stretch(gridView);
            UIFactory.VBox(gridView.gameObject, spacing: 3 * S);

            var filterRow = UIFactory.Rect(gridView, "Filters");
            UIFactory.Size(filterRow.gameObject, height: 18 * S);
            UIFactory.HBox(filterRow.gameObject, spacing: 2 * S, expandWidth: true);
            AddFilter(filterRow, "All", -1);
            for (int i = 0; i < JourneyCatalog.Chapters.Count; i++) AddFilter(filterRow, "Ch " + (i + 1), i);

            countLabel = UIFactory.Text(gridView, "Count", "", TextStyle.Body, PixelTheme.Sand, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(countLabel.gameObject, height: 14 * S);

            var scrollRoot = UIFactory.ScrollView(gridView, "Scroll", out _);
            UIFactory.VBox(scrollRoot.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(scrollRoot.gameObject);

            grid = UIFactory.Rect(scrollRoot, "Grid");
            UIFactory.Grid(grid.gameObject, CardSize, new Vector2(4 * S, 4 * S), Columns, responsive: true);
            UIFactory.FitHeight(grid.gameObject);

            emptyLabel = UIFactory.Text(scrollRoot, "Empty", "", TextStyle.Body, PixelTheme.Muted);
            UIFactory.Size(emptyLabel.gameObject, flexWidth: 1f);

            // Detail view -----------------------------------------------------------------------------
            detailView = UIFactory.Rect(root, "DetailView");
            UIFactory.Stretch(detailView);
            UIFactory.VBox(detailView.gameObject, spacing: 4 * S);

            var main = UIFactory.Rect(detailView, "Main");
            UIFactory.Size(main.gameObject, flexWidth: 1f, flexHeight: 1f);
            UIFactory.HBox(main.gameObject, spacing: 6 * S, align: TextAnchor.UpperLeft, expandHeight: false);

            var artFrame = UIFactory.Panel(main, "ArtFrame", "ui_slot_open");
            UIFactory.VBox(artFrame.gameObject, padding: 4 * S, align: TextAnchor.MiddleCenter, expandWidth: false);
            detailArt = UIFactory.Icon(artFrame.transform, "Art", null, DetailArtScale);

            var textColumn = UIFactory.Rect(main, "Text");
            UIFactory.Size(textColumn.gameObject, flexWidth: 1f, flexHeight: 1f);
            UIFactory.VBox(textColumn.gameObject, spacing: 2 * S);
            detailTitle = UIFactory.Text(textColumn, "Title", "", TextStyle.Heading, PixelTheme.Gold);
            detailMeta = UIFactory.Text(textColumn, "Meta", "", TextStyle.Meta, PixelTheme.Sand);
            var bodyScroll = UIFactory.ScrollView(textColumn, "BodyScroll", out _);
            UIFactory.VBox(bodyScroll.gameObject, spacing: 2 * S);
            UIFactory.FitHeight(bodyScroll.gameObject);
            detailBody = bodyScroll;

            backButton = UIFactory.MakeButton(detailView, "Back", "Back to " + Noun.ToLower(), CloseDetail, ButtonKind.Primary,
                "icon_arrow_left", 110 * S, 20 * S);
            detailView.gameObject.SetActive(false);
        }

        private void AddFilter(RectTransform parent, string label, int chapterIndex)
        {
            var button = UIFactory.MakeButton(parent, label.Replace(" ", ""), label, () =>
            {
                filter = chapterIndex;
                Refresh();
            }, ButtonKind.Tab, null, height: 18 * S);
            filterButtons[button] = chapterIndex;
        }

        public override void OnShow()
        {
            if (detailView != null && detailView.gameObject.activeSelf) CloseDetail();
        }

        public override bool HandleBack()
        {
            if (detailView == null || !detailView.gameObject.activeSelf) return false;
            CloseDetail();
            return true;
        }

        public override void Refresh()
        {
            if (grid == null) return;
            if (detailView.gameObject.activeSelf) return; // reading: nothing to rebuild

            var entries = Entries().ToList();
            var shown = entries.Where(e => filter < 0 || (int)e.Chapter == filter).ToList();

            foreach (var pair in filterButtons)
                UIFactory.SetTabActive(pair.Key, pair.Value == filter);

            int found = entries.Count(e => e.Discovered);
            string scope = filter < 0 ? "" : $"   Chapter {filter + 1}: {shown.Count(e => e.Discovered)} of {shown.Count}" +
                (shown.Count > 0 ? $" ({JourneyStats.Percent((float)shown.Count(e => e.Discovered) / shown.Count)})" : "");
            countLabel.text = $"{Noun} found: {found} of {entries.Count}{scope}";

            UIFactory.ClearChildren(grid);
            firstCard = null;

            bool chapterReached = filter < 0 || JourneyStats.IsChapterUnlocked((ChapterId)filter);
            emptyLabel.gameObject.SetActive(shown.Count == 0 || !chapterReached || (shown.All(e => !e.Discovered) && filter >= 0));
            emptyLabel.text = !chapterReached ? "This chapter has not been reached yet."
                : shown.Count == 0 ? "Nothing to show here."
                : shown.All(e => !e.Discovered) ? "Nothing found in this chapter yet. Look closely at the world around Lucille." : "";

            foreach (var entry in shown) BuildCard(entry);
        }

        private void BuildCard(Entry e)
        {
            var viewedKey = EntryPrefix + ":" + e.Id;
            bool isNew = e.Discovered && !GameManager.Instance.IsEntryViewed(viewedKey);

            Image card;
            if (e.Discovered)
            {
                card = UIFactory.Panel(grid, e.Id, "ui_slot_open", raycast: true);
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                button.transition = Selectable.Transition.ColorTint;
                var colors = button.colors;
                colors.highlightedColor = new Color(1f, 0.96f, 0.8f, 1f);
                colors.selectedColor = new Color(1f, 0.9f, 0.55f, 1f);
                colors.pressedColor = new Color(0.9f, 0.8f, 0.5f, 1f);
                button.colors = colors;
                button.onClick.AddListener(() => { UISound.Click(); OpenDetail(e); });
                if (firstCard == null) firstCard = button;
            }
            else
            {
                card = UIFactory.Panel(grid, e.Id, "ui_slot");
            }

            UIFactory.VBox(card.gameObject, spacing: 2 * S, padding: 3 * S, align: TextAnchor.MiddleCenter, expandWidth: false);

            string artKey = e.Discovered ? e.ArtKey : (TintClosedArt ? e.ArtKey : ClosedArtKey);
            var art = UIFactory.Icon(card.transform, "Art", artKey, ArtScale);
            // Undiscovered keepsakes show their object's real sprite as a flat black silhouette.
            if (!e.Discovered && TintClosedArt) art.color = new Color(0.07f, 0.07f, 0.08f, 1f);

            var title = UIFactory.Text(card.transform, "Title", e.Discovered ? e.Title : "Undiscovered", TextStyle.BodySmall,
                e.Discovered ? PixelTheme.DeepBrown : PixelTheme.Muted, TextAlignmentOptions.Top);
            UIFactory.Size(title.gameObject, flexWidth: 1f);

            if (!e.Discovered)
            {
                var lockIcon = UIFactory.Icon(card.transform, "Lock", "icon_lock", 2);
                var rt = lockIcon.rectTransform;
                lockIcon.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                UIFactory.Anchor(rt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-2 * S, -2 * S), new Vector2(32f, 32f));
            }

            if (isNew)
            {
                var badge = UIFactory.Icon(card.transform, "New", "ui_badge_new", 2);
                badge.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                UIFactory.Anchor(badge.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-2 * S, -2 * S), new Vector2(32f, 32f));
            }
        }

        private void OpenDetail(Entry e)
        {
            openEntryId = e.Id;
            GameManager.Instance.MarkEntryViewed(EntryPrefix + ":" + e.Id);

            gridView.gameObject.SetActive(false);
            detailView.gameObject.SetActive(true);

            var sprite = PixelTheme.Sprite(e.ArtKey);
            detailArt.sprite = sprite;
            if (sprite != null)
            {
                var size = new Vector2(sprite.rect.width, sprite.rect.height) * DetailArtScale;
                var le = detailArt.GetComponent<LayoutElement>();
                le.preferredWidth = size.x;
                le.preferredHeight = size.y;
            }

            detailTitle.text = e.Title;
            var chapter = JourneyCatalog.GetChapter(e.Chapter);
            detailMeta.text = chapter != null ? chapter.Title : "";

            UIFactory.ClearChildren(detailBody);
            foreach (var paragraph in e.Body.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(paragraph)) continue;
                UIFactory.Text(detailBody, "Paragraph", paragraph, TextStyle.Body, PixelTheme.Cream);
            }

            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(backButton.gameObject);
        }

        private void CloseDetail()
        {
            detailView.gameObject.SetActive(false);
            gridView.gameObject.SetActive(true);
            Refresh();

            // Return focus to the card that was open so keyboard users keep their place.
            Selectable target = null;
            foreach (Transform child in grid)
                if (child.name == openEntryId) target = child.GetComponent<Selectable>();
            target ??= DefaultSelection;
            if (target != null) UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(target.gameObject);
        }
    }

    public class MemoriesPage : CollectionPage
    {
        public override HubTab Tab => HubTab.Memories;
        protected override string Noun => "Memories";
        protected override string EntryPrefix => "memory";
        protected override string ClosedArtKey => "mem_unknown";
        protected override bool TintClosedArt => false;
        protected override int ArtScale => 3;
        protected override int DetailArtScale => 6;
        protected override Vector2 CardSize => new Vector2(78 * S, 82 * S);

        protected override IEnumerable<Entry> Entries() =>
            JourneyCatalog.Memories.Select(m => new Entry
            {
                Id = m.Id, Chapter = m.Chapter, Title = m.Title, ArtKey = m.ArtKey,
                Body = string.Join("\n", m.Lines.Select(l => l.Speaker == "Narrator" ? l.Text : $"{l.Speaker}: \"{l.Text}\"")),
                Discovered = GameManager.Instance.IsMemoryDiscovered(m.Id)
            });
    }

    public class KeepsakesPage : CollectionPage
    {
        public override HubTab Tab => HubTab.Keepsakes;
        protected override string Noun => "Keepsakes";
        protected override string EntryPrefix => "keepsake";
        protected override string ClosedArtKey => "";
        protected override bool TintClosedArt => true;
        protected override int ArtScale => 4;
        protected override int DetailArtScale => 8;
        protected override Vector2 CardSize => new Vector2(78 * S, 82 * S);

        protected override IEnumerable<Entry> Entries() =>
            JourneyCatalog.Keepsakes.Select(k => new Entry
            {
                Id = k.Id, Chapter = k.Chapter, Title = k.Title, ArtKey = k.IconKey, Body = k.Description,
                Discovered = GameManager.Instance.IsKeepsakeCollected(k.Id)
            });
    }
}
