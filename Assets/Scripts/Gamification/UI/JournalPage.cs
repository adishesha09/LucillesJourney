using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// A reading archive of everything Lucille's player has discovered, ordered by chapter: memories
    /// and examined keepsakes. It reuses the Memory Album and Keepsake discovery records and never
    /// shows anything that has not been found.
    /// </summary>
    public class JournalPage : HubPage
    {
        private const int S = UIFactory.S;

        private RectTransform list;
        private TMP_Text emptyLabel, detailTitle, detailMeta;
        private RectTransform detailBody;
        private Image detailArt;
        private Selectable first;
        private string selectedKey;

        public override HubTab Tab => HubTab.Journal;
        public override Selectable DefaultSelection => first;

        protected override void Build(RectTransform root)
        {
            UIFactory.HBox(root.gameObject, spacing: 6 * S, align: TextAnchor.UpperLeft, expandWidth: false, expandHeight: true);

            var left = UIFactory.Rect(root, "Left");
            UIFactory.Size(left.gameObject, 150 * S, flexWidth: 1f, flexHeight: 1f, minWidth: 80 * S);
            UIFactory.VBox(left.gameObject, spacing: 3 * S);
            list = UIFactory.ScrollView(left, "List", out _);
            UIFactory.VBox(list.gameObject, spacing: 2 * S, padding: 2 * S);
            UIFactory.FitHeight(list.gameObject);

            var detail = UIFactory.Rect(root, "Detail");
            UIFactory.Size(detail.gameObject, 150 * S, flexWidth: 1f, flexHeight: 1f, minWidth: 80 * S);
            UIFactory.VBox(detail.gameObject, spacing: 3 * S);

            emptyLabel = UIFactory.Text(detail, "Empty",
                "Nothing has been written here yet. Look closely at the objects around Lucille, and what you find will be kept in this journal.",
                TextStyle.Body, PixelTheme.Muted);

            detailTitle = UIFactory.Text(detail, "Title", "", TextStyle.Heading, PixelTheme.Gold);
            detailMeta = UIFactory.Text(detail, "Meta", "", TextStyle.Meta, PixelTheme.Sand);
            detailArt = UIFactory.Icon(detail, "Art", null, 4);
            var scroll = UIFactory.ScrollView(detail, "BodyScroll", out _);
            UIFactory.VBox(scroll.gameObject, spacing: 2 * S);
            UIFactory.FitHeight(scroll.gameObject);
            detailBody = scroll;
        }

        public override void Refresh()
        {
            if (list == null) return;
            UIFactory.ClearChildren(list);
            first = null;

            var game = GameManager.Instance;
            bool any = false;
            foreach (var chapter in JourneyCatalog.Chapters)
            {
                var memories = JourneyCatalog.MemoriesFor(chapter.Id).Where(m => game.IsMemoryDiscovered(m.Id)).ToList();
                var keepsakes = JourneyCatalog.KeepsakesFor(chapter.Id).Where(k => game.IsKeepsakeCollected(k.Id)).ToList();
                if (memories.Count + keepsakes.Count == 0) continue;
                any = true;

                var heading = UIFactory.Text(list, "Heading", chapter.Title, TextStyle.Label, PixelTheme.Gold);
                UIFactory.Size(heading.gameObject, flexWidth: 1f);

                foreach (var m in memories) AddEntry("memory:" + m.Id, m.Title, "icon_memory", () => ShowMemory(m));
                foreach (var k in keepsakes) AddEntry("keepsake:" + k.Id, k.Title, "icon_keepsake", () => ShowKeepsake(k));
            }

            emptyLabel.gameObject.SetActive(!any);
            detailTitle.gameObject.SetActive(any);
            detailMeta.gameObject.SetActive(any);
            detailArt.gameObject.SetActive(any);
            detailBody.parent.parent.gameObject.SetActive(any);
        }

        private void AddEntry(string key, string title, string icon, System.Action show)
        {
            var button = UIFactory.MakeCardButton(list, key, show);
            UIFactory.HBox(button.gameObject, spacing: 3 * S, padding: 3 * S, align: TextAnchor.MiddleLeft);
            UIFactory.Icon(button.transform, "Icon", icon, 2);
            var label = UIFactory.Text(button.transform, "Title", title, TextStyle.BodySmall, PixelTheme.DeepBrown, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(label.gameObject, flexWidth: 1f);
            button.gameObject.AddComponent<SelectNotifier>().OnSelected = show;
            first ??= button;

            if (selectedKey == null || selectedKey == key)
            {
                selectedKey = key;
                show();
            }
        }

        private void ShowMemory(MemoryInfo m)
        {
            selectedKey = "memory:" + m.Id;
            Show(m.Title, m.Chapter, m.ArtKey, 4, m.Lines.Select(l => l.Speaker == "Narrator" ? l.Text : $"{l.Speaker}: \"{l.Text}\""));
        }

        private void ShowKeepsake(KeepsakeInfo k)
        {
            selectedKey = "keepsake:" + k.Id;
            Show(k.Title, k.Chapter, k.IconKey, 6, new[] { k.Description });
        }

        private void Show(string title, ChapterId chapter, string artKey, int artScale, IEnumerable<string> paragraphs)
        {
            detailTitle.text = title;
            detailMeta.text = JourneyCatalog.GetChapter(chapter)?.Title ?? "";
            var sprite = PixelTheme.Sprite(artKey);
            detailArt.sprite = sprite;
            if (sprite != null)
            {
                var le = detailArt.GetComponent<LayoutElement>();
                le.preferredWidth = sprite.rect.width * artScale;
                le.preferredHeight = sprite.rect.height * artScale;
            }
            UIFactory.ClearChildren(detailBody);
            foreach (var p in paragraphs) UIFactory.Text(detailBody, "Paragraph", p, TextStyle.Body, PixelTheme.Cream);
        }
    }
}
