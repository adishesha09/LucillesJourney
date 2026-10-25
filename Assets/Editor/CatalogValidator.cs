using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LucillesJourney.Core;
using LucillesJourney.Gamification;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Static integrity checks for the journey data: unique ids, valid references, required art and
    /// sequences present. Run from the menu or in batch mode with
    /// -executeMethod LucillesJourney.EditorTools.CatalogValidator.RunBatch
    /// </summary>
    public static class CatalogValidator
    {
        [MenuItem("Lucille's Journey/Validate Journey Catalog")]
        public static void RunFromMenu() => Debug.Log(Validate().Count == 0 ? "[Catalog] OK" : "[Catalog] issues found; see warnings.");

        public static void RunBatch()
        {
            var problems = Validate();
            foreach (var p in problems) Debug.LogError("[Catalog] " + p);
            Debug.Log(problems.Count == 0 ? "[Catalog] PASSED" : $"[Catalog] FAILED with {problems.Count} problem(s)");
            EditorApplication.Exit(problems.Count == 0 ? 0 : 1);
        }

        public static List<string> Validate()
        {
            var problems = new List<string>();

            void Duplicates(string kind, IEnumerable<string> ids)
            {
                foreach (var g in ids.GroupBy(x => x).Where(g => g.Count() > 1))
                    problems.Add($"duplicate {kind} id '{g.Key}'");
            }

            Duplicates("objective", JourneyCatalog.Objectives.Select(o => o.Id));
            Duplicates("memory", JourneyCatalog.Memories.Select(m => m.Id));
            Duplicates("keepsake", JourneyCatalog.Keepsakes.Select(k => k.Id));
            Duplicates("achievement", AchievementService.All.Select(a => a.Id));
            Duplicates("sequence", JourneyCatalog.Objectives.Where(o => !string.IsNullOrEmpty(o.SequenceName)).Select(o => o.SequenceName));

            foreach (var m in JourneyCatalog.Memories)
            {
                if (!JourneyCatalog.TryGetObjective(m.Id, out var o)) problems.Add($"memory '{m.Id}' has no objective");
                else if (!o.Optional || o.Kind != ObjectiveKind.Memory || o.Chapter != m.Chapter) problems.Add($"memory '{m.Id}' objective mismatch");
                if (m.Lines.Length == 0) problems.Add($"memory '{m.Id}' has no text");
                RequireSprite(problems, m.ArtKey);
            }
            RequireSprite(problems, "mem_unknown");

            foreach (var k in JourneyCatalog.Keepsakes)
            {
                if (!JourneyCatalog.TryGetObjective(k.Id, out var o)) problems.Add($"keepsake '{k.Id}' has no objective");
                else if (!o.Optional || o.Kind != ObjectiveKind.Keepsake || o.Chapter != k.Chapter) problems.Add($"keepsake '{k.Id}' objective mismatch");
                RequireSprite(problems, k.IconKey);
            }

            foreach (var o in JourneyCatalog.Objectives)
            {
                foreach (var step in o.StepIds)
                    if (!JourneyCatalog.TryGetObjective(step, out var s) || s.ParentId != o.Id) problems.Add($"objective '{o.Id}' has invalid step '{step}'");
                foreach (var req in o.RequiresIds)
                    if (!JourneyCatalog.TryGetObjective(req, out _)) problems.Add($"objective '{o.Id}' requires unknown '{req}'");
                if (o.Kind == ObjectiveKind.Step && !JourneyCatalog.TryGetObjective(o.ParentId, out _)) problems.Add($"step '{o.Id}' has no parent");
                if (o.Kind == ObjectiveKind.Memory && !JourneyCatalog.TryGetMemory(o.Id, out _)) problems.Add($"memory objective '{o.Id}' has no memory");
                if (o.Kind == ObjectiveKind.Keepsake && !JourneyCatalog.TryGetKeepsake(o.Id, out _)) problems.Add($"keepsake objective '{o.Id}' has no keepsake");
                bool needsSequence = !o.HasSteps && o.Kind != ObjectiveKind.Decision;
                if (needsSequence && string.IsNullOrEmpty(o.SequenceName)) problems.Add($"objective '{o.Id}' has no narrative sequence name");
                if (needsSequence && !string.IsNullOrEmpty(o.SequenceName)
                    && AssetDatabase.LoadAssetAtPath<LucillesJourney.Narrative.NarrativeSequenceSO>($"Assets/Data/Narrative/{o.SequenceName}.asset") == null)
                    problems.Add($"objective '{o.Id}': narrative asset '{o.SequenceName}' not found (run Build All)");
            }

            foreach (var chapter in JourneyCatalog.Chapters)
            {
                if (!JourneyCatalog.RequiredFor(chapter.Id).Any()) problems.Add($"chapter {chapter.Id} has no required objectives");
                RequireSprite(problems, chapter.ThumbnailKey);

                var data = AssetDatabase.LoadAssetAtPath<ChapterDataSO>($"Assets/Data/Chapters/Chapter_{chapter.Id}.asset");
                if (data == null) { problems.Add($"missing ChapterDataSO for {chapter.Id}"); continue; }
                if (data.displayName != chapter.Title) problems.Add($"{chapter.Id}: ChapterDataSO title '{data.displayName}' != catalog '{chapter.Title}'");
                if (data.sceneName != chapter.SceneName) problems.Add($"{chapter.Id}: scene name mismatch");
                var assetIds = data.objectives.Select(o => o.objectiveId).OrderBy(x => x).ToList();
                var catalogIds = JourneyCatalog.ObjectivesFor(chapter.Id).Select(o => o.Id).OrderBy(x => x).ToList();
                if (!assetIds.SequenceEqual(catalogIds)) problems.Add($"{chapter.Id}: ChapterDataSO objectives differ from the catalog (run Build All)");
            }

            foreach (var a in AchievementService.All)
            {
                RequireSprite(problems, a.IconKey);
                if (a.Measure == null) problems.Add($"achievement '{a.Id}' has no measure");
            }

            foreach (var c in JourneyCatalog.Choices) RequireSprite(problems, c.ArtKey);
            foreach (var name in new[]
            {
                "ui_panel", "ui_panel_parchment", "ui_button", "ui_button_hover", "ui_button_pressed", "ui_button_disabled", "ui_button_selected",
                "ui_tab", "ui_tab_active", "ui_slot", "ui_slot_open", "ui_toast", "ui_bar_frame", "ui_bar_fill", "ui_slider_bg", "ui_knob",
                "ui_toggle_box", "ui_toggle_check", "ui_path_dot", "ui_badge_new",
                "icon_scroll", "icon_journal", "icon_album", "icon_keepsake", "icon_trophy", "icon_journey", "icon_stats", "icon_settings", "icon_lock",
                "icon_check", "icon_box", "icon_star", "icon_pause", "icon_play", "icon_close", "icon_arrow_left", "icon_arrow_right", "icon_help",
                "icon_door", "icon_memory", "icon_objective", "icon_optional", "icon_sound", "icon_text", "icon_motion",
            }) RequireSprite(problems, name);

            foreach (var font in new[] { "PressStart2P-Regular", "VT323-Regular" })
                if (AssetDatabase.LoadAssetAtPath<Font>($"Assets/Resources/Fonts/{font}.ttf") == null) problems.Add($"missing font {font}");

            foreach (var p in problems) Debug.LogWarning("[Catalog] " + p);
            return problems;
        }

        private static void RequireSprite(List<string> problems, string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            string path = $"Assets/Resources/JourneyUI/{name}.png";
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null) problems.Add($"missing sprite {path}");
        }
    }
}
