using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>Main, optional and completed objectives for one chapter, with step progress for multi-step tasks.</summary>
    public class QuestsPage : HubPage
    {
        private const int S = UIFactory.S;

        private ChapterId chapter = ChapterId.OrdinaryLife;
        private TMP_Text chapterLabel, summaryLabel;
        private Button prev, next;
        private RectTransform content;

        public override HubTab Tab => HubTab.Quests;
        public override Selectable DefaultSelection => next != null && next.interactable ? next : prev;

        protected override void Build(RectTransform root)
        {
            UIFactory.VBox(root.gameObject, spacing: 3 * S);

            var header = UIFactory.Rect(root, "Header");
            UIFactory.Size(header.gameObject, height: 20 * S);
            UIFactory.HBox(header.gameObject, spacing: 3 * S, align: TextAnchor.MiddleLeft);
            prev = UIFactory.MakeButton(header, "Prev", null, () => Step(-1), ButtonKind.Secondary, "icon_arrow_left", 26 * S, 20 * S);
            chapterLabel = UIFactory.Text(header, "Chapter", "", TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.Midline);
            UIFactory.Size(chapterLabel.gameObject, flexWidth: 1f);
            next = UIFactory.MakeButton(header, "Next", null, () => Step(1), ButtonKind.Secondary, "icon_arrow_right", 26 * S, 20 * S);

            summaryLabel = UIFactory.Text(root, "Summary", "", TextStyle.Meta, PixelTheme.Sand, TextAlignmentOptions.Midline);
            UIFactory.Size(summaryLabel.gameObject, height: 12 * S);

            content = UIFactory.ScrollView(root, "List", out _);
            UIFactory.VBox(content.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(content.gameObject);
        }

        public override void OnShow()
        {
            var current = GameManager.Instance.CurrentChapterId;
            chapter = current != ChapterId.None
                ? current
                : (ChapterId)Mathf.Clamp(GameManager.Instance.HighestUnlockedChapterIndex, 0, (int)ChapterId.SecondChance);
        }

        private void Step(int direction)
        {
            int highest = Highest();
            int index = Mathf.Clamp((int)chapter + direction, 0, highest);
            chapter = (ChapterId)index;
            Refresh();
        }

        private static int Highest()
        {
            int highest = GameManager.Instance.HighestUnlockedChapterIndex;
            if (GameManager.Instance.CurrentChapterId != ChapterId.None) highest = Mathf.Max(highest, (int)GameManager.Instance.CurrentChapterId);
            return Mathf.Clamp(highest, 0, (int)ChapterId.SecondChance);
        }

        public override void Refresh()
        {
            if ((int)chapter > Highest()) chapter = (ChapterId)Highest();
            var info = JourneyCatalog.GetChapter(chapter);
            var progress = JourneyStats.GetChapter(chapter);
            chapterLabel.text = info.Title;
            summaryLabel.text = $"Main {progress.RequiredDone}/{progress.RequiredTotal}   Optional {progress.OptionalDone}/{progress.OptionalTotal}";
            prev.interactable = (int)chapter > 0;
            next.interactable = (int)chapter < Highest();

            UIFactory.ClearChildren(content);
            var all = JourneyCatalog.ObjectivesFor(chapter).ToList();
            var main = all.Where(o => !o.Optional && !JourneyStats.IsObjectiveDone(o)).ToList();
            var optional = all.Where(o => o.Optional && !JourneyStats.IsObjectiveDone(o)).ToList();
            var done = all.Where(JourneyStats.IsObjectiveDone).ToList();

            Section("Main objectives", main, "All main objectives are complete.");
            Section("Optional", optional, "No optional discoveries left here.");
            Section("Completed", done, "Nothing completed yet.");
        }

        private void Section(string heading, List<ObjectiveInfo> items, string emptyText)
        {
            var h = UIFactory.Text(content, heading + "Heading", heading, TextStyle.Label, PixelTheme.Gold);
            UIFactory.Size(h.gameObject, height: 12 * S);

            if (items.Count == 0)
            {
                var empty = UIFactory.Text(content, "Empty", emptyText, TextStyle.BodySmall, PixelTheme.Muted);
                UIFactory.Size(empty.gameObject, flexWidth: 1f);
                return;
            }
            foreach (var o in items) ObjectiveRow(o);
        }

        private void ObjectiveRow(ObjectiveInfo o)
        {
            bool done = JourneyStats.IsObjectiveDone(o);
            var row = UIFactory.Panel(content, o.Id, "ui_slot_open", done ? new Color(1f, 1f, 1f, 0.72f) : Color.white);
            UIFactory.MakeFocusable(row);
            UIFactory.HBox(row.gameObject, spacing: 4 * S, padding: 4 * S, align: TextAnchor.UpperLeft, expandHeight: false);
            UIFactory.Icon(row.transform, "State", done ? "icon_check" : "icon_box", S);

            var texts = UIFactory.Rect(row.transform, "Texts");
            UIFactory.VBox(texts.gameObject);
            UIFactory.Size(texts.gameObject, flexWidth: 1f);

            var titleColor = done ? PixelTheme.MidBrown : PixelTheme.DeepBrown;
            UIFactory.Text(texts, "Title", o.Title, TextStyle.Body, titleColor);

            if (o.HasSteps)
            {
                var (d, t) = JourneyStats.StepProgress(o);
                UIFactory.Text(texts, "StepCount", $"{d} of {t} steps", TextStyle.Meta, PixelTheme.MidBrown);
                foreach (var stepId in o.StepIds)
                {
                    if (!JourneyCatalog.TryGetObjective(stepId, out var step)) continue;
                    bool stepDone = GameManager.Instance.IsObjectiveComplete(stepId);
                    var stepRow = UIFactory.Rect(texts, "Step");
                    UIFactory.HBox(stepRow.gameObject, spacing: 2 * S, align: TextAnchor.MiddleLeft, expandHeight: false);
                    UIFactory.Icon(stepRow, "State", stepDone ? "icon_check" : "icon_box", 2);
                    UIFactory.Text(stepRow, "Label", step.Title, TextStyle.BodySmall, stepDone ? PixelTheme.MidBrown : PixelTheme.DeepBrown,
                        TextAlignmentOptions.MidlineLeft);
                }
            }
            else if (!done && !string.IsNullOrEmpty(o.Hint))
            {
                UIFactory.Text(texts, "Hint", o.Hint, TextStyle.Meta, PixelTheme.MidBrown);
            }

            if (o.Optional)
            {
                var tag = UIFactory.Text(row.transform, "Tag", "Optional", TextStyle.Meta, PixelTheme.Blue, TextAlignmentOptions.TopRight);
                tag.color = PixelTheme.Hex("3E5A6E");
                    UIFactory.Size(tag.gameObject, 52 * S);
            }
        }
    }
}
