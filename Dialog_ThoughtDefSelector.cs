using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    public class Dialog_ThoughtDefSelector : Window
    {
        private Vector2 scrollPos;
        private List<ThoughtDef> allDefs;
        private List<string> selection;
        private string currentCategory = "All";
        private readonly List<string> categories = new List<string> { "All", "PawnSanitizer_Tag_Mood".Translate(), "PawnSanitizer_Tag_Situational".Translate(), "PawnSanitizer_Tag_Social".Translate(), "PawnSanitizer_Tag_Needs".Translate(), "PawnSanitizer_Tag_Memory".Translate() };
        private Dictionary<string, List<ThoughtDef>> filteredDefsCache = new Dictionary<string, List<ThoughtDef>>();
        private static readonly Dictionary<string, Color> categoryColors = new Dictionary<string, Color>
        {
            { "PawnSanitizer_Tag_Mood".Translate(), Color.cyan },
            { "PawnSanitizer_Tag_Social".Translate(), Color.magenta },
            { "PawnSanitizer_Tag_Needs".Translate(), Color.yellow },
            { "PawnSanitizer_Tag_Situational".Translate(), Color.grey },
            { "PawnSanitizer_Tag_Memory".Translate(), new Color(1f, 0.5f, 0.1f) }
        };

        public override Vector2 InitialSize => new Vector2(700f, 720f);

        public Dialog_ThoughtDefSelector(List<string> selection)
        {
            this.selection = selection ?? new List<string>();
            this.forcePause = false;
            this.closeOnClickedOutside = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = false;

            allDefs = DefDatabase<ThoughtDef>.AllDefs
                .Where(d => !string.IsNullOrWhiteSpace(d.label))
                .OrderBy(d => d.label)
                .ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            float top = 0f;
            Rect categoryLabelRect = new Rect(inRect.x, top, 80f, 30f);
            Widgets.Label(categoryLabelRect, "PawnSanitizer_CategoryLabel".Translate());
            Rect categoryDropdownRect = new Rect(categoryLabelRect.xMax + 10f, top, 200f, 30f);
            if (Widgets.ButtonText(categoryDropdownRect, $"{currentCategory.Translate()} ({FilterDefsByCategory().Count})"))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var category in categories)
                {
                    options.Add(new FloatMenuOption(category.Translate(), () =>
                    {
                        currentCategory = category;
                        filteredDefsCache.Clear();  // Clear cache on category change
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            Rect selectAllRect = new Rect(categoryDropdownRect.xMax + 20f, top, 120f, 30f);
            if (Widgets.ButtonText(selectAllRect, "PawnSanitizer_SelectAll".Translate()))
            {
                var filteredDefs = FilterDefsByCategory();
                foreach (var def in filteredDefs)
                {
                    if (!selection.Contains(def.defName))
                        selection.Add(def.defName);
                }
            }

            Rect deselectAllRect = new Rect(selectAllRect.xMax + 10f, top, 120f, 30f);
            if (Widgets.ButtonText(deselectAllRect, "PawnSanitizer_DeselectAll".Translate()))
            {
                var filteredDefs = FilterDefsByCategory();
                foreach (var def in filteredDefs)
                {
                    selection.Remove(def.defName);
                }
            }

            Rect masterSelectRect = new Rect(deselectAllRect.xMax + 10f, top, 140f, 30f);
            if (Widgets.ButtonText(masterSelectRect, "PawnSanitizer_SelectAllGlobal".Translate()))
            {
                foreach (var def in allDefs)
                {
                    if (!selection.Contains(def.defName))
                        selection.Add(def.defName);
                }
            }

            top += 40f;
            Rect outRect = new Rect(inRect.x, top, inRect.width, inRect.height - top - 40);
            var filtered = FilterDefsByCategory() ?? new List<ThoughtDef>();
            var grouped = filtered.GroupBy(d => d.GetSanitizerCategoryTag().Translate()).ToList();
            Rect viewRect = new Rect(0, 0, outRect.width - 16, grouped.Sum(g => g.Count()) * 38f + 40f);

            if (outRect.height <= 0f || outRect.width <= 0f || viewRect.height <= 0f || viewRect.width <= 0f)
            {
                Log.Warning("[Sanitizer] Invalid scroll view dimensions.");
                return;
            }
            try
            {
                Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
                float y = 0f;

                foreach (var group in grouped)
                {
                    Widgets.Label(new Rect(0, y, viewRect.width, 28f), group.Key);
                    y += 28f;

                    foreach (var def in group)
                    {
                        Rect row = new Rect(0, y, viewRect.width, 36f);
                        bool selected = selection.Contains(def.defName);

                        Rect checkboxRect = new Rect(row.x, row.y, row.width - 300f, 30f);
                        Rect tooltipRect = new Rect(row.xMax - 200f, row.y, 30f, 30f);
                        Rect tagRect = new Rect(row.xMax - 160f, row.y, 150f, 30f);

                        Widgets.CheckboxLabeled(checkboxRect, def.label.CapitalizeFirst(), ref selected);

                        if (selected && !selection.Contains(def.defName))
                            selection.Add(def.defName);
                        else if (!selected && selection.Contains(def.defName))
                            selection.Remove(def.defName);

                        if (!string.IsNullOrEmpty(def.description))
                        {
                            TooltipHandler.TipRegion(tooltipRect, def.description);
                            Widgets.Label(tooltipRect, "(i)");
                        }

                        string tag = group.Key;
                        Color originalColor = GUI.color;
                        SetCategoryColor(tag);

                        Widgets.Label(tagRect, tag);
                        GUI.color = originalColor;

                        y += 36f;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[Sanitizer] Exception inside scroll view: {ex}");
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        private void SetCategoryColor(string tag)
        {
            if (categoryColors.ContainsKey(tag))
            {
                GUI.color = categoryColors[tag];
            }
            else
            {
                GUI.color = Color.white;
            }
        }

        private List<ThoughtDef> FilterDefsByCategory()
        {
            if (filteredDefsCache.ContainsKey(currentCategory))
            {
                return filteredDefsCache[currentCategory];
            }

            List<ThoughtDef> filtered = allDefs.Where(def =>
                (currentCategory == "All") ||
                (currentCategory == "PawnSanitizer_Tag_Mood".Translate() && def.IsMoodThought()) ||
                (currentCategory == "PawnSanitizer_Tag_Social".Translate() && def.IsSocialThought()) ||
                (currentCategory == "PawnSanitizer_Tag_Needs".Translate() && def.defName.ToLowerInvariant().Contains("need")) ||
                (currentCategory == "PawnSanitizer_Tag_Memory".Translate() && def.IsMemoryThought()) ||
                (currentCategory == "PawnSanitizer_Tag_Situational".Translate() && !def.IsMoodThought() && !def.IsSocialThought() && !def.defName.ToLowerInvariant().Contains("need") && !def.IsMemoryThought())
            ).ToList();

            filteredDefsCache[currentCategory] = filtered;
            return filtered;
        }
    }

    public static class ThoughtDefExtensions
    {
        public static bool IsMoodThought(this ThoughtDef def)
        {
            return def.stackedEffectMultiplier > 0 || (def.stages?.Any(s => s.baseMoodEffect != 0) == true);
        }

        public static bool IsSocialThought(this ThoughtDef def)
        {
            return def.thoughtClass != null &&
                   (def.thoughtClass.Name?.ToLowerInvariant().Contains("social") == true ||
                    def.thoughtClass.FullName?.ToLowerInvariant().Contains("social") == true ||
                    def.workerClass?.Name?.ToLowerInvariant().Contains("social") == true ||
                    def.defName.ToLowerInvariant().Contains("social") ||
                    def.defName.ToLowerInvariant().Contains("opinion") ||
                    def.defName.ToLowerInvariant().Contains("friend") ||
                    def.defName.ToLowerInvariant().Contains("rival"));
        }

        public static bool IsMemoryThought(this ThoughtDef def)
        {
            return def.thoughtClass?.Name?.ToLowerInvariant().Contains("memory") == true ||
                   def.workerClass?.Name?.ToLowerInvariant().Contains("memory") == true ||
                   def.defName.ToLowerInvariant().Contains("memory");
        }

        public static string GetSanitizerCategoryTag(this ThoughtDef def)
        {
            var ext = def.GetModExtension<ThoughtDefSanitizerCategoryExtension>();
            if (ext != null && !string.IsNullOrWhiteSpace(ext.sanitizerCategory))
                return ext.sanitizerCategory;

            if (def.IsMoodThought()) return "PawnSanitizer_Tag_Mood".Translate();
            if (def.IsSocialThought()) return "PawnSanitizer_Tag_Social".Translate();
            if (def.IsMemoryThought()) return "PawnSanitizer_Tag_Memory".Translate();
            if (def.defName.ToLowerInvariant().Contains("need")) return "PawnSanitizer_Tag_Needs".Translate();
            return "PawnSanitizer_Tag_Situational".Translate();
        }
    }
}