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
    public class Dialog_LoadoutConfigurator : Window
    {
        private Vector2 scrollPosition;
        private readonly Building_WorkTable building;

        private readonly Dictionary<BodyPartGroupDef, Dictionary<ApparelLayerDef, ThingDef>> selectedApparel =
            new Dictionary<BodyPartGroupDef, Dictionary<ApparelLayerDef, ThingDef>>();

        private readonly Dictionary<ThingDef, List<Tuple<BodyPartGroupDef, ApparelLayerDef>>> filledFromPrimary =
            new Dictionary<ThingDef, List<Tuple<BodyPartGroupDef, ApparelLayerDef>>>();

        private readonly List<Tuple<BodyPartGroupDef, ApparelLayerDef>> validCombos =
            new List<Tuple<BodyPartGroupDef, ApparelLayerDef>>();

        private readonly Dictionary<Tuple<BodyPartGroupDef, ApparelLayerDef>, List<ThingDef>> perRowFilterCache =
            new Dictionary<Tuple<BodyPartGroupDef, ApparelLayerDef>, List<ThingDef>>();

        private readonly ObjectPool<List<FloatMenuOption>> floatMenuOptionPool =
            new ObjectPool<List<FloatMenuOption>>(delegate { return new List<FloatMenuOption>(); }, delegate (List<FloatMenuOption> list) { list.Clear(); });

        private const float RowHeight = 60f;
        private const float TooltipPanelWidth = 250f;
        private const float PawnPreviewPanelWidth = 220f;
        private ThingDef hoveredApparel;
        private Pawn previewPawn;

        private readonly Action<List<ThingDef>> onApply;
        private readonly Action onCancel;

        public override Vector2 InitialSize => new Vector2(1150f, 640f);

        public Dialog_LoadoutConfigurator(Building_WorkTable building, Action<List<ThingDef>> onApply, Action onCancel)
        {
            this.building = building;
            this.onApply = onApply;
            this.onCancel = onCancel;

            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;

            PrecomputeValidCombinations();
            InitPreviewPawn();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect listRect = new Rect(inRect.x, inRect.y, inRect.width - TooltipPanelWidth - PawnPreviewPanelWidth - 20f, inRect.height - 50f);
            Rect tooltipRect = new Rect(listRect.xMax + 5f, inRect.y, TooltipPanelWidth, inRect.height - 50f);
            Rect pawnRect = new Rect(tooltipRect.xMax + 5f, inRect.y, PawnPreviewPanelWidth, inRect.height - 50f);
            Rect buttonRect = new Rect(0f, inRect.height - 45f, inRect.width, 40f);

            float totalHeight = validCombos.Count * RowHeight;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, totalHeight);

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
            DrawComboRows(viewRect);
            Widgets.EndScrollView();

            DrawTooltipPreview(tooltipRect);
            DrawPawnPreview(pawnRect);
            DrawBottomButtons(buttonRect);
        }

        private void PrecomputeValidCombinations()
        {
            var allApparel = DefDatabase<ThingDef>.AllDefs
                .Where(td => td.IsApparel && Utility_BillConfigurator.IsItemAvailableAtBench(td, building.def))
                .ToList();

            var bodyGroups = allApparel.SelectMany(td => td.apparel.bodyPartGroups).Distinct();
            var layers = allApparel.SelectMany(td => td.apparel.layers).Distinct();

            foreach (var bg in bodyGroups)
            {
                foreach (var layer in layers)
                {
                    var matches = allApparel
                        .Where(td => td.apparel.bodyPartGroups.Contains(bg) && td.apparel.layers.Contains(layer))
                        .ToList();

                    if (matches.Any())
                    {
                        var combo = Tuple.Create(bg, layer);
                        validCombos.Add(combo);
                        perRowFilterCache[combo] = matches;
                    }
                }
            }

            Utility_BillConfigurator.filteredItems = allApparel;
        }

        private void DrawComboRows(Rect viewRect)
        {
            float y = 0f;
            List<FloatMenuOption> options = floatMenuOptionPool.Get();

            foreach (Tuple<BodyPartGroupDef, ApparelLayerDef> combo in validCombos)
            {
                BodyPartGroupDef bg = combo.Item1;
                ApparelLayerDef layer = combo.Item2;
                Tuple<BodyPartGroupDef, ApparelLayerDef> rowKey = Tuple.Create(bg, layer);

                if (!selectedApparel.ContainsKey(bg))
                    selectedApparel[bg] = new Dictionary<ApparelLayerDef, ThingDef>();

                Rect rowRect = new Rect(0f, y, viewRect.width, RowHeight);
                Widgets.DrawBox(rowRect);

                Rect labelRect = new Rect(rowRect.x + 10f, rowRect.y + 5f, 300f, 30f);
                Widgets.Label(labelRect, $"{bg.label} – {layer.label}");

                Rect buttonRect = new Rect(labelRect.xMax + 10f, rowRect.y + 5f, 240f, 30f);

                ThingDef selected = selectedApparel[bg].TryGetValue(layer, out ThingDef sel) ? sel : null;
                string label = selected != null ? selected.LabelCap : "EM_LoadoutConfigurator_Select".Translate();

                List<ThingDef> validOptions = perRowFilterCache.ContainsKey(rowKey) ? perRowFilterCache[rowKey] : new List<ThingDef>();
                bool hasOptions = validOptions.Any();

                bool isLocked = false;
                foreach (KeyValuePair<ThingDef, List<Tuple<BodyPartGroupDef, ApparelLayerDef>>> kv in filledFromPrimary)
                {
                    if (kv.Value.Contains(rowKey))
                    {
                        // Only allow interaction for the first row it appears in
                        if (!kv.Value[0].Equals(rowKey))
                            isLocked = true;
                        break;
                    }
                }

                if (hasOptions)
                {
                    if (!isLocked && Widgets.ButtonText(buttonRect, label))
                    {
                        options.Clear();
                        foreach (ThingDef td in validOptions)
                        {
                            options.Add(new FloatMenuOption(td.LabelCap, delegate
                            {
                                ApplyApparelToAllCompatibleRows(td);
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }

                    if (selected != null)
                    {
                        TooltipHandler.TipRegion(buttonRect, BuildTooltipForItem(selected));
                        if (Mouse.IsOver(buttonRect))
                            hoveredApparel = selected;
                    }

                    if (isLocked)
                    {
                        GUI.color = Color.gray;
                        Widgets.DrawBoxSolid(buttonRect, new Color(0.3f, 0.3f, 0.3f, 0.5f));
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.Label(buttonRect, label);
                        Text.Anchor = TextAnchor.UpperLeft;
                        GUI.color = Color.white;
                    }
                }
                else
                {
                    GUI.color = Color.gray;
                    Widgets.DrawBox(buttonRect);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(buttonRect, label);
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = Color.white;
                }

                y += RowHeight;
            }

            floatMenuOptionPool.Release(options);
        }

        private void ApplyApparelToAllCompatibleRows(ThingDef apparel)
        {
            // Step 1: Find all combinations that this apparel can occupy
            List<Tuple<BodyPartGroupDef, ApparelLayerDef>> newCombos = new List<Tuple<BodyPartGroupDef, ApparelLayerDef>>();
            foreach (Tuple<BodyPartGroupDef, ApparelLayerDef> combo in validCombos)
            {
                if (apparel.apparel.bodyPartGroups.Contains(combo.Item1) &&
                    apparel.apparel.layers.Contains(combo.Item2))
                {
                    newCombos.Add(combo);
                }
            }

            // Step 2: Remove only from conflicting entries
            foreach (Tuple<BodyPartGroupDef, ApparelLayerDef> pair in newCombos)
            {
                foreach (KeyValuePair<ThingDef, List<Tuple<BodyPartGroupDef, ApparelLayerDef>>> kv in filledFromPrimary)
                {
                    if (!kv.Key.Equals(apparel) && kv.Value.Contains(pair))
                    {
                        if (selectedApparel.ContainsKey(pair.Item1))
                        {
                            selectedApparel[pair.Item1].Remove(pair.Item2);
                        }
                        kv.Value.Remove(pair);
                    }
                }
            }

            // Step 3: Clean up empty entries
            List<ThingDef> keysToRemove = new List<ThingDef>();
            foreach (KeyValuePair<ThingDef, List<Tuple<BodyPartGroupDef, ApparelLayerDef>>> kv in filledFromPrimary)
            {
                if (kv.Value.Count == 0)
                {
                    keysToRemove.Add(kv.Key);
                }
            }
            foreach (ThingDef key in keysToRemove)
            {
                filledFromPrimary.Remove(key);
            }

            // Step 4: Apply new selection
            foreach (Tuple<BodyPartGroupDef, ApparelLayerDef> combo in newCombos)
            {
                if (!selectedApparel.ContainsKey(combo.Item1))
                {
                    selectedApparel[combo.Item1] = new Dictionary<ApparelLayerDef, ThingDef>();
                }
                selectedApparel[combo.Item1][combo.Item2] = apparel;
            }

            filledFromPrimary[apparel] = newCombos;
        }

        private void DrawTooltipPreview(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect.ContractedBy(6f));

            if (hoveredApparel != null)
            {
                Widgets.Label(new Rect(0f, 0f, rect.width - 12f, 30f), hoveredApparel.LabelCap);
                Widgets.Label(new Rect(0f, 40f, rect.width - 12f, rect.height - 40f), hoveredApparel.description);
            }
            else
            {
                Widgets.Label(new Rect(0f, 0f, rect.width - 12f, 60f), "EM_LoadoutConfigurator_HoverPreview".Translate());
            }

            GUI.EndGroup();
        }

        private void DrawPawnPreview(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect.ContractedBy(5f));

            previewPawn.apparel?.WornApparel?.Clear();

            foreach (var def in filledFromPrimary.Keys.ToList())
            {
                var stuff = def.MadeFromStuff ? Utility_BillConfigurator.GetDefaultStuff(def) : null;
                var item = (Apparel)ThingMaker.MakeThing(def, stuff);
                previewPawn.apparel.Wear(item, false);
            }

            var portraitRect = new Rect(0f, 0f, rect.width - 10f, rect.height - 10f);
            var portrait = PortraitsCache.Get(previewPawn, portraitRect.size, Rot4.South, Vector3.zero, 1.0f);
            GUI.DrawTexture(portraitRect, portrait);

            GUI.EndGroup();
        }

        private void InitPreviewPawn()
        {
            var comp = building.GetComp<CompBillConfigurator>();
            var props = comp?.Props;

            var gender = props?.previewPawnGender ?? Gender.Male;
            var bodyType = props?.previewPawnBodyType ?? BodyTypeDefOf.Hulk;

            var request = new PawnGenerationRequest(
                kind: Faction.OfPlayer.def.basicMemberKind,
                faction: Faction.OfPlayer,
                fixedGender: gender
            );

            previewPawn = PawnGenerator.GeneratePawn(request);
            if (previewPawn?.story != null && bodyType != null)
                previewPawn.story.bodyType = bodyType;
        }

        private void DrawBottomButtons(Rect rect)
        {
            float third = rect.width / 3f;
            Rect cancelRect = new Rect(rect.x, rect.y, third, rect.height);
            Rect resetRect = new Rect(rect.x + third, rect.y, third, rect.height);
            Rect applyRect = new Rect(rect.x + 2 * third, rect.y, third, rect.height);

            if (Widgets.ButtonText(cancelRect, "EM_LoadoutConfigurator_ButtonCancel".Translate()))
            {
                onCancel?.Invoke();
                Close();
            }

            if (Widgets.ButtonText(resetRect, "EM_LoadoutConfigurator_ResetAll".Translate()))
            {
                selectedApparel.Clear();
                filledFromPrimary.Clear();
                previewPawn?.apparel?.WornApparel?.Clear();
            }

            bool hasSelection = filledFromPrimary.Keys.Any();
            if (!hasSelection)
            {
                GUI.color = Color.gray;
                Widgets.DrawAtlas(applyRect, Widgets.ButtonBGAtlas);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(applyRect, "EM_LoadoutConfigurator_ButtonConfirm".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                TooltipHandler.TipRegion(applyRect, "EM_LoadoutConfigurator_NoSelection".Translate());
            }
            else if (Widgets.ButtonText(applyRect, "EM_LoadoutConfigurator_ButtonConfirm".Translate()))
            {
                var selectedDefs = filledFromPrimary.Keys.ToList();
                onApply?.Invoke(selectedDefs);
                Close();
            }
        }

        private string BuildTooltipForItem(ThingDef def)
        {
            var sb = new StringBuilder();
            sb.AppendLine(def.LabelCap.Colorize(ColoredText.TipSectionTitleColor));

            var recipes = Utility_BillConfigurator.GetRecipesForItemAtWorkbench(def, building.def);
            if (recipes.Any())
            {
                sb.AppendLine("EM_LoadoutConfigurator_AvailableRecipes".Translate());
                foreach (var r in recipes)
                {
                    bool unlocked = Utility_BillConfigurator.AreResearchPrerequisitesMet(r);
                    sb.AppendLine(unlocked
                        ? $"  <color=green>✓ {r.LabelCap}</color>"
                        : $"  <color=red>✗ {r.LabelCap}</color>");
                }
            }

            if (!def.description.NullOrEmpty())
                sb.AppendLine("\n" + def.description);

            return sb.ToString();
        }

        public class ObjectPool<T> where T : class
        {
            private readonly Func<T> createFunc;
            private readonly Action<T> resetFunc;
            private readonly Stack<T> pool = new Stack<T>();

            public ObjectPool(Func<T> createFunc, Action<T> resetFunc = null)
            {
                this.createFunc = createFunc;
                this.resetFunc = resetFunc ?? delegate { };
            }

            public T Get()
            {
                return pool.Count > 0 ? pool.Pop() : createFunc();
            }

            public void Release(T obj)
            {
                resetFunc(obj);
                pool.Push(obj);
            }
        }
    }
}