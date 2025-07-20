using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public static class Utility_BillConfigurator
    {
        // This static list holds the filtered apparel items for both dialogs
        public static List<ThingDef> filteredItems = new List<ThingDef>();

        public static bool IsItemAvailableAtBench(ThingDef item, ThingDef buildingDef)
        {
            return GetRecipesForItemAtWorkbench(item, buildingDef)
                .Any(recipe => AreResearchPrerequisitesMet(recipe));
        }

        public static List<RecipeDef> GetRecipesForItemAtWorkbench(ThingDef item, ThingDef buildingDef)
        {
            return DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.products?.Any(p => p.thingDef == item) == true)
                .Where(r => r.recipeUsers?.Contains(buildingDef) == true)
                .ToList();
        }

        public static bool AreResearchPrerequisitesMet(RecipeDef recipe)
        {
            if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished)
            {
                return false;
            }

            if (recipe.researchPrerequisites != null && recipe.researchPrerequisites.Any(r => !r.IsFinished))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<ResearchProjectDef> GetAllRelevantResearchesForItemSet(IEnumerable<ThingDef> items, ThingDef buildingDef)
        {
            var result = new HashSet<ResearchProjectDef>();

            foreach (var item in items)
            {
                foreach (var recipe in GetRecipesForItemAtWorkbench(item, buildingDef))
                {
                    if (recipe.researchPrerequisite != null)
                        result.Add(recipe.researchPrerequisite);

                    if (recipe.researchPrerequisites != null)
                        foreach (var prereq in recipe.researchPrerequisites)
                            result.Add(prereq);
                }
            }

            return result;
        }

        public static bool ItemRequiresResearch(ThingDef item, ResearchProjectDef research, ThingDef buildingDef)
        {
            return GetRecipesForItemAtWorkbench(item, buildingDef)
                .Any(r =>
                    r.researchPrerequisite == research ||
                    (r.researchPrerequisites?.Contains(research) ?? false));
        }

        public static string BuildRecipeTooltip(ThingDef item, ThingDef buildingDef)
        {
            var tip = new StringBuilder();
            tip.AppendLine(item.LabelCap.Colorize(ColoredText.TipSectionTitleColor));

            var recipes = GetRecipesForItemAtWorkbench(item, buildingDef);
            if (recipes.Any())
            {
                tip.AppendLine("EM_LoadoutConfigurator_AvailableRecipes".Translate());
                foreach (var recipe in recipes)
                {
                    bool unlocked = (recipe.researchPrerequisite?.IsFinished ?? true)
                                     && (recipe.researchPrerequisites?.All(r => r.IsFinished) ?? true);

                    tip.AppendLine(unlocked
                        ? $"  <color=#00FF00>✓ {recipe.LabelCap}</color>"
                        : $"  <color=#FF6347>✗ {recipe.LabelCap}</color>");

                    if (recipe.researchPrerequisite != null)
                    {
                        tip.AppendLine("EM_LoadoutConfigurator_ResearchRequired".Translate() + " " + recipe.researchPrerequisite.LabelCap);
                    }

                    if (recipe.researchPrerequisites != null)
                    {
                        foreach (var r in recipe.researchPrerequisites)
                        {
                            tip.AppendLine("EM_LoadoutConfigurator_ResearchRequired".Translate() + " " + r.LabelCap);
                        }
                    }
                }
            }

            if (!item.description.NullOrEmpty())
            {
                tip.AppendLine();
                tip.AppendLine(item.description);
            }

            return tip.ToString();
        }

        public static bool MatchesApparelSlot(ThingDef apparel, BodyPartGroupDef group, ApparelLayerDef layer)
        {
            return apparel?.apparel != null
                && apparel.apparel.bodyPartGroups.Contains(group)
                && apparel.apparel.layers.Contains(layer);
        }

        public static List<ThingDef> GetAlternativeApparelSuggestions(ThingDef baseItem, IEnumerable<ThingDef> pool)
        {
            if (!baseItem.IsApparel || baseItem.apparel == null || baseItem.apparel.bodyPartGroups.NullOrEmpty())
                return new List<ThingDef>();

            var baseGroups = baseItem.apparel.bodyPartGroups.Select(g => g.defName).ToHashSet();
            var baseLayers = baseItem.apparel.layers.Select(l => l.defName).ToHashSet();

            return pool
                .Where(t => t.IsApparel && t != baseItem && t.apparel != null)
                .Where(t =>
                {
                    var app = t.apparel;
                    bool sameGroups = app.bodyPartGroups.Any(g => baseGroups.Contains(g.defName));
                    bool differentLayer = app.layers.Any(l => !baseLayers.Contains(l.defName));
                    return sameGroups && differentLayer;
                })
                .Distinct()
                .ToList();
        }

        public static readonly Dictionary<StuffCategoryDef, ThingDef> DefaultStuffPerCategory =
            new Dictionary<StuffCategoryDef, ThingDef>
        {
        { StuffCategoryDefOf.Metallic, ThingDefOf.Steel },
        { StuffCategoryDefOf.Fabric, ThingDef.Named("Cloth") },
        { StuffCategoryDefOf.Leathery, ThingDef.Named("Leather_Plain") },
        { DefDatabase<StuffCategoryDef>.GetNamed("Woody", false), ThingDef.Named("WoodLog") },
        { DefDatabase<StuffCategoryDef>.GetNamed("Stony", false), ThingDef.Named("BlocksGranite") }
        };

        public static ThingDef GetDefaultStuff(ThingDef def)
        {
            if (!def.MadeFromStuff || def.stuffCategories == null)
                return null;

            foreach (StuffCategoryDef cat in def.stuffCategories)
            {
                if (cat == null) continue;
                if (DefaultStuffPerCategory.TryGetValue(cat, out ThingDef matched))
                    return matched;
            }

            // CE fallback (safe)
            return GenStuff.DefaultStuffFor(def);
        }
    }
}
