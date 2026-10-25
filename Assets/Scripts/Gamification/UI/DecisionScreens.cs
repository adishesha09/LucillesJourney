using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Interaction;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>Shown when a chapter's required objectives are done: what was found, what remains optional, and Continue.</summary>
    public class ChapterSummaryScreen : JourneyScreen
    {
        private const int S = UIFactory.S;

        private Image thumb;
        private TMP_Text chapterTitle, subtitle, remaining, noAchievements;
        private RectTransform rows, achievementList;
        private Button continueButton;
        private TMP_Text continueLabel;
        private ChapterId chapter;
        private List<AchievementInfo> newAchievements = new List<AchievementInfo>();
        private Action onContinue;

        public override bool CanDismiss => false;
        protected override Selectable DefaultSelection => continueButton;

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "Chapter complete", "icon_check", new Vector2(1500f, 900f), out _);
            UIFactory.HBox(body.gameObject, spacing: 8 * S, align: TextAnchor.UpperLeft, expandWidth: false, expandHeight: true);

            var left = UIFactory.ScrollView(body, "Left", out _);
            UIFactory.Size(left.parent.parent.gameObject, 128 * S, flexHeight: 1f);
            UIFactory.VBox(left.gameObject, spacing: 3 * S, align: TextAnchor.UpperCenter);
            UIFactory.FitHeight(left.gameObject);
            var thumbFrame = UIFactory.Panel(left, "ThumbFrame", "ui_slot_open");
            UIFactory.VBox(thumbFrame.gameObject, padding: 3 * S, align: TextAnchor.MiddleCenter, expandWidth: false);
            thumb = UIFactory.Icon(thumbFrame.transform, "Thumb", null, 3);
            chapterTitle = UIFactory.Text(left, "Title", "", TextStyle.Label, PixelTheme.Gold, TextAlignmentOptions.Top);
            subtitle = UIFactory.Text(left, "Subtitle", "", TextStyle.Body, PixelTheme.Sand, TextAlignmentOptions.Top);

            var right = UIFactory.Rect(body, "Right");
            UIFactory.Size(right.gameObject, flexWidth: 1f, flexHeight: 1f);
            UIFactory.VBox(right.gameObject, spacing: 3 * S);

            var scrollContent = UIFactory.ScrollView(right, "Scroll", out _);
            UIFactory.VBox(scrollContent.gameObject, spacing: 3 * S, padding: 2 * S);
            UIFactory.FitHeight(scrollContent.gameObject);

            rows = UIFactory.Rect(scrollContent, "Rows");
            UIFactory.VBox(rows.gameObject, spacing: 2 * S);

            var achHeading = UIFactory.Text(scrollContent, "AchievementsHeading", "Achievements this chapter", TextStyle.Label, PixelTheme.Gold);
            UIFactory.Size(achHeading.gameObject, flexWidth: 1f);
            achievementList = UIFactory.Rect(scrollContent, "Achievements");
            UIFactory.VBox(achievementList.gameObject, spacing: 2 * S);
            noAchievements = UIFactory.Text(scrollContent, "None", "None earned in this chapter.", TextStyle.BodySmall, PixelTheme.Muted);

            remaining = UIFactory.Text(scrollContent, "Remaining", "", TextStyle.Body, PixelTheme.Sand);

            continueButton = UIFactory.MakeButton(right, "Continue", "Continue", Continue, ButtonKind.Primary, "icon_arrow_right", height: 24 * S);
            continueLabel = continueButton.GetComponentInChildren<TMP_Text>();
        }

        public void Setup(ChapterId chapterId, List<AchievementInfo> achievements, Action continueAction)
        {
            chapter = chapterId;
            newAchievements = achievements ?? new List<AchievementInfo>();
            onContinue = continueAction;
        }

        public override void OnOpen()
        {
            if (continueButton != null) continueButton.interactable = true;
        }

        public override void Refresh()
        {
            var info = JourneyCatalog.GetChapter(chapter);
            var p = JourneyStats.GetChapter(chapter);
            if (info == null) return;

            var sprite = PixelTheme.Sprite(info.ThumbnailKey);
            thumb.sprite = sprite;
            if (sprite != null)
            {
                var le = thumb.GetComponent<LayoutElement>();
                le.preferredWidth = sprite.rect.width * 3;
                le.preferredHeight = sprite.rect.height * 3;
            }
            chapterTitle.text = info.Title;
            subtitle.text = info.Description;
            continueLabel.text = chapter == ChapterId.SecondChance ? "Continue to the ending" : "Continue";

            UIFactory.ClearChildren(rows);
            Row("Main objectives completed", $"{p.RequiredDone} of {p.RequiredTotal}");
            Row("Optional objectives completed", $"{p.OptionalDone} of {p.OptionalTotal}");
            Row("Memories discovered", $"{p.MemoriesDone} of {p.MemoriesTotal}");
            Row("Keepsakes collected", $"{p.KeepsakesDone} of {p.KeepsakesTotal}");

            UIFactory.ClearChildren(achievementList);
            noAchievements.gameObject.SetActive(newAchievements.Count == 0);
            foreach (var a in newAchievements)
            {
                var row = UIFactory.Rect(achievementList, a.Id);
                UIFactory.HBox(row.gameObject, spacing: 3 * S, align: TextAnchor.MiddleLeft);
                UIFactory.Icon(row, "Icon", a.IconKey, 2);
                UIFactory.Text(row, "Title", a.Title, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.MidlineLeft);
            }

            int left = p.OptionalRemaining;
            remaining.text = left > 0
                ? (left == 1 ? "1 optional discovery remains in this chapter. You do not need to find it to continue."
                             : $"{left} optional discoveries remain in this chapter. You do not need to find them to continue.")
                : "Everything optional in this chapter has been found.";
        }

        private void Row(string label, string value)
        {
            var row = UIFactory.Rect(rows, label);
            UIFactory.Size(row.gameObject, height: 12 * S);
            UIFactory.HBox(row.gameObject, spacing: 3 * S, align: TextAnchor.MiddleLeft);
            var l = UIFactory.Text(row, "Label", label, TextStyle.Body, PixelTheme.Cream, TextAlignmentOptions.MidlineLeft);
            UIFactory.Size(l.gameObject, flexWidth: 1f);
            UIFactory.Text(row, "Value", value, TextStyle.Body, PixelTheme.Gold, TextAlignmentOptions.MidlineRight);
        }

        private void Continue()
        {
            continueButton.interactable = false; // guards against a double click starting two scene loads
            var action = onContinue;
            onContinue = null;
            Ui.Pop();
            action?.Invoke();
        }
    }

    /// <summary>
    /// Presents Lucille's impossible choice: three options with illustrations, what each covers and what
    /// it leaves less for, pointer and keyboard selection, and a separate confirmation step. No option is
    /// presented as the correct one.
    /// </summary>
    public class FinalChoiceScreen : JourneyScreen
    {
        private const int S = UIFactory.S;

        private sealed class Card
        {
            public ChoiceOption Option;
            public ChoiceInfo Info;
            public Button Button;
            public Image Frame;
            public GameObject Marker;
        }

        private TMP_Text promptLabel, reviewTitle, reviewCovers, reviewLeaves;
        private RectTransform cardRow;
        private Button confirmButton;
        private readonly List<Card> cards = new List<Card>();
        private Card selected;
        private Action<ChoiceOption> onChosen;
        private IReadOnlyList<ChoiceOption> options = new List<ChoiceOption>();
        private string prompt = "";

        public override bool CanDismiss => false;
        protected override Selectable DefaultSelection => cards.Count > 0 ? cards[0].Button : (Selectable)confirmButton;

        protected override void Build(RectTransform root)
        {
            var body = BuildFrame(root, "A Second Chance", "icon_journey", new Vector2(1780f, 960f), out _);
            UIFactory.VBox(body.gameObject, spacing: 4 * S);

            // The prompt and options scroll if the interface is zoomed in; the review and Confirm stay in view below.
            var content = UIFactory.ScrollView(body, "Scroll", out _);
            UIFactory.VBox(content.gameObject, spacing: 4 * S, padding: 2 * S);
            UIFactory.FitHeight(content.gameObject);

            promptLabel = UIFactory.Text(content, "Prompt", "", TextStyle.Body, PixelTheme.Cream);
            var note = UIFactory.Text(content, "Note", "Every option leaves something uncovered this month. Take your time: nothing is final until you confirm.",
                TextStyle.BodySmall, PixelTheme.Sand);
            UIFactory.Size(note.gameObject, flexWidth: 1f);

            cardRow = UIFactory.Rect(content, "Cards");
            UIFactory.Size(cardRow.gameObject, flexWidth: 1f);
            UIFactory.HBox(cardRow.gameObject, spacing: 5 * S, align: TextAnchor.UpperCenter, expandWidth: true, expandHeight: true);

            var review = UIFactory.Panel(body, "Review", "ui_slot_open");
            UIFactory.Size(review.gameObject, minHeight: 42 * S, flexHeight: 0f);
            UIFactory.HBox(review.gameObject, spacing: 5 * S, padding: 4 * S, align: TextAnchor.MiddleLeft, expandWidth: false, expandHeight: true);

            var reviewText = UIFactory.Rect(review.transform, "Text");
            UIFactory.Size(reviewText.gameObject, flexWidth: 1f);
            UIFactory.VBox(reviewText.gameObject, spacing: S, align: TextAnchor.MiddleLeft);
            reviewTitle = UIFactory.Text(reviewText, "Title", "", TextStyle.Label, PixelTheme.DeepBrown);
            reviewCovers = UIFactory.Text(reviewText, "Covers", "", TextStyle.BodySmall, PixelTheme.DeepBrown);
            reviewLeaves = UIFactory.Text(reviewText, "Leaves", "", TextStyle.BodySmall, PixelTheme.MidBrown);

            confirmButton = UIFactory.MakeButton(review.transform, "Confirm", "Confirm choice", Confirm, ButtonKind.Primary, "icon_check", 78 * S, 24 * S);
        }

        public void Setup(string promptText, IReadOnlyList<ChoiceOption> choiceOptions, Action<ChoiceOption> chosen)
        {
            prompt = promptText;
            options = choiceOptions;
            onChosen = chosen;
            selected = null;
        }

        public override void OnOpen()
        {
            promptLabel.text = prompt;
            UIFactory.ClearChildren(cardRow);
            cards.Clear();
            selected = null;

            foreach (var option in options) cards.Add(BuildCard(option));
            confirmButton.interactable = false;
            ShowReview(null);
        }

        private Card BuildCard(ChoiceOption option)
        {
            var info = JourneyCatalog.GetChoice(option.id);
            var card = new Card { Option = option, Info = info };

            card.Button = UIFactory.MakeCardButton(cardRow, option.id, () => Select(card));
            card.Frame = (Image)card.Button.targetGraphic;
            UIFactory.Size(card.Button.gameObject, flexWidth: 1f, flexHeight: 1f);
            UIFactory.VBox(card.Button.gameObject, spacing: 3 * S, padding: 5 * S, align: TextAnchor.UpperCenter);
            card.Button.gameObject.AddComponent<SelectNotifier>().OnSelected = () => ShowReview(card);

            UIFactory.Icon(card.Button.transform, "Art", info != null ? info.ArtKey : null, 4);
            var title = UIFactory.Text(card.Button.transform, "Title", info != null ? info.Title : option.label, TextStyle.Label,
                PixelTheme.DeepBrown, TextAlignmentOptions.Top);
            UIFactory.Size(title.gameObject, flexWidth: 1f);
            var label = UIFactory.Text(card.Button.transform, "Label", option.label, TextStyle.Body, PixelTheme.MidBrown, TextAlignmentOptions.Top);
            UIFactory.Size(label.gameObject, flexWidth: 1f);

            UIFactory.Spacer(card.Button.transform, flexible: 1f);

            var marker = UIFactory.Rect(card.Button.transform, "SelectedMarker");
            UIFactory.HBox(marker.gameObject, spacing: 2 * S, align: TextAnchor.MiddleCenter, expandWidth: false, expandHeight: false);
            UIFactory.Icon(marker, "Check", "icon_check", 2);
            UIFactory.Text(marker, "Label", "Selected", TextStyle.Body, PixelTheme.Hex("3F6B45"), TextAlignmentOptions.Center);
            marker.gameObject.SetActive(false);
            card.Marker = marker.gameObject;
            return card;
        }

        private void Select(Card card)
        {
            UISound.Play("ui_select");
            selected = card;
            foreach (var c in cards)
            {
                bool on = c == card;
                c.Marker.SetActive(on);
                var colors = c.Button.colors;
                colors.normalColor = on ? new Color(1f, 0.9f, 0.55f, 1f) : Color.white;
                c.Button.colors = colors;
                c.Frame.color = on ? new Color(1f, 0.9f, 0.55f, 1f) : Color.white;
            }
            confirmButton.interactable = true;
            ShowReview(card);
        }

        private void ShowReview(Card card)
        {
            var shown = card ?? selected;
            if (shown == null)
            {
                reviewTitle.text = "Choose an option to review it";
                reviewCovers.text = "Highlight any option to read what it covers and what it leaves less for.";
                reviewLeaves.text = "";
                return;
            }
            string title = shown.Info != null ? shown.Info.Title : shown.Option.label;
            reviewTitle.text = title + (shown == selected ? "  (selected)" : "");
            reviewCovers.text = shown.Info != null ? "Covers: " + shown.Info.Covers : shown.Option.label;
            reviewLeaves.text = shown.Info != null ? "Leaves less for: " + shown.Info.LeavesLess : "";
        }

        private void Confirm()
        {
            if (selected == null) return;
            UISound.Play("ui_confirm");
            confirmButton.interactable = false;
            var chosen = selected.Option;
            var callback = onChosen;
            onChosen = null;
            Ui.Pop();
            callback?.Invoke(chosen);
        }
    }
}
