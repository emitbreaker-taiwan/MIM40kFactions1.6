using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class Dialog_BillConfigurator : Window
    {
        private readonly Building_WorkTable building;
        private readonly Action onAccept;
        private readonly Action onCancel;

        // Configuration options
        private List<ThingDef> availableItems;
        private List<ThingDef> selectedItems = new List<ThingDef>();

        private Vector2 availableItemsScrollPos;
        private Vector2 selectedItemsScrollPos; // New separate scroll position

        // Search function
        private string searchString = "";
        private List<ThingDef> filteredAvailableItems = Utility_BillConfigurator.filteredItems; // Access shared filtered list

        // Add these near your other filter variables
        private string researchFilterMode = "EM_BillConfigurator_ResearchFilter_AllLabel"; // "All" or "Specific"
        private const string ResearchFilterAllKey = "EM_BillConfigurator_ResearchFilter_AllLabel";
        private const string ResearchFilterSpecificKey = "EM_BillConfigurator_ResearchFilter_SpecificLabel";
        private const string SelectResearchKey = "EM_BillConfigurator_ResearchFilter_SelectResearchLabel";
        private const string MaxBillsInQueueReachedKey = "EM_BillConfigurator_MaxXBillsInQueueReached";
        private ResearchProjectDef selectedResearchFilter = null;

        private string apparelLayerFilter = "EM_BillConfigurator_ResearchFilter_ApparelLayerAllLabel"; // "All" or specific layer names
        private const string apparelLayerFilterAllKey = "EM_BillConfigurator_ResearchFilter_ApparelLayerAllLabel";
        private const string searchTitleKey = "EM_BillConfigurator_searchLabel";
        private const string clearAllSelectedItemsKey = "EM_BillConfigurator_clearAllLabel";

        // Comps
        private CompBillConfigurator compBillConfigurator;
        private string titleLabel = "Configure";
        private string searchPlaceholderLabel = "Search items..."; 
        private string availableItemsLabel = "Available items:";
        private string selectedItemsLabel = "Selected items:"; 
        private string availableRecipesLabel = "Available recipes:";
        private string researchRequiresLabel = "Research required:";
        private string buttonCancelLabel = "Cancel";
        private string buttonConfirmLabel = "Confirm"; 
        private string researchFilterLabel = "Filter by research required";
        private string apparelLayerFilterLabel = "Filter by apparel layer";
        private string queueItemsLabel = "Queue";
        private string noResearchFound = "No Research Found";
        private string noItemSelected = "No item selected.";
        private string selectedLabel = "Selected:";
        private string noSuggestionLabel = "No suggestions.";

        // Size
        public override Vector2 InitialSize => new Vector2(1280f, 800f); // Wider window

        // Add these constants near the top of your class
        private const float FilterPaneWidth = 200f;
        private const float ListPaneWidth = 400f;
        private const float PaddingBetweenPanes = 10f;

        private const int MaxSelectedItems = 15; // Maximum items that can be selected
        private const int MaxBillsInQueue = 15; // Maximum bills the workbench can have

        private ThingDef highlightedAvailableItem;
        private Vector2 suggestionScrollPos;
        private ThingDef draggingItem = null;
        private int draggingIndex = -1;
        private Vector2 draggingOffset;

        private bool showSuggestionPane = true;

        // Method to open the Loadout Configurator as a modal overlay
        private void OpenLoadoutConfigurator()
        {
            Find.WindowStack.Add(new Dialog_LoadoutConfigurator(building, ApplyLoadoutFromConfigurator, () => { }));
        }

        public Dialog_BillConfigurator(Building_WorkTable building, Action onAccept, Action onCancel)
        {
            this.building = building;
            this.onAccept = onAccept;
            this.onCancel = onCancel;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnClickedOutside = false;
            this.compBillConfigurator = building.GetComp<CompBillConfigurator>();
            InitializeAvailableItems();
        }

        protected virtual void InitializeAvailableItems()
        {
            availableItems = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(td => Utility_BillConfigurator.IsItemAvailableAtBench(td, building.def))
                .OrderBy(t => t.label)
                .ToList();

            filteredAvailableItems = new List<ThingDef>(availableItems);
            Utility_BillConfigurator.filteredItems = filteredAvailableItems;
        }

        private void UpdateFilteredItems()
        {
            filteredAvailableItems = availableItems
                .Where(item =>
                    (searchString.NullOrEmpty() || item.label.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (researchFilterMode == ResearchFilterAllKey ||
                     (researchFilterMode == ResearchFilterSpecificKey && selectedResearchFilter != null &&
                      Utility_BillConfigurator.ItemRequiresResearch(item, selectedResearchFilter, building.def))) &&
                    (apparelLayerFilter == ResearchFilterAllKey || MeetsApparelLayerFilter(item))
                )
                .ToList();

            Utility_BillConfigurator.filteredItems = filteredAvailableItems;
        }

        private IEnumerable<ResearchProjectDef> GetAllRelevantResearches()
        {
            return Utility_BillConfigurator.GetAllRelevantResearchesForItemSet(availableItems, building.def);
        }

        private bool ItemRequiresResearch(ThingDef item, ResearchProjectDef research)
        {
            var recipes = GetRecipesForItem(item);
            return recipes.Any(r =>
                r.researchPrerequisite == research ||
                (r.researchPrerequisites?.Contains(research) == true));
        }

        private bool MeetsApparelLayerFilter(ThingDef item)
        {
            if (apparelLayerFilter == apparelLayerFilterAllKey)
                return true;

            return item.IsApparel && item.apparel.layers.Any(l => l.defName == apparelLayerFilter);
        }

        private List<RecipeDef> GetRecipesForItem(ThingDef item)
        {
            return Utility_BillConfigurator.GetRecipesForItemAtWorkbench(item, building.def);
        }

        private bool IsItemAvailableAtBench(ThingDef item)
        {
            var relevantRecipes = Utility_BillConfigurator.GetRecipesForItemAtWorkbench(item, building.def);
            return relevantRecipes.Any(recipe => Utility_BillConfigurator.AreResearchPrerequisitesMet(recipe));
        }

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                const float titleHeight = 35f;
                const float filterRowHeight = 40f;
                const float buttonHeight = 55f;
                const float verticalPadding = 10f;
                const float betweenSectionsPadding = 10f;

                // TITLE
                Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, titleHeight);
                Text.Font = GameFont.Medium;
                if (compBillConfigurator != null)
                {
                    titleLabel = compBillConfigurator.Props.titleLabel;
                }
                Widgets.Label(titleRect, titleLabel.Translate() + " " + building.LabelCap);
                Text.Font = GameFont.Small;

                // Calculate dynamic button width based on the button text
                string openLoadoutButtonLabel = "EM_LoadoutConfigurator_Open".Translate();
                string toggleLabel = showSuggestionPane ? "EM_BillConfigurator_HideSuggestionLabel".Translate() : "EM_BillConfigurator_ShowSuggestionLabel".Translate();

                float buttonSpacing = 10f;
                float openLoadoutButtonWidth = Text.CalcSize(openLoadoutButtonLabel).x + buttonSpacing * 2;  // Width based on label text
                float toggleButtonWidth = Text.CalcSize(toggleLabel).x + buttonSpacing * 2;  // Width based on label text

                // Position the buttons on the right side of the title bar
                float totalButtonWidth = openLoadoutButtonWidth + toggleButtonWidth + buttonSpacing; // Total width of both buttons and spacing
                float startX = inRect.width - totalButtonWidth; // Calculate starting position for the buttons on the right

                // Open Loadout Configurator Button
                Rect openLoadoutButtonRect = new Rect(startX, titleRect.y + 5f, openLoadoutButtonWidth, 25f);
                if (Widgets.ButtonText(openLoadoutButtonRect, openLoadoutButtonLabel))
                {
                    OpenLoadoutConfigurator();
                }

                // Show/Hide Suggestions Button
                Rect toggleRect = new Rect(openLoadoutButtonRect.xMax + buttonSpacing, titleRect.y + 5f, toggleButtonWidth, 25f);
                if (Widgets.ButtonText(toggleRect, toggleLabel))
                {
                    showSuggestionPane = !showSuggestionPane;
                }

                // SEARCH AND FILTERS
                Rect filterRowRect = new Rect(inRect.x, titleRect.yMax + verticalPadding, inRect.width, filterRowHeight);
                DoTopFilterRow(filterRowRect);

                // MAIN CONTENT
                Rect contentRect = new Rect(
                    inRect.x,
                    filterRowRect.yMax + betweenSectionsPadding,
                    inRect.width,
                    inRect.height - titleHeight - filterRowHeight - verticalPadding * 2 - buttonHeight
                );

                float listPaneWidth;
                float suggestionPaneWidth = 300f;

                if (showSuggestionPane)
                {
                    listPaneWidth = (contentRect.width - suggestionPaneWidth - PaddingBetweenPanes * 2) / 2f;
                }
                else
                {
                    listPaneWidth = (contentRect.width - PaddingBetweenPanes) / 2f;
                }

                Rect availablePaneRect = new Rect(
                    contentRect.x,
                    contentRect.y,
                    listPaneWidth,
                    contentRect.height
                );

                Rect suggestionPaneRect = new Rect(
                    availablePaneRect.xMax + PaddingBetweenPanes,
                    contentRect.y,
                    suggestionPaneWidth,
                    contentRect.height
                );

                Rect selectedPaneRect = showSuggestionPane
                    ? new Rect(suggestionPaneRect.xMax + PaddingBetweenPanes, contentRect.y, listPaneWidth, contentRect.height)
                    : new Rect(availablePaneRect.xMax + PaddingBetweenPanes, contentRect.y, listPaneWidth, contentRect.height);

                DoAvailableItemsPane(availablePaneRect);

                if (showSuggestionPane)
                {
                    DoSuggestionPane(suggestionPaneRect);
                }

                DoSelectedItemsPane(selectedPaneRect);

                // BUTTONS
                Rect buttonRect = new Rect(inRect.x, inRect.yMax - buttonHeight, inRect.width, buttonHeight);
                DoBottomButtons(buttonRect);
            }
            catch (Exception ex)
            {
                Log.Error($"Error drawing config dialog: {ex}");
                Close();
            }
            finally
            {
                Widgets.EnsureMousePositionStackEmpty();
            }
        }

        private void DoTopFilterRow(Rect rect)
        {
            float spacing = 10f;
            float searchWidth = 250f;
            float buttonWidth = 180f;
            float clearButtonWidth = 24f;
            float height = rect.height;

            float x = 0f;

            // Search label and box
            Rect searchLabelRect = new Rect(x, rect.y + 4f, 60f, height - 4f);
            Widgets.Label(searchLabelRect, searchTitleKey.Translate());
            x += searchLabelRect.width + 4f;

            Rect searchRect = new Rect(x, rect.y + 2f, searchWidth, height - 6f);
            string newSearch = Widgets.TextField(searchRect, searchString);
            if (newSearch != searchString)
            {
                searchString = newSearch;
                UpdateFilteredItems();
            }
            if (searchString.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                Widgets.Label(new Rect(searchRect.x + 10f, searchRect.y, searchRect.width - 20f, searchRect.height),
                    (compBillConfigurator?.Props.searchPlaceholderLabel ?? searchPlaceholderLabel).Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            x += searchRect.width + 2f;

            // Clear search button
            Rect clearRect = new Rect(x, rect.y + 6f, clearButtonWidth, clearButtonWidth);
            if (!searchString.NullOrEmpty() && Widgets.ButtonImage(clearRect, TexButton.CloseXSmall))
            {
                ClearSearch();
                GUI.FocusControl(null);
            }
            x += clearButtonWidth + spacing;

            // Research filter
            Rect researchRect = new Rect(x, rect.y + 2f, buttonWidth, height - 6f);
            if (compBillConfigurator != null)
            {
                researchFilterLabel = compBillConfigurator.Props.researchFilterLabel;
            }
            TooltipHandler.TipRegion(researchRect, researchFilterLabel.Translate());
            if (Widgets.ButtonText(researchRect, researchFilterMode.Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(ResearchFilterAllKey.Translate(), () => {
                        researchFilterMode = ResearchFilterAllKey;
                        selectedResearchFilter = null;
                        UpdateFilteredItems();
                    }),
                    new FloatMenuOption(ResearchFilterSpecificKey.Translate(), () => {
                        researchFilterMode = ResearchFilterSpecificKey;
                        UpdateFilteredItems();
                    })
                };
                Find.WindowStack.Add(new FloatMenu(options));
            }
            x += buttonWidth + spacing;

            // Specific research selector (if needed)
            if (researchFilterMode == ResearchFilterSpecificKey)
            {
                Rect specificResearchRect = new Rect(x, rect.y + 2f, buttonWidth, height - 6f);
                string label = selectedResearchFilter?.LabelCap ?? SelectResearchKey.Translate();
                if (Widgets.ButtonText(specificResearchRect, label))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (var research in GetAllRelevantResearches())
                    {
                        options.Add(new FloatMenuOption(research.LabelCap, () => {
                            selectedResearchFilter = research;
                            UpdateFilteredItems();
                        }));
                    }
                    if (!options.Any())
                    {
                        options.Add(new FloatMenuOption((compBillConfigurator?.Props.noResearchFound ?? noResearchFound).Translate(), null));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                x += buttonWidth + spacing;
            }

            // Apparel layer filter
            Rect layerRect = new Rect(x, rect.y + 2f, buttonWidth, height - 6f);
            if (compBillConfigurator.Props.apparelLayerFilterLabel != null)
            {
                apparelLayerFilterLabel = compBillConfigurator.Props.apparelLayerFilterLabel;
            }
            TooltipHandler.TipRegion(layerRect, apparelLayerFilterLabel.Translate());
            if (Widgets.ButtonText(layerRect, GetApparelLayerFilterLabel()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(apparelLayerFilterAllKey.Translate(), () => {
                        apparelLayerFilter = apparelLayerFilterAllKey;
                        UpdateFilteredItems();
                    })
                };
                foreach (var layer in DefDatabase<ApparelLayerDef>.AllDefs)
                {
                    options.Add(new FloatMenuOption(layer.LabelCap, () => {
                        apparelLayerFilter = layer.defName;
                        UpdateFilteredItems();
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void ApplyLoadoutFromConfigurator(List<ThingDef> apparelDefs)
        {
            foreach (var def in apparelDefs)
            {
                if (!selectedItems.Contains(def) && selectedItems.Count < MaxSelectedItems)
                {
                    selectedItems.Add(def);
                    highlightedAvailableItem = def; // 👈 Set highlight for the last one
                }
            }

            // Auto-apply if eligible
            if (CanAddMoreBills())
            {
                ApplyConfiguration();
                onAccept?.Invoke();
                Close();
            }
        }

        private void DoSuggestionPane(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect.ContractedBy(5f));
            try
            {
                float y = 0f;
                const float lineHeight = 28f;

                if (highlightedAvailableItem == null)
                {
                    if (compBillConfigurator.Props.noItemSelected != null)
                    {
                        noItemSelected = compBillConfigurator.Props.noItemSelected;
                    }
                    Widgets.Label(new Rect(0f, y, rect.width - 10f, lineHeight), noItemSelected.Translate());
                    return;
                }

                // Header
                if (compBillConfigurator.Props.selectedLabel != null)
                {
                    selectedLabel = compBillConfigurator.Props.selectedLabel;
                }
                Widgets.Label(new Rect(0f, y, rect.width - 10f, lineHeight), selectedLabel.Translate() + " " + highlightedAvailableItem.LabelCap);
                y += lineHeight + 5f;

                List<ThingDef> suggestions = GetSuggestionsForItem(highlightedAvailableItem);
                if (suggestions.NullOrEmpty())
                {
                    if (compBillConfigurator.Props.noSuggestionLabel != null)
                    {
                        noSuggestionLabel = compBillConfigurator.Props.noSuggestionLabel;
                    }
                    Widgets.Label(new Rect(0f, y, rect.width - 10f, lineHeight), noSuggestionLabel.Translate());
                    return;
                }

                Rect scrollRect = new Rect(0f, y, rect.width - 10f, rect.height - y);
                Rect viewRect = new Rect(0f, 0f, scrollRect.width - 16f, suggestions.Count * lineHeight);

                Widgets.BeginScrollView(scrollRect, ref suggestionScrollPos, viewRect);
                Rect itemRect = new Rect(0f, 0f, viewRect.width, lineHeight);

                foreach (var item in suggestions)
                {
                    if (Widgets.ButtonText(itemRect, item.LabelCap))
                    {
                        // Optional: add to selectedItems if not full
                        if (!selectedItems.Contains(item) && selectedItems.Count < MaxSelectedItems)
                        {
                            selectedItems.Add(item);
                            SoundDefOf.Click.PlayOneShotOnCamera();
                        }
                    }
                    itemRect.y += lineHeight;
                }

                Widgets.EndScrollView();
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private List<ThingDef> GetSuggestionsForItem(ThingDef baseItem)
        {
            if (baseItem == null)
                return new List<ThingDef>();

            // === Handle CE-style weapon suggestion via reflection ===
            if (baseItem.IsWeapon && compBillConfigurator?.Props?.ceAmmoCompClass != null)
            {
                try
                {
                    var ceCompType = GenTypes.GetTypeInAnyAssembly(compBillConfigurator.Props.ceAmmoCompClass);
                    if (ceCompType != null)
                    {
                        var ceComp = baseItem.comps?.FirstOrDefault(c => ceCompType.IsAssignableFrom(c.GetType()));
                        if (ceComp != null)
                        {
                            // Use reflection to access ammoSet.ammoTypes
                            var ammoSetField = ceCompType.GetField("ammoSet");
                            var ammoSet = ammoSetField?.GetValue(ceComp);

                            if (ammoSet != null)
                            {
                                var ammoSetType = ammoSet.GetType();
                                var ammoTypesField = ammoSetType.GetField("ammoTypes");

                                if (ammoTypesField != null)
                                {
                                    var ammoTypes = ammoTypesField.GetValue(ammoSet) as IEnumerable<object>;
                                    var result = new List<ThingDef>();

                                    foreach (var ammoType in ammoTypes)
                                    {
                                        var ammoDefField = ammoType.GetType().GetField("ammo");
                                        if (ammoDefField != null)
                                        {
                                            var ammoDef = ammoDefField.GetValue(ammoType) as ThingDef;
                                            if (ammoDef != null)
                                            {
                                                result.Add(ammoDef);
                                            }
                                        }
                                    }

                                    return result.Distinct().ToList();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[BillConfigurator] Failed to resolve CE ammo suggestions for {baseItem.defName}: {ex.Message}");
                }

                return new List<ThingDef>(); // Fallback to no suggestions
            }

            // === Apparel suggestions ===
            if (!baseItem.IsApparel || baseItem.apparel == null || baseItem.apparel.bodyPartGroups.NullOrEmpty())
                return new List<ThingDef>();

            var baseGroups = baseItem.apparel.bodyPartGroups.Select(g => g.defName).ToHashSet();
            var baseLayers = baseItem.apparel.layers.Select(l => l.defName).ToHashSet();

            return filteredAvailableItems
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

        private void ClearSearch()
        {
            searchString = "";
            UpdateFilteredItems();
        }

        private void DoFilterPane(Rect rect)
        {
            // Background
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect.ContractedBy(5f));
            Rect innerRect = new Rect(0f, 0f, rect.width - 10f, rect.height - 10f);

            try
            {
                float yPos = 0f;
                const float sectionSpacing = 15f;
                const float filterHeight = 30f;
                const float searchHeight = 30f;

                // SEARCH FILTER
                Rect searchRect = new Rect(0f, yPos, innerRect.width - 25f, searchHeight);
                Rect clearButtonRect = new Rect(innerRect.width - 20f, yPos, 20f, 20f);

                string newSearch = Widgets.TextField(searchRect, searchString);
                if (newSearch != searchString)
                {
                    searchString = newSearch;
                    UpdateFilteredItems();
                }

                // Clear button
                if (!searchString.NullOrEmpty())
                {
                    if (Widgets.ButtonImage(clearButtonRect, TexButton.CloseXSmall))
                    {
                        ClearSearch();
                        GUI.FocusControl(null);
                    }
                }

                // Placeholder text
                if (searchString.NullOrEmpty())
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    if (compBillConfigurator != null)
                    {
                        searchPlaceholderLabel = compBillConfigurator.Props.searchPlaceholderLabel;
                    }
                    Widgets.Label(new Rect(searchRect.x + 10f, searchRect.y, searchRect.width - 20f, searchRect.height),
                        searchPlaceholderLabel.Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = Color.white;
                }

                yPos += searchHeight + sectionSpacing;

                // RESEARCH FILTER
                if (compBillConfigurator != null)
                {
                    researchFilterLabel = compBillConfigurator.Props.researchFilterLabel;
                }
                Widgets.Label(new Rect(0f, yPos, innerRect.width, filterHeight), researchFilterLabel.Translate());
                yPos += filterHeight;

                // Research mode toggle 
                Rect researchModeRect = new Rect(0f, yPos, innerRect.width, filterHeight);
                if (Widgets.ButtonText(researchModeRect, researchFilterMode.Translate()))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>
                    {
                        new FloatMenuOption(ResearchFilterAllKey.Translate(), () => {
                            researchFilterMode = ResearchFilterAllKey;
                            selectedResearchFilter = null;
                            UpdateFilteredItems();
                        }),
                        new FloatMenuOption(ResearchFilterSpecificKey.Translate(), () => {
                            researchFilterMode = ResearchFilterSpecificKey;
                            UpdateFilteredItems();
                        })
                    };
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                yPos += filterHeight;

                // Specific research selector (only shown when mode is "Specific")
                if (researchFilterMode == ResearchFilterSpecificKey)
                {
                    Rect specificResearchRect = new Rect(0f, yPos, innerRect.width, filterHeight);
                    string label = selectedResearchFilter?.LabelCap ?? SelectResearchKey.Translate();

                    if (Widgets.ButtonText(specificResearchRect, label))
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();

                        foreach (var research in GetAllRelevantResearches())
                        {
                            options.Add(new FloatMenuOption(research.LabelCap, () =>
                            {
                                selectedResearchFilter = research;
                                UpdateFilteredItems();
                            }));
                        }

                        if (!options.Any())
                        {
                            if (compBillConfigurator != null)
                            {
                                noResearchFound = compBillConfigurator.Props.noResearchFound;
                            }
                            options.Add(new FloatMenuOption(noResearchFound.Translate(), null));
                        }

                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    yPos += filterHeight + sectionSpacing;
                }
                else
                {
                    yPos += sectionSpacing;
                }

                // APPAREL LAYER FILTER
                if (compBillConfigurator != null)
                {
                    apparelLayerFilterLabel = compBillConfigurator.Props.apparelLayerFilterLabel;
                }
                Widgets.Label(new Rect(0f, yPos, innerRect.width, filterHeight), apparelLayerFilterLabel.Translate());
                yPos += filterHeight;

                Rect apparelFilterRect = new Rect(0f, yPos, innerRect.width, filterHeight);
                if (Widgets.ButtonText(apparelFilterRect, GetApparelLayerFilterLabel()))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption(apparelLayerFilterAllKey.Translate(), () => { apparelLayerFilter = apparelLayerFilterAllKey; UpdateFilteredItems(); })
            };

                    foreach (var layer in DefDatabase<ApparelLayerDef>.AllDefs)
                    {
                        options.Add(new FloatMenuOption(layer.LabelCap, () =>
                        {
                            apparelLayerFilter = layer.defName;
                            UpdateFilteredItems();
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DoAvailableItemsPane(Rect rect)
        {
            // Background
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect.ContractedBy(5f));

            try
            {
                if (compBillConfigurator != null)
                {
                    availableItemsLabel = compBillConfigurator.Props.availableItemsLabel;
                }
                // Label
                Widgets.Label(new Rect(0f, 0f, rect.width - 10f, 25f),
                    availableItemsLabel.Translate() + " " + filteredAvailableItems.Count + " / " + availableItems.Count);

                // List
                Rect listRect = new Rect(0f, 30f, rect.width - 10f, rect.height - 35f);
                DoItemsList(listRect, filteredAvailableItems, ref availableItemsScrollPos, item => {
                    if (!selectedItems.Contains(item) && selectedItems.Count < MaxSelectedItems)
                    {
                        selectedItems.Add(item);
                    }
                    highlightedAvailableItem = item; // Track for suggestions
                });
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DoSelectedItemsPane(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect.ContractedBy(5f));

            try
            {
                if (compBillConfigurator != null)
                {
                    selectedItemsLabel = compBillConfigurator.Props.selectedItemsLabel;
                    queueItemsLabel = compBillConfigurator.Props.queueItemsLabel;
                }

                Rect statusRect = new Rect(0f, 0f, rect.width - 10f, 25f);
                string statusText = selectedItemsLabel.Translate() + " " + selectedItems.Count + "/" + MaxSelectedItems +
                                    " (" + queueItemsLabel.Translate() + " " + building.billStack.Count + "/" + MaxBillsInQueue + ")";

                if (building.billStack.Count >= MaxBillsInQueue)
                {
                    GUI.color = ColorLibrary.RedReadable;
                }
                else if (building.billStack.Count + selectedItems.Count > MaxBillsInQueue)
                {
                    GUI.color = Color.yellow;
                }
                else
                {
                    GUI.color = Color.white;
                }

                Widgets.Label(statusRect, statusText);
                GUI.color = Color.white;

                // Clear button
                Rect clearAllRect = new Rect(rect.width - 85f, 0f, 80f, 25f);
                if (Widgets.ButtonText(clearAllRect, clearAllSelectedItemsKey.Translate()))
                {
                    selectedItems.Clear();
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }

                Rect listRect = new Rect(0f, 30f, rect.width - 10f, rect.height - 35f);
                DoItemsList(listRect, selectedItems, ref selectedItemsScrollPos, RemoveSelectedItem, draggable: true);

                if (building.billStack.Count >= MaxBillsInQueue)
                {
                    Rect warningRect = new Rect(0f, listRect.yMax - 30f, listRect.width, 30f);
                    Text.Anchor = TextAnchor.UpperCenter;
                    GUI.color = Color.red;
                    Widgets.Label(warningRect, MaxBillsInQueueReachedKey.Translate(MaxBillsInQueue));
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = Color.white;
                }
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void RemoveSelectedItem(ThingDef item)
        {
            selectedItems.Remove(item);
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

        private string GetApparelLayerFilterLabel()
        {
            if (apparelLayerFilter == apparelLayerFilterAllKey)
                return apparelLayerFilterAllKey.Translate();

            var layerDef = DefDatabase<ApparelLayerDef>.GetNamedSilentFail(apparelLayerFilter);
            return layerDef?.LabelCap ?? apparelLayerFilterAllKey.Translate();
        }

        private void DoItemsList(Rect rect, List<ThingDef> items, ref Vector2 scrollPos, Action<ThingDef> onClick, bool draggable = false)
        {
            try
            {
                GUI.BeginGroup(rect);

                Widgets.DrawMenuSection(new Rect(0f, 0f, rect.width, rect.height));
                Rect innerRect = new Rect(5f, 5f, rect.width - 10f, rect.height - 10f);

                float itemHeight = 36f;
                float totalContentHeight = items.Count * itemHeight;
                bool needsScroll = totalContentHeight > innerRect.height;

                Rect viewRect = new Rect(0f, 0f,
                    innerRect.width - (needsScroll ? 16f : 0f),
                    Mathf.Max(totalContentHeight, innerRect.height));

                if (needsScroll)
                    Widgets.BeginScrollView(innerRect, ref scrollPos, viewRect);

                try
                {
                    Rect itemRect = new Rect(0f, 0f, viewRect.width, itemHeight);
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];

                        if (itemRect.y + itemHeight >= scrollPos.y &&
                            itemRect.y <= scrollPos.y + innerRect.height)
                        {
                            // === DRAG HANDLE ===
                            if (draggable)
                            {
                                Rect gripRect = new Rect(itemRect.x + 2f, itemRect.y + 10f, 12f, 12f);
                                Text.Font = GameFont.Tiny;
                                GUI.color = Color.gray;
                                Widgets.Label(gripRect, "≡");
                                Text.Font = GameFont.Small;
                                GUI.color = Color.white;

                                // Handle dragging
                                if (Mouse.IsOver(itemRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                                {
                                    draggingIndex = i;
                                    draggingItem = item;
                                    draggingOffset = Event.current.mousePosition - itemRect.position;
                                    Event.current.Use();
                                }

                                if (draggingItem != null && draggingIndex == i)
                                {
                                    Vector2 mousePos = Event.current.mousePosition - draggingOffset;
                                    Rect draggedRect = new Rect(mousePos.x, mousePos.y, itemRect.width, itemHeight);
                                    GUI.DrawTexture(draggedRect, TexUI.HighlightTex);
                                    Widgets.Label(new Rect(draggedRect.x + 40f, draggedRect.y, draggedRect.width - 40f, itemHeight), draggingItem.LabelCap);

                                    if (Event.current.type == EventType.MouseUp)
                                    {
                                        int newIndex = Mathf.Clamp((int)((Event.current.mousePosition.y + scrollPos.y) / itemHeight), 0, items.Count - 1);
                                        if (newIndex != draggingIndex)
                                        {
                                            items.Remove(draggingItem);
                                            items.Insert(newIndex, draggingItem);
                                            SoundDefOf.DragSlider.PlayOneShotOnCamera();
                                        }
                                        draggingItem = null;
                                        draggingIndex = -1;
                                        Event.current.Use();
                                    }
                                    continue; // Don't draw the original while dragging
                                }
                            }

                            if (Widgets.ButtonInvisible(itemRect))
                            {
                                onClick?.Invoke(item);
                                SoundDefOf.Click.PlayOneShotOnCamera();
                            }

                            if (Mouse.IsOver(itemRect))
                            {
                                Widgets.DrawHighlight(itemRect);
                                TooltipHandler.TipRegion(itemRect, GetItemTooltip(item));
                            }

                            Widgets.DefIcon(new Rect(itemRect.x + 18f, itemRect.y, 36f, 36f), item);
                            Widgets.Label(new Rect(itemRect.x + 58f, itemRect.y, itemRect.width - 58f, itemHeight), item.LabelCap);
                        }

                        itemRect.y += itemHeight;
                    }
                }
                finally
                {
                    if (needsScroll)
                        Widgets.EndScrollView();
                }

                GUI.EndGroup();
            }
            catch (Exception ex)
            {
                Log.Error($"Error drawing items list: {ex}");
            }
        }

        private string GetItemTooltip(ThingDef item)
        {
            var tip = new StringBuilder();
            tip.AppendLine(item.LabelCap.Colorize(ColoredText.TipSectionTitleColor));

            // Get all recipes for this item at current bench
            var recipes = DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(r => r.products.Any(p => p.thingDef == item))
                .Where(r => r.recipeUsers?.Contains(building.def) == true)
                .ToList();

            if (recipes.Any())
            {
                if (compBillConfigurator != null)
                {
                    availableRecipesLabel = compBillConfigurator.Props.availableRecipesLabel;
                }
                tip.AppendLine(availableRecipesLabel.Translate());
                foreach (var recipe in recipes)
                {
                    bool unlocked = (recipe.researchPrerequisite?.IsFinished ?? true) &&
                                  (recipe.researchPrerequisites?.All(r => r.IsFinished) ?? true);

                    tip.AppendLine(unlocked
                        ? $"  <color=#00FF00>✓ {recipe.LabelCap}</color>"
                        : $"  <color=#FF6347>✗ {recipe.LabelCap}</color>");

                    // Show research requirements
                    if (compBillConfigurator != null)
                    {
                        researchRequiresLabel = compBillConfigurator.Props.researchRequiresLabel;
                    }
                    if (recipe.researchPrerequisite != null)
                    {
                        tip.AppendLine(researchRequiresLabel.Translate() + " " + recipe.researchPrerequisite.LabelCap);
                    }
                    if (recipe.researchPrerequisites != null)
                    {
                        foreach (var research in recipe.researchPrerequisites)
                        {
                            tip.AppendLine(researchRequiresLabel.Translate() + " " + research.LabelCap);
                        }
                    }
                }
            }

            // Add item description
            if (!item.description.NullOrEmpty())
            {
                tip.AppendLine("\n" + item.description);
            }

            return tip.ToString();
        }

        private void DoBottomButtons(Rect rect)
        {
            // Call Label
            if (compBillConfigurator != null)
            {
                buttonCancelLabel = compBillConfigurator.Props.buttonCancelLabel;
                buttonConfirmLabel = compBillConfigurator.Props.buttonConfirmLabel;
            }

            // Cancel button (always active)
            if (Widgets.ButtonText(rect.LeftPartPixels(rect.width / 2f).ContractedBy(5f), buttonCancelLabel.Translate()))
            {
                onCancel();
                Close();
            }

            // Confirm button - handle disabled state properly
            Rect confirmRect = rect.RightPartPixels(rect.width / 2f).ContractedBy(5f);
            bool canConfirm = CanAddMoreBills();

            if (!canConfirm)
            {
                // Draw disabled button
                GUI.color = Color.gray;
                Widgets.DrawAtlas(confirmRect, Widgets.ButtonBGAtlas);
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.gray;
                Widgets.Label(confirmRect, buttonConfirmLabel.Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                // Tooltip explaining why it's disabled
                if (Mouse.IsOver(confirmRect))
                {
                    string reason = building.billStack.Count >= MaxBillsInQueue
                        ? "EM_BillConfigurator_MaxXBillsInQueueReached".Translate(MaxBillsInQueue)
                        : "EM_BillConfigurator_NoItemsSelected".Translate();
                    TooltipHandler.TipRegion(confirmRect, reason);
                }
            }
            else if (Widgets.ButtonText(confirmRect, buttonConfirmLabel.Translate()))
            {
                ApplyConfiguration();
                onAccept?.Invoke();
                Close();
            }
        }

        private bool CanAddMoreBills()
        {
            if (selectedItems.Count == 0)
            {
                return false;
            }
            if (building.billStack.Count >= MaxBillsInQueue)
            {
                return false;
            }

            return (building.billStack.Count + selectedItems.Count) <= MaxBillsInQueue;
        }

        private void ApplyConfiguration()
        {
            // Check if we can add bills
            if (!CanAddMoreBills())
            {
                if (building.billStack.Count >= MaxBillsInQueue)
                {
                    Messages.Message($"{"EM_BillConfigurator_MaxXBillsInQueueReached".Translate()} {MaxBillsInQueue}", MessageTypeDefOf.RejectInput);
                }
                else if (selectedItems.Count == 0)
                {
                    Messages.Message("EM_BillConfigurator_NoItemsSelected".Translate(), MessageTypeDefOf.RejectInput);
                }
                return;
            }

            int successfullyAdded = 0;

            foreach (var item in selectedItems.ToList()) // Using ToList() to avoid modification during iteration
            {
                // Check if we've reached the queue limit during iteration
                if (building.billStack.Count >= MaxBillsInQueue)
                {
                    Messages.Message("EM_BillConfigurator_QueueFullAfterAddingX".Translate(successfullyAdded, MaxBillsInQueue), MessageTypeDefOf.CautionInput);
                    break;
                }

                var recipe = DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(r => r.products.Any(p => p.thingDef == item))
                    .Where(r => r.recipeUsers?.Contains(building.def) == true)
                    .FirstOrDefault(r => (r.researchPrerequisite?.IsFinished ?? true) &&
                                       (r.researchPrerequisites?.All(res => res.IsFinished) ?? true));

                if (recipe != null)
                {
                    var bill = (Bill_Production)BillUtility.MakeNewBill(recipe);
                    bill.repeatMode = BillRepeatModeDefOf.RepeatCount;
                    bill.repeatCount = 1;
                    building.billStack.AddBill(bill);
                    successfullyAdded++;
                }
            }

            // Success message
            if (successfullyAdded > 0)
            {
                string successMsg;
                if (successfullyAdded == 1)
                {
                    successMsg = "EM_BillConfigurator_AddedSingleBill".Translate(building.LabelCap);
                }
                else
                {
                    successMsg = "EM_BillConfigurator_AddedXBillsToY".Translate(successfullyAdded, building.LabelCap);
                }
                Messages.Message(successMsg, MessageTypeDefOf.PositiveEvent);
            }

            // Clear selection if everything was added successfully
            if (successfullyAdded == selectedItems.Count)
            {
                selectedItems.Clear();
            }
            else if (successfullyAdded > 0)
            {
                // Remove only the successfully added items
                selectedItems.RemoveRange(0, successfullyAdded);
            }
        }
    }
}