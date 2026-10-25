using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;
using LucillesJourney.Narrative;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// The unobtrusive gameplay HUD: current chapter, the one main objective that matters now (with a
    /// next-step hint), a quiet optional-discoveries note, and shortcut buttons. It fades away while
    /// narrative text, the final choice or any full-screen interface is showing.
    /// </summary>
    public class JourneyHUD
    {
        private const int S = UIFactory.S;

        private JourneyUI ui;
        private CanvasGroup group;
        private TMP_Text chapterLabel, objectiveLabel, hintLabel, optionalLabel;
        private Image objectiveIcon;
        private GameObject hintRow, optionalRow;

        public void Create(JourneyUI owner, Transform parent)
        {
            ui = owner;
            var root = UIFactory.Rect(parent, "HUD");
            UIFactory.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            BuildObjectivePanel(root);
            BuildShortcuts(root);
        }

        private void BuildObjectivePanel(RectTransform root)
        {
            var panel = UIFactory.Panel(root, "ObjectivePanel", "ui_panel");
            UIFactory.Anchor(panel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -32f), new Vector2(820f, 0f));
            var clamp = panel.gameObject.AddComponent<ClampSize>(); // leaves room for the shortcut buttons on narrow screens
            clamp.Design = new Vector2(820f, 0f);
            clamp.Margin = 250f;
            clamp.ClampHeight = false;
            UIFactory.VBox(panel.gameObject, spacing: 3 * S, padding: 6 * S);
            UIFactory.FitHeight(panel.gameObject);

            chapterLabel = UIFactory.Text(panel.transform, "Chapter", "", TextStyle.Label, PixelTheme.Gold);

            var divider = UIFactory.Solid(panel.transform, "Divider", PixelTheme.Tan);
            UIFactory.Size(divider.gameObject, height: S);

            var row = UIFactory.Rect(panel.transform, "ObjectiveRow");
            UIFactory.HBox(row.gameObject, spacing: 3 * S, align: TextAnchor.UpperLeft, expandHeight: false);
            objectiveIcon = UIFactory.Icon(row, "Icon", "icon_objective", S);
            var texts = UIFactory.Rect(row, "Texts");
            UIFactory.VBox(texts.gameObject);
            UIFactory.Size(texts.gameObject, flexWidth: 1f);
            objectiveLabel = UIFactory.Text(texts, "Objective", "", TextStyle.Body, PixelTheme.Cream);
            hintLabel = UIFactory.Text(texts, "Hint", "", TextStyle.Meta, PixelTheme.Sand);
            hintRow = hintLabel.gameObject;

            var optional = UIFactory.Rect(panel.transform, "OptionalRow");
            UIFactory.HBox(optional.gameObject, spacing: 2 * S, align: TextAnchor.MiddleLeft, expandHeight: false);
            UIFactory.Icon(optional, "Icon", "icon_optional", 2);
            optionalLabel = UIFactory.Text(optional, "Optional", "", TextStyle.Meta, PixelTheme.Muted, TextAlignmentOptions.MidlineLeft);
            optionalRow = optional.gameObject;
        }

        private void BuildShortcuts(RectTransform root)
        {
            var row = UIFactory.Rect(root, "Shortcuts");
            UIFactory.Anchor(row, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -32f), new Vector2(0f, 20 * S));
            UIFactory.HBox(row.gameObject, spacing: 3 * S, align: TextAnchor.MiddleRight, expandHeight: true);
            UIFactory.FitHeight(row.gameObject);
            var fitter = row.gameObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            UIFactory.MakeButton(row, "JournalButton", JourneyInput.Journal, () => ui.OpenHub(HubTab.Quests), ButtonKind.Secondary, "icon_scroll", 44 * S, 18 * S);
            UIFactory.MakeButton(row, "AlbumButton", JourneyInput.Album, () => ui.OpenHub(HubTab.Memories), ButtonKind.Secondary, "icon_album", 44 * S, 18 * S);
            UIFactory.MakeButton(row, "PauseButton", JourneyInput.Pause, () => ui.OpenPause(), ButtonKind.Secondary, "icon_pause", 64 * S, 18 * S);
        }

        public void Refresh()
        {
            var game = GameManager.Instance;
            var chapter = JourneyCatalog.GetChapter(game.CurrentChapterId);
            if (chapter == null) return;

            chapterLabel.text = chapter.Title;
            var next = JourneyStats.NextMainObjective(chapter.Id);
            if (next == null)
            {
                objectiveLabel.text = "This chapter's tasks are complete.";
                hintLabel.text = "";
                hintRow.SetActive(false);
                objectiveIcon.sprite = PixelTheme.Sprite("icon_check");
            }
            else
            {
                var (done, total) = JourneyStats.StepProgress(next);
                objectiveLabel.text = total > 0 ? $"{next.Title} ({done}/{total})" : next.Title;
                hintLabel.text = next.Hint;
                hintRow.SetActive(!string.IsNullOrEmpty(next.Hint));
                objectiveIcon.sprite = PixelTheme.Sprite("icon_objective");
            }

            int remaining = JourneyStats.GetChapter(chapter.Id).OptionalRemaining;
            optionalRow.SetActive(remaining > 0);
            optionalLabel.text = remaining == 1 ? "1 optional discovery here" : $"{remaining} optional discoveries here";
        }

        public void Tick()
        {
            bool inChapter = GameManager.Instance.CurrentChapterId != ChapterId.None;
            bool narrative = NarrativePanelUI.Instance != null && NarrativePanelUI.Instance.IsShowing;
            bool visible = inChapter && !ui.IsAnyScreenOpen && !narrative && !ui.IsLegacyChoiceVisible;

            float target = visible ? 1f : 0f;
            float speed = LucillesJourney.Core.SettingsManager.ReducedMotion ? 100f : 6f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * speed);
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }
    }
}
