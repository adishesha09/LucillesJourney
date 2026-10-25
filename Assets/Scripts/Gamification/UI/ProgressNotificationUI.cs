using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// Brief, tasteful progress notifications (objective complete, memory discovered, keepsake found,
    /// achievement unlocked). Toasts are shown one at a time from a queue, never duplicate an entry that
    /// is already queued, and wait while a full-screen interface is open so they are never missed.
    /// </summary>
    public class ProgressNotificationUI
    {
        private struct Toast
        {
            public string Key, Heading, Title, Icon;
            public Color Accent;
            public float Hold;
            public int IconScale;
            public int Sound; // 0 none, 1 chime, 2 discovery (also drives the icon pop animation)
            public string SoundName;
        }

        private readonly JourneyUI ui;
        private readonly RectTransform container;
        private readonly Queue<Toast> queue = new Queue<Toast>();
        private readonly HashSet<string> queuedKeys = new HashSet<string>();
        private bool running;

        public ProgressNotificationUI(JourneyUI owner, Transform parent)
        {
            ui = owner;
            container = UIFactory.Rect(parent, "Toasts");
            UIFactory.Stretch(container);
        }

        public void Objective(ObjectiveInfo o) =>
            Enqueue(new Toast { Key = "obj:" + o.Id, Heading = o.Kind == ObjectiveKind.Step ? "Step complete" : "Objective complete",
                Title = o.Title, Icon = "icon_check", Accent = PixelTheme.Green, Hold = 2.0f, IconScale = 3, Sound = 1, SoundName = "ui_objective" });

        public void Memory(MemoryInfo m) =>
            Enqueue(new Toast { Key = "mem:" + m.Id, Heading = "Memory discovered", Title = m.Title, Icon = "icon_memory",
                Accent = PixelTheme.Gold, Hold = 2.8f, IconScale = 4, Sound = 2, SoundName = "ui_memory" });

        public void Keepsake(KeepsakeInfo k) =>
            Enqueue(new Toast { Key = "ks:" + k.Id, Heading = "Keepsake found", Title = k.Title, Icon = k.IconKey,
                Accent = PixelTheme.Orange, Hold = 2.8f, IconScale = 3, Sound = 2, SoundName = "ui_keepsake" });

        public void Achievement(AchievementInfo a) =>
            Enqueue(new Toast { Key = "ach:" + a.Id, Heading = "Achievement unlocked", Title = a.Title, Icon = a.IconKey,
                Accent = PixelTheme.Gold, Hold = 3.4f, IconScale = 3, Sound = 1, SoundName = "ui_achievement" });

        private void Enqueue(Toast toast)
        {
            if (!queuedKeys.Add(toast.Key)) return; // already waiting to be shown
            queue.Enqueue(toast);
            if (!running) ui.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            running = true;
            while (queue.Count > 0)
            {
                while (ui.IsAnyScreenOpen) yield return null;

                var toast = queue.Dequeue();
                queuedKeys.Remove(toast.Key);

                // When several are waiting, shorten the hold so a burst of discoveries clears quickly.
                float hold = queue.Count > 1 ? Mathf.Min(toast.Hold, 1.4f) : toast.Hold;
                yield return ShowToast(toast, hold);
            }
            running = false;
        }

        private IEnumerator ShowToast(Toast toast, float hold)
        {
            const int S = UIFactory.S;
            bool reduced = SettingsManager.ReducedMotion;

            var panel = UIFactory.Panel(container, "Toast", "ui_toast", raycast: false);
            var rt = panel.rectTransform;
            UIFactory.Anchor(rt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -120f), new Vector2(760f, 0f));
            UIFactory.VBox(panel.gameObject, padding: 6 * S, align: TextAnchor.MiddleLeft);
            UIFactory.FitHeight(panel.gameObject);
            var group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            var row = UIFactory.Rect(panel.transform, "Row");
            UIFactory.HBox(row.gameObject, spacing: 5 * S, align: TextAnchor.MiddleLeft);

            var iconHolder = UIFactory.Rect(row, "IconHolder");
            UIFactory.Size(iconHolder.gameObject, 36 * S, 36 * S);
            var icon = UIFactory.Icon(iconHolder, "Icon", toast.Icon, toast.IconScale);
            UIFactory.Anchor(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, icon.rectTransform.sizeDelta);
            Object.Destroy(icon.GetComponent<LayoutElement>());
            var iconSize = icon.sprite != null ? new Vector2(icon.sprite.rect.width, icon.sprite.rect.height) * toast.IconScale : Vector2.one * 16 * toast.IconScale;
            icon.rectTransform.sizeDelta = iconSize;

            var texts = UIFactory.Rect(row, "Texts");
            UIFactory.VBox(texts.gameObject, spacing: 0, align: TextAnchor.MiddleLeft);
            UIFactory.Size(texts.gameObject, flexWidth: 1f);
            UIFactory.Text(texts, "Heading", toast.Heading, TextStyle.BodySmall, toast.Accent);
            UIFactory.Text(texts, "Title", toast.Title, TextStyle.Body, PixelTheme.Cream);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            UISound.Play(toast.SoundName);

            // Enter
            Vector2 end = rt.anchoredPosition;
            Vector2 start = end + (reduced ? Vector2.zero : new Vector2(80f, 0f));
            float enter = reduced ? 0.1f : 0.22f;
            for (float t = 0f; t < enter; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / enter);
                group.alpha = k;
                rt.anchoredPosition = Vector2.Lerp(start, end, k);
                if (!reduced && toast.Sound == 2) icon.rectTransform.localScale = Vector3.one * (1f + 0.4f * (1f - k));
                yield return null;
            }
            group.alpha = 1f;
            rt.anchoredPosition = end;
            icon.rectTransform.localScale = Vector3.one;

            for (float t = 0f; t < hold; t += Time.unscaledDeltaTime) yield return null;

            float exit = reduced ? 0.1f : 0.25f;
            for (float t = 0f; t < exit; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1f - t / exit;
                yield return null;
            }
            Object.Destroy(panel.gameObject);
        }
    }
}
