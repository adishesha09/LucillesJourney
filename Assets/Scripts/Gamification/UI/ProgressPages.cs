using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>Achievement cards with progress; selecting one shows its details beside the list.</summary>
    public class AchievementsPage : HubPage
    {
        private const int S = UIFactory.S;

        private TMP_Text countLabel, detailTitle, detailDescription, detailStatus, detailProgressLabel;
        private Image detailIcon;
        private PixelBar detailBar;
        private RectTransform list;
        private Selectable first;
        private string selectedId;

        public override HubTab Tab => HubTab.Achievements;
        public override Selectable DefaultSelection => first;

        protected override void Build(RectTransform root)
        {
            UIFactory.HBox(root.gameObject, spacing: 6 * S, align: TextAnchor.UpperLeft, expandWidth: false, expandHeight: true);

            var left = UIFactory.Rect(root, "Left");
            UIFactory.Size(left.gameObject, 170 * S, flexWidth: 1.4f, flexHeight: 1f, minWidth: 90 * S);
            UIFactory.VBox(left.gameObject, spacing: 3 * S);
            countLabel = UIFactory.Text(left, "Count", "", TextStyle.Body, PixelTheme.Sand, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(countLabel.gameObject, height: 14 * S);
            list = UIFactory.ScrollView(left, "List", out _);
            UIFactory.VBox(list.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(list.gameObject);

            var detail = UIFactory.Panel(root, "Detail", "ui_slot_open");
            UIFactory.Size(detail.gameObject, 110 * S, flexWidth: 1f, flexHeight: 1f, minWidth: 70 * S);
            UIFactory.VBox(detail.gameObject, spacing: 3 * S, padding: 6 * S, align: TextAnchor.UpperCenter);

            detailIcon = UIFactory.Icon(detail.transform, "Icon", null, 6);
            detailTitle = UIFactory.Text(detail.transform, "Title", "", TextStyle.Heading, PixelTheme.DeepBrown, TextAlignmentOptions.Top);
            detailStatus = UIFactory.Text(detail.transform, "Status", "", TextStyle.Label, PixelTheme.Terracotta, TextAlignmentOptions.Top);
            detailDescription = UIFactory.Text(detail.transform, "Description", "", TextStyle.Body, PixelTheme.DeepBrown, TextAlignmentOptions.Top);
            UIFactory.Spacer(detail.transform, flexible: 1f);
            detailProgressLabel = UIFactory.Text(detail.transform, "Progress", "", TextStyle.Body, PixelTheme.MidBrown, TextAlignmentOptions.Top);
            detailBar = UIFactory.Bar(detail.transform, "Bar", PixelTheme.Teal);
        }

        public override void OnShow() { }

        public override void Refresh()
        {
            if (list == null) return;
            UIFactory.ClearChildren(list);
            first = null;

            int unlocked = 0;
            foreach (var a in AchievementService.All)
            {
                bool isUnlocked = GameManager.Instance.IsAchievementUnlocked(a.Id);
                if (isUnlocked) unlocked++;
                BuildRow(a, isUnlocked);
            }
            countLabel.text = $"Unlocked: {unlocked} of {AchievementService.All.Count}";

            var current = AchievementService.Get(selectedId) ?? AchievementService.All[0];
            ShowDetail(current);
        }

        private void BuildRow(AchievementInfo a, bool unlocked)
        {
            var button = UIFactory.MakeCardButton(list, a.Id, () => ShowDetail(a), unlocked ? "ui_slot_open" : "ui_slot");
            UIFactory.HBox(button.gameObject, spacing: 4 * S, padding: 3 * S, align: TextAnchor.MiddleLeft);
            first ??= button;
            button.gameObject.AddComponent<SelectNotifier>().OnSelected = () => ShowDetail(a);

            var icon = UIFactory.Icon(button.transform, "Icon", a.IconKey, 2);
            if (!unlocked) icon.color = new Color(0.12f, 0.12f, 0.14f, 1f);

            var texts = UIFactory.Rect(button.transform, "Texts");
            UIFactory.VBox(texts.gameObject, spacing: S);
            UIFactory.Size(texts.gameObject, flexWidth: 1f);
            UIFactory.Text(texts, "Title", a.Title, TextStyle.Body, unlocked ? PixelTheme.DeepBrown : PixelTheme.Muted);

            var (current, target) = AchievementService.Progress(a);
            var line = UIFactory.Text(texts, "Progress", unlocked ? "Unlocked" : $"{current} / {target}", TextStyle.Meta,
                unlocked ? PixelTheme.Hex("3F6B45") : PixelTheme.Muted);
            UIFactory.Size(line.gameObject, height: 9 * S);

            if (unlocked)
            {
                var star = UIFactory.Icon(button.transform, "Star", "icon_star", 2);
                star.raycastTarget = false;
            }
        }

        private void ShowDetail(AchievementInfo a)
        {
            selectedId = a.Id;
            bool unlocked = GameManager.Instance.IsAchievementUnlocked(a.Id);
            var (current, target) = AchievementService.Progress(a);

            var sprite = PixelTheme.Sprite(a.IconKey);
            detailIcon.sprite = sprite;
            detailIcon.color = unlocked ? Color.white : new Color(0.12f, 0.12f, 0.14f, 1f);
            detailTitle.text = a.Title;
            detailStatus.text = unlocked ? "Unlocked" : "Locked";
            detailStatus.color = unlocked ? PixelTheme.Hex("3F6B45") : PixelTheme.Terracotta;
            detailDescription.text = a.Description;
            detailProgressLabel.text = $"Progress: {current} of {target}";
            detailBar.SetValue(target == 0 ? 0f : (float)current / target);
        }
    }

    /// <summary>The six chapters as a connected path, each with its state, story progress and optional exploration.</summary>
    public class JourneyPage : HubPage
    {
        private const int S = UIFactory.S;

        private PixelBar storyBar, exploreBar;
        private TMP_Text storyLabel, exploreLabel, decisionLabel, statusLabel;
        private RectTransform cardsHost;

        public override HubTab Tab => HubTab.Journey;
        public override Selectable DefaultSelection => null;

        protected override void Build(RectTransform root)
        {
            UIFactory.VBox(root.gameObject, spacing: 3 * S);

            var summary = UIFactory.Panel(root, "Summary", "ui_slot_open");
            UIFactory.HBox(summary.gameObject, spacing: 6 * S, padding: 4 * S, align: TextAnchor.MiddleLeft, expandWidth: true, expandHeight: false);
            UIFactory.Size(summary.gameObject, minHeight: 52 * S);

            var story = UIFactory.Rect(summary.transform, "Story");
            UIFactory.VBox(story.gameObject, spacing: S);
            UIFactory.Size(story.gameObject, flexWidth: 1f);
            storyLabel = UIFactory.Text(story, "Label", "", TextStyle.BodySmall, PixelTheme.DeepBrown);
            storyBar = UIFactory.Bar(story, "Bar", PixelTheme.Orange);

            var explore = UIFactory.Rect(summary.transform, "Explore");
            UIFactory.VBox(explore.gameObject, spacing: S);
            UIFactory.Size(explore.gameObject, flexWidth: 1f);
            exploreLabel = UIFactory.Text(explore, "Label", "", TextStyle.BodySmall, PixelTheme.DeepBrown);
            exploreBar = UIFactory.Bar(explore, "Bar", PixelTheme.Blue);

            var status = UIFactory.Rect(summary.transform, "Status");
            UIFactory.VBox(status.gameObject, spacing: S, align: TextAnchor.MiddleLeft);
            UIFactory.Size(status.gameObject, 140 * S);
            statusLabel = UIFactory.Text(status, "Journey", "", TextStyle.BodySmall, PixelTheme.DeepBrown);
            decisionLabel = UIFactory.Text(status, "Decision", "", TextStyle.BodySmall, PixelTheme.MidBrown);

            cardsHost = UIFactory.ScrollView(root, "Cards", out _);
            UIFactory.VBox(cardsHost.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(cardsHost.gameObject);
        }

        public override void Refresh()
        {
            if (cardsHost == null) return;

            storyLabel.text = $"Story: {JourneyStats.MainDone}/{JourneyStats.MainTotal} main ({JourneyStats.Percent(JourneyStats.StoryProgress)})";
            storyBar.SetValue(JourneyStats.StoryProgress);
            exploreLabel.text = $"Exploring: {JourneyStats.OptionalDone}/{JourneyStats.OptionalTotal} optional ({JourneyStats.Percent(JourneyStats.ExplorationProgress)})";
            exploreBar.SetValue(JourneyStats.ExplorationProgress);

            statusLabel.text = JourneyStats.IsJourneyComplete ? "Journey complete" : $"Chapters: {JourneyStats.ChaptersCompleted} of {JourneyStats.ChaptersTotal}";
            var choice = JourneyCatalog.GetChoice(GameManager.Instance.FinalChoiceId);
            decisionLabel.text = choice != null ? $"Decision: {choice.Title}" : "Decision: not yet made";

            UIFactory.ClearChildren(cardsHost);
            var chapters = JourneyStats.AllChapters().ToList();
            for (int rowIndex = 0; rowIndex < 2; rowIndex++)
            {
                var row = UIFactory.Rect(cardsHost, "Row" + rowIndex);
                UIFactory.Size(row.gameObject, flexWidth: 1f, minHeight: 156 * S);
                UIFactory.HBox(row.gameObject, spacing: 0, align: TextAnchor.MiddleCenter, expandWidth: false, expandHeight: true);
                for (int col = 0; col < 3; col++)
                {
                    BuildCard(row, chapters[rowIndex * 3 + col]);
                    if (col < 2) BuildConnector(row);
                }
            }
        }

        private static void BuildConnector(RectTransform row)
        {
            var holder = UIFactory.Rect(row, "Connector");
            UIFactory.Size(holder.gameObject, 22 * S);
            UIFactory.VBox(holder.gameObject, align: TextAnchor.MiddleCenter, expandWidth: false);
            var dots = UIFactory.Rect(holder, "Dots");
            UIFactory.HBox(dots.gameObject, spacing: 2 * S, align: TextAnchor.MiddleCenter, expandWidth: false, expandHeight: false);
            for (int i = 0; i < 3; i++) UIFactory.Icon(dots, "Dot", "ui_path_dot", 2);
        }

        private void BuildCard(RectTransform row, ChapterProgress p)
        {
            bool locked = p.State == ChapterState.Locked;
            var card = UIFactory.Panel(row, p.Info.ShortTitle, locked ? "ui_slot" : "ui_slot_open");
            UIFactory.MakeFocusable(card);
            UIFactory.Size(card.gameObject, 118 * S, flexWidth: 1f, flexHeight: 1f, minWidth: 40 * S);
            UIFactory.VBox(card.gameObject, spacing: 2 * S, padding: 3 * S, align: TextAnchor.UpperCenter);

            var art = UIFactory.Icon(card.transform, "Thumb", p.Info.ThumbnailKey, 2);
            if (locked) art.color = new Color(0.1f, 0.1f, 0.12f, 1f);

            var titleRow = UIFactory.Rect(card.transform, "TitleRow");
            UIFactory.HBox(titleRow.gameObject, spacing: 2 * S, align: TextAnchor.MiddleCenter, expandWidth: false, expandHeight: false);
            if (p.IsCurrent) UIFactory.Icon(titleRow, "Here", "icon_objective", 2);
            var title = UIFactory.Text(titleRow, "Title", $"{p.Info.Index + 1}. {p.Info.ShortTitle}", TextStyle.Body,
                locked ? PixelTheme.Muted : PixelTheme.DeepBrown, TextAlignmentOptions.Center);
            UIFactory.Size(title.gameObject, flexWidth: 1f);

            var (stateText, stateIcon, stateColor) = StateStyle(p.State);
            var stateRow = UIFactory.Rect(card.transform, "State");
            UIFactory.HBox(stateRow.gameObject, spacing: 2 * S, align: TextAnchor.MiddleCenter, expandWidth: false, expandHeight: false);
            UIFactory.Icon(stateRow, "Icon", stateIcon, 2);
            UIFactory.Text(stateRow, "Label", stateText, TextStyle.BodySmall, stateColor, TextAlignmentOptions.Center);

            var desc = UIFactory.Text(card.transform, "Description", locked ? "Not reached yet." : p.Info.Description,
                TextStyle.Meta, locked ? PixelTheme.Muted : PixelTheme.MidBrown, TextAlignmentOptions.Top);
            UIFactory.Size(desc.gameObject, flexWidth: 1f);

            UIFactory.Spacer(card.transform, flexible: 1f);

            if (!locked)
            {
                var story = UIFactory.Text(card.transform, "StoryLabel", $"Story {p.RequiredDone}/{p.RequiredTotal}", TextStyle.Meta, PixelTheme.DeepBrown);
                UIFactory.Size(story.gameObject, height: 9 * S);
                UIFactory.Bar(card.transform, "StoryBar", PixelTheme.Orange, 8 * S).SetValue(p.Story, false);
                var optional = UIFactory.Text(card.transform, "ExploreLabel",
                    $"Optional {p.OptionalDone}/{p.OptionalTotal}", TextStyle.Meta, PixelTheme.DeepBrown);
                UIFactory.Size(optional.gameObject, height: 9 * S);
                UIFactory.Bar(card.transform, "ExploreBar", PixelTheme.Blue, 8 * S).SetValue(p.Exploration, false);
            }
        }

        private static (string text, string icon, Color color) StateStyle(ChapterState state)
        {
            switch (state)
            {
                case ChapterState.Locked: return ("Locked", "icon_lock", PixelTheme.Muted);
                case ChapterState.Available: return ("Available", "icon_objective", PixelTheme.Hex("3E5A6E"));
                case ChapterState.InProgress: return ("In progress", "icon_objective", PixelTheme.Hex("8A5A2B"));
                case ChapterState.Completed: return ("Completed", "icon_check", PixelTheme.Hex("3F6B45"));
                default: return ("Fully explored", "icon_star", PixelTheme.Hex("8A6A12"));
            }
        }
    }

    /// <summary>Accurate numbers drawn straight from the save: chapters, objectives, memories, keepsakes, achievements, playtime.</summary>
    public class StatisticsPage : HubPage
    {
        private const int S = UIFactory.S;
        private RectTransform host;

        public override HubTab Tab => HubTab.Statistics;
        public override Selectable DefaultSelection => null;

        protected override void Build(RectTransform root)
        {
            UIFactory.VBox(root.gameObject, spacing: 3 * S);
            host = UIFactory.ScrollView(root, "Rows", out _);
            UIFactory.VBox(host.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(host.gameObject);
        }

        public override void Refresh()
        {
            if (host == null) return;
            UIFactory.ClearChildren(host);

            Row("Chapters completed", $"{JourneyStats.ChaptersCompleted} of {JourneyStats.ChaptersTotal}", (float)JourneyStats.ChaptersCompleted / JourneyStats.ChaptersTotal, PixelTheme.Orange);
            Row("Main objectives", $"{JourneyStats.MainDone} of {JourneyStats.MainTotal}", JourneyStats.StoryProgress, PixelTheme.Orange);
            Row("Optional objectives", $"{JourneyStats.OptionalDone} of {JourneyStats.OptionalTotal}", JourneyStats.ExplorationProgress, PixelTheme.Blue);
            Row("Memories discovered", $"{JourneyStats.MemoriesDone} of {JourneyStats.MemoriesTotal}", Ratio(JourneyStats.MemoriesDone, JourneyStats.MemoriesTotal), PixelTheme.Gold);
            Row("Keepsakes collected", $"{JourneyStats.KeepsakesDone} of {JourneyStats.KeepsakesTotal}", Ratio(JourneyStats.KeepsakesDone, JourneyStats.KeepsakesTotal), PixelTheme.Gold);
            Row("Achievements earned", $"{JourneyStats.AchievementsDone} of {JourneyStats.AchievementsTotal}", Ratio(JourneyStats.AchievementsDone, JourneyStats.AchievementsTotal), PixelTheme.Teal);
            Row("Story progress", JourneyStats.Percent(JourneyStats.StoryProgress), JourneyStats.StoryProgress, PixelTheme.Orange);
            Row("Exploration progress", JourneyStats.Percent(JourneyStats.ExplorationProgress), JourneyStats.ExplorationProgress, PixelTheme.Blue);
            Row("Time played", JourneyStats.FormatPlayTime(GameManager.Instance.PlayTimeSeconds), -1f, PixelTheme.Cream);

            var choice = JourneyCatalog.GetChoice(GameManager.Instance.FinalChoiceId);
            Row("Final decision", choice != null ? choice.Title : "Not yet made", -1f, PixelTheme.Cream);
        }

        private static float Ratio(int a, int b) => b == 0 ? 0f : (float)a / b;

        private void Row(string label, string value, float fill, Color barColor)
        {
            var row = UIFactory.Rect(host, label);
            UIFactory.Size(row.gameObject, minHeight: 15 * S);
            UIFactory.HBox(row.gameObject, spacing: 4 * S, align: TextAnchor.MiddleLeft);

            var name = UIFactory.Text(row, "Label", label, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(name.gameObject, 110 * S);

            var val = UIFactory.Text(row, "Value", value, TextStyle.Body, PixelTheme.Gold, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(val.gameObject, 70 * S);

            if (fill >= 0f) UIFactory.Bar(row, "Bar", barColor, 8 * S).SetValue(fill, false);
            else UIFactory.Spacer(row, flexible: 0f);
        }
    }
}
