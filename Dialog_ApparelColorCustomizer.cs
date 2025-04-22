using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MIM40kFactions
{
    public class Dialog_ApparelColorCustomizer : Window
    {
        private Pawn pawn;
        private Thing colorChanger;
        private Vector2 scrollPosition;
        private Dictionary<Apparel, Color> apparelColors = new Dictionary<Apparel, Color>();
        private Dictionary<Apparel, Color> originalColors = new Dictionary<Apparel, Color>();
        private float viewRectHeight;
        private bool showHeadgear = true;
        private bool showClothes = true;

        // RGB editing
        private Apparel selectedApparel;
        private float rValue;
        private float gValue;
        private float bValue;
        private string rInput = "0";
        private string gInput = "0";
        private string bInput = "0";

        // Dye tracking
        private int totalDyeRequired;
        private bool insufficientDyeWarning;

        private List<Color> colorHistory = new List<Color>();
        private const int MaxColorHistory = 8;

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);
        private static readonly Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);
        private const float PortraitZoom = 1.1f;
        private const float LeftRectPercent = 0.3f;
        private const float ApparelRowHeight = 126f;
        private const int DyePerApparelChange = 1;

        private Texture2D hueSatTexture;
        private float valueSlider = 1f;
        private string hexInput = "#FFFFFF";
        private Vector2 rightScrollPos = Vector2.zero;
        private Color lastSelectedColor = Color.white;
        private bool showHiddenApparel = false;

        private enum ColorPickerMode { RG_B, HS_V }
        private ColorPickerMode pickerMode = ColorPickerMode.RG_B;

        private Dictionary<Apparel, Stack<Color>> undoStacks = new Dictionary<Apparel, Stack<Color>>();
        private Dictionary<Apparel, Stack<Color>> redoStacks = new Dictionary<Apparel, Stack<Color>>();
        private List<Apparel> selectedApparels = new List<Apparel>();
        private Color draggedColor = default;
        private bool isDragging = false;

        public override Vector2 InitialSize => new Vector2(1200f, 850f);

        public Dialog_ApparelColorCustomizer(Pawn pawn, Thing colorChanger)
        {
            this.pawn = pawn;
            this.colorChanger = colorChanger;
            this.forcePause = true;
            this.closeOnAccept = false;
            this.closeOnCancel = false;
            this.absorbInputAroundWindow = true;

            foreach (Apparel apparel in pawn.apparel.WornApparel.Where(IsCustomizableApparel))
            {
                apparelColors[apparel] = apparel.DrawColor;
                originalColors[apparel] = apparel.DrawColor;
                undoStacks[apparel] = new Stack<Color>();
                redoStacks[apparel] = new Stack<Color>();
            }

            CalculateDyeRequirements();
        }

        private bool IsCustomizableApparel(Apparel apparel)
        {
            var ext = apparel.def.GetModExtension<PowerArmorCustomizationExtension>();
            if (ext == null) return false;

            if (showHiddenApparel) return true;

            // Skip if the mask texture is missing or all black
            if (!string.IsNullOrEmpty(apparel.def.apparel?.wornGraphicPath))
            {
                try
                {
                    Material mat = GraphicDatabase.Get<Graphic_Multi>(
                        apparel.def.apparel.wornGraphicPath,
                        ShaderDatabase.CutoutComplex,
                        Vector2.one,
                        Color.white).MatAt(Rot4.North);

                    if (mat.HasProperty("_MaskTex"))
                    {
                        Texture maskTex = mat.GetTexture("_MaskTex");

                        if (maskTex is Texture2D tex2D && tex2D.isReadable)
                        {
                            if (IsTextureAllBlack(tex2D))
                            {
                                // Skip this apparel if the mask is black-only
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to check mask texture for {apparel.def.defName}: {ex.Message}");
                }
            }

            return true;
        }

        private bool IsTextureAllBlack(Texture2D tex)
        {
            // Get the pixels of the texture
            Color[] pixels = tex.GetPixels();
            foreach (Color pixel in pixels)
            {
                // If the pixel is not almost black (i.e., R, G, and B components > 0.01)
                if (pixel.r > 0.01f || pixel.g > 0.01f || pixel.b > 0.01f || pixel.a > 0.01f)
                {
                    return false;  // Return false if any pixel is not black
                }
            }
            return true;  // Return true if all pixels are black or near black
        }

        private void CalculateDyeRequirements()
        {
            totalDyeRequired = 0;
            foreach (var kvp in apparelColors)
            {
                if (!kvp.Value.IndistinguishableFrom(originalColors[kvp.Key]))
                {
                    totalDyeRequired += DyePerApparelChange;
                }
            }

            insufficientDyeWarning = false;
            if (colorChanger != null && colorChanger.Spawned && !DebugSettings.godMode)
            {
                insufficientDyeWarning = colorChanger.Map.resourceCounter.GetCount(ThingDefOf.Dye) < totalDyeRequired;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, Text.LineHeight * 2f);
            Widgets.Label(titleRect, "EMWH_ApparelColorCustomizer".Translate() + " " + pawn.LabelCap);
            Text.Font = GameFont.Small;
            inRect.yMin = titleRect.yMax + 4f;

            float leftPanelWidth = inRect.width * 0.28f;
            float centerPanelWidth = inRect.width * 0.42f;
            float rightPanelWidth = inRect.width * 0.30f - 10f;

            Rect leftRect = new Rect(inRect.x, inRect.y, leftPanelWidth, inRect.height - ButSize.y - 4f);
            Rect centerRect = new Rect(leftRect.xMax + 5f, inRect.y, centerPanelWidth, inRect.height - ButSize.y - 4f);
            Rect rightRect = new Rect(centerRect.xMax + 5f, inRect.y, rightPanelWidth, inRect.height - ButSize.y - 4f);

            DrawPawn(leftRect);
            DrawApparelList(centerRect);
            DrawScrollableColorAdjustments(rightRect);
            DrawBottomButtons(inRect);
        }

        private void UpdateColorState(Color current)
        {
            if (!lastSelectedColor.IndistinguishableFrom(current))
            {
                lastSelectedColor = current;
                hexInput = ColorUtility.ToHtmlStringRGB(current).Insert(0, "#");

                if (colorHistory.Count == 0 || !colorHistory.Last().IndistinguishableFrom(current))
                {
                    colorHistory.Add(current);
                    if (colorHistory.Count > MaxColorHistory)
                        colorHistory.RemoveAt(0);
                }
            }
        }

        private void DrawApparelList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect toggleRect = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 24f);
            Widgets.CheckboxLabeled(toggleRect, "EMWH_ApparelColorCustomizer_ShowHidden".Translate(), ref showHiddenApparel);

            float scrollYStart = toggleRect.yMax + 5f;
            Rect viewRect = new Rect(rect.x, scrollYStart, rect.width - 16f, viewRectHeight);
            Rect scrollRect = new Rect(rect.x, scrollYStart, rect.width, rect.height - (scrollYStart - rect.y));
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);

            float y = scrollYStart;
            foreach (Apparel apparel in pawn.apparel.WornApparel.Where(IsCustomizableApparel))
            {
                Rect apparelRect = new Rect(rect.x, y, viewRect.width, 100f);
                y += apparelRect.height;

                bool isSelected = selectedApparels.Contains(apparel);
                if (isSelected) Widgets.DrawHighlightSelected(apparelRect);
                else if (!apparelColors[apparel].IndistinguishableFrom(originalColors[apparel])) Widgets.DrawHighlight(apparelRect);

                Rect graphicRect = new Rect(apparelRect.x + 5f, apparelRect.y + 5f, 90f, 90f);
                Widgets.ThingIcon(graphicRect, apparel);
                Widgets.Label(new Rect(graphicRect.xMax + 10f, apparelRect.y + 5f,
                    apparelRect.width - graphicRect.width - 15f, 30f), apparel.LabelCap);

                float buttonWidth = (apparelRect.width - graphicRect.width - 30f - 20f) / 3f;
                float buttonX = graphicRect.xMax + 10f;

                Rect selectRect = new Rect(buttonX, apparelRect.y + 40f, buttonWidth, 30f);
                Rect randomRect = new Rect(selectRect.xMax + 5f, apparelRect.y + 40f, buttonWidth, 30f);
                Rect resetRect = new Rect(randomRect.xMax + 5f, apparelRect.y + 40f, buttonWidth, 30f);

                if (Widgets.ButtonText(selectRect, "EMWH_ApparelColorCustomizer_Select".Translate()))
                {
                    if (Event.current.shift)
                    {
                        if (!selectedApparels.Contains(apparel)) selectedApparels.Add(apparel);
                    }
                    else
                    {
                        selectedApparel = apparel;
                        selectedApparels.Clear();
                        selectedApparels.Add(apparel);
                    }
                    rValue = apparelColors[apparel].r;
                    gValue = apparelColors[apparel].g;
                    bValue = apparelColors[apparel].b;
                    SyncInputFields();
                }

                if (Widgets.ButtonText(randomRect, "EMWH_ApparelColorCustomizer_Randomize".Translate()))
                {
                    Color random = new Color(Rand.Value, Rand.Value, Rand.Value);
                    undoStacks[apparel].Push(apparelColors[apparel]);
                    redoStacks[apparel].Clear();
                    apparelColors[apparel] = random;
                    if (selectedApparel == apparel)
                    {
                        rValue = random.r;
                        gValue = random.g;
                        bValue = random.b;
                        SyncInputFields();
                    }
                    PortraitsCache.SetDirty(pawn);
                    CalculateDyeRequirements();
                }

                if (Widgets.ButtonText(resetRect, "EMWH_ApparelColorCustomizer_Reset".Translate()))
                {
                    if (originalColors.TryGetValue(apparel, out var orig))
                    {
                        undoStacks[apparel].Push(apparelColors[apparel]);
                        redoStacks[apparel].Clear();
                        apparelColors[apparel] = orig;
                        if (selectedApparel == apparel)
                        {
                            rValue = orig.r;
                            gValue = orig.g;
                            bValue = orig.b;
                            SyncInputFields();
                        }
                        PortraitsCache.SetDirty(pawn);
                        CalculateDyeRequirements();
                    }
                }

                // Drag-and-drop support
                if (Mouse.IsOver(apparelRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isDragging = true;
                    draggedColor = apparelColors[apparel];
                    Event.current.Use();
                }

                if (Mouse.IsOver(apparelRect) && isDragging && Event.current.type == EventType.MouseUp)
                {
                    apparelColors[apparel] = draggedColor;
                    PortraitsCache.SetDirty(pawn);
                    CalculateDyeRequirements();
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.Layout)
                viewRectHeight = y - scrollYStart;

            isDragging = false;
            Widgets.EndScrollView();
        }

        private void DrawScrollableColorAdjustments(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, 1200f);
            Widgets.BeginScrollView(rect.ContractedBy(10f), ref rightScrollPos, viewRect);

            float y = 0f;
            if (selectedApparel == null || !apparelColors.ContainsKey(selectedApparel))
            {
                Widgets.Label(viewRect, "EMWH_ApparelColorCustomizer_SelectApparelToCustomize".Translate());
                Widgets.EndScrollView();
                return;
            }

            Color current = new Color(rValue, gValue, bValue);
            UpdateColorState(current);

            // Color Preview
            Rect colorDisplayRect = new Rect(viewRect.x, y, viewRect.width, 60f);
            Widgets.DrawBoxSolid(colorDisplayRect, current);
            Widgets.DrawBox(colorDisplayRect);
            Widgets.Label(new Rect(colorDisplayRect.x, colorDisplayRect.yMax + 5f, colorDisplayRect.width, 25f),
                $"R: {rValue * 255:F0} G: {gValue * 255:F0} B: {bValue * 255:F0}");
            y = colorDisplayRect.yMax + 35f;

            // Color Mode Toggle
            Widgets.Label(new Rect(viewRect.x, y, 100f, 24f), "EMWH_ApparelColorCustomizer_ColorMode".Translate());
            if (Widgets.ButtonText(new Rect(viewRect.x + 105f, y, 80f, 24f), pickerMode.ToString()))
            {
                pickerMode = pickerMode == ColorPickerMode.RG_B ? ColorPickerMode.HS_V : ColorPickerMode.RG_B;
                SyncValueSliderFromRGB();
            }
            y += 30f;

            // Picker
            float pickerSize = 128f;
            Rect pickerRect = new Rect(viewRect.x, y, pickerSize, pickerSize);
            Rect sliderRect = new Rect(pickerRect.xMax + 5f, y, 16f, pickerSize);

            hueSatTexture = pickerMode == ColorPickerMode.RG_B
                ? GenerateColorSpectrumTexture((int)pickerSize, valueSlider)
                : GenerateHueSatTexture((int)pickerSize, valueSlider);

            GUI.DrawTexture(pickerRect, hueSatTexture);
            DrawVerticalGradient(sliderRect);

            if (Mouse.IsOver(pickerRect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 local = Event.current.mousePosition - pickerRect.position;
                Color picked = hueSatTexture.GetPixel((int)local.x, (int)local.y);
                Color adjusted = pickerMode == ColorPickerMode.RG_B
                    ? new Color(picked.r, picked.g, valueSlider)
                    : Color.HSVToRGB(picked.r, picked.g, valueSlider);

                rValue = adjusted.r;
                gValue = adjusted.g;
                bValue = adjusted.b;
                Event.current.Use();
                UpdateColorState(adjusted);
            }

            valueSlider = GUI.VerticalSlider(sliderRect, valueSlider, 1f, 0f);
            SyncRGBFromValueSlider();
            y += pickerSize + 10f;

            if (Widgets.ButtonText(new Rect(viewRect.x, y, viewRect.width, 30f), "EMWH_ApparelColorCustomizer_Preview".Translate()))
            {
                TryApplyToSelected(new Color(rValue, gValue, bValue), true);
            }
            y += 35f;

            // RGB Sliders
            y = DrawColorSlider(viewRect, y, "R:", ref rValue, ref rInput);
            y = DrawColorSlider(viewRect, y, "G:", ref gValue, ref gInput);
            y = DrawColorSlider(viewRect, y, "B:", ref bValue, ref bInput);

            if (Widgets.ButtonText(new Rect(viewRect.x, y, viewRect.width, 30f), "EMWH_ApparelColorCustomizer_Preview".Translate()))
            {
                TryApplyToSelected(new Color(rValue, gValue, bValue), true);
            }
            y += 35f;

            // Hex Input
            Rect hexRect = new Rect(viewRect.x, y, 100f, 30f);
            hexInput = Widgets.TextField(hexRect, hexInput);
            if (Widgets.ButtonText(new Rect(hexRect.xMax + 5f, y, 50f, 30f), "EMWH_ApparelColorCustomizer_Copy".Translate()))
            {
                GUIUtility.systemCopyBuffer = hexInput;
            }
            if (Widgets.ButtonText(new Rect(viewRect.x, y + 35f, viewRect.width, 30f), "EMWH_ApparelColorCustomizer_Preview".Translate()) &&
                ColorUtility.TryParseHtmlString(hexInput, out Color hexColor))
            {
                TryApplyToSelected(hexColor, true);
            }

            y += 70f;

            // Undo / Redo Buttons
            Rect undoRect = new Rect(viewRect.x, y, viewRect.width / 2f - 5f, 30f);
            Rect redoRect = new Rect(viewRect.x + viewRect.width / 2f + 5f, y, viewRect.width / 2f - 5f, 30f);

            if (selectedApparel != null && undoStacks.ContainsKey(selectedApparel) && undoStacks[selectedApparel].Count > 0)
            {
                if (Widgets.ButtonText(undoRect, "EMWH_ApparelColorCustomizer_Undo".Translate()))
                {
                    Color prev = undoStacks[selectedApparel].Pop();
                    redoStacks[selectedApparel].Push(new Color(rValue, gValue, bValue));
                    rValue = prev.r;
                    gValue = prev.g;
                    bValue = prev.b;
                    apparelColors[selectedApparel] = prev;
                    SyncInputFields();
                    PortraitsCache.SetDirty(pawn);
                    CalculateDyeRequirements();
                }
            }
            if (selectedApparel != null && redoStacks.ContainsKey(selectedApparel) && redoStacks[selectedApparel].Count > 0)
            {
                if (Widgets.ButtonText(redoRect, "EMWH_ApparelColorCustomizer_Redo".Translate()))
                {
                    Color next = redoStacks[selectedApparel].Pop();
                    undoStacks[selectedApparel].Push(new Color(rValue, gValue, bValue));
                    rValue = next.r;
                    gValue = next.g;
                    bValue = next.b;
                    apparelColors[selectedApparel] = next;
                    SyncInputFields();
                    PortraitsCache.SetDirty(pawn);
                    CalculateDyeRequirements();
                }
            }

            y += 40f;
            DrawDyeRequirement(new Rect(viewRect.x, y, viewRect.width, 50f));
            Widgets.EndScrollView();
        }

        private void TryApplyToSelected(Color newColor, bool isPreview = false)
        {
            // If the apparel is not destroyed, proceed
            if (!selectedApparel.DestroyedOrNull())
            {
                if (!newColor.IndistinguishableFrom(originalColors[selectedApparel]))
                {
                    // Preview function - only show preview color but don't change the apparel yet
                    if (isPreview)
                    {
                        apparelColors[selectedApparel] = newColor;
                        rValue = newColor.r;
                        gValue = newColor.g;
                        bValue = newColor.b;
                        hexInput = ColorUtility.ToHtmlStringRGB(newColor).Insert(0, "#");
                        UpdateColorState(newColor); // Update color history for preview

                        PortraitsCache.SetDirty(pawn);
                        CalculateDyeRequirements(); // Update dye requirements based on preview
                    }
                    else // This is an actual color application
                    {
                        // Check if enough dye is available
                        if (!DebugSettings.godMode && colorChanger != null && colorChanger.Spawned)
                        {
                            int dyeAvailable = colorChanger.Map.resourceCounter.GetCount(ThingDefOf.Dye);
                            if (dyeAvailable < DyePerApparelChange)
                            {
                                Messages.Message("EMWH_ApparelColorCustomizer_NotEnoughDye".Translate(), MessageTypeDefOf.RejectInput);
                                return;
                            }
                            Thing dye = colorChanger.Map.listerThings.ThingsOfDef(ThingDefOf.Dye).FirstOrDefault();
                            dye?.SplitOff(1)?.Destroy();
                        }

                        // Apply color change and store in undo stack
                        undoStacks[selectedApparel].Push(apparelColors[selectedApparel]);
                        redoStacks[selectedApparel].Clear();

                        apparelColors[selectedApparel] = newColor;
                        rValue = newColor.r;
                        gValue = newColor.g;
                        bValue = newColor.b;
                        hexInput = ColorUtility.ToHtmlStringRGB(newColor).Insert(0, "#");

                        selectedApparel.SetColor(newColor);
                        selectedApparel.Notify_ColorChanged();
                        originalColors[selectedApparel] = newColor;

                        UpdateColorState(newColor);
                        PortraitsCache.SetDirty(pawn);
                        pawn.Drawer.renderer.SetAllGraphicsDirty();
                        pawn.mindState.Notify_OutfitChanged();
                        CalculateDyeRequirements(); // Update dye requirements after applying the color
                    }
                }
            }
        }

        private void SyncValueSliderFromRGB()
        {
            if (pickerMode == ColorPickerMode.RG_B)
            {
                valueSlider = bValue;
            }
            else if (pickerMode == ColorPickerMode.HS_V)
            {
                Color.RGBToHSV(new Color(rValue, gValue, bValue), out float h, out float s, out float v);
                valueSlider = v;
            }
        }

        private void SyncRGBFromValueSlider()
        {
            if (pickerMode == ColorPickerMode.RG_B)
            {
                // Only update the blue value when adjusting the slider
                //bValue = valueSlider;
            }
            else if (pickerMode == ColorPickerMode.HS_V)
            {
                // In HSV mode, we calculate the new RGB values based on the slider
                Color.RGBToHSV(new Color(rValue, gValue, bValue), out float h, out float s, out _);
                Color newColor = Color.HSVToRGB(h, s, valueSlider);
                rValue = newColor.r;
                gValue = newColor.g;
                bValue = newColor.b;
            }
        }

        private Texture2D GenerateColorSpectrumTexture(int size, float bComponent)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float r = (float)x / size;
                    float g = (float)y / size;
                    tex.SetPixel(x, y, new Color(r, g, bComponent));
                }
            }
            tex.Apply();
            return tex;
        }

        private void DrawVerticalGradient(Rect rect)
        {
            Texture2D gradient = new Texture2D(1, 128);
            for (int i = 0; i < 128; i++)
            {
                Color color = Color.Lerp(Color.black, Color.white, i / 127f);
                gradient.SetPixel(0, i, color);
            }
            gradient.Apply();
            GUI.DrawTexture(rect, gradient);
        }

        private Texture2D GenerateHueSatTexture(int size, float v)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float h = (float)x / size;
                    float s = (float)y / size;
                    tex.SetPixel(x, y, Color.HSVToRGB(h, s, v));
                }
            }
            tex.Apply();
            return tex;
        }

        private float DrawColorSlider(Rect container, float y, string label, ref float value, ref string inputValue)
        {
            Rect labelRect = new Rect(container.x, y, 30f, 30f);
            Rect sliderRect = new Rect(labelRect.xMax + 5f, y, 150f, 30f);
            Rect inputRect = new Rect(sliderRect.xMax + 5f, y, 40f, 30f);

            Widgets.Label(labelRect, label);
            float newValue = Widgets.HorizontalSlider(sliderRect, value, 0f, 1f);
            int sliderIntValue = Mathf.RoundToInt(newValue * 255);
            inputValue = Widgets.TextField(inputRect, sliderIntValue.ToString());

            if (int.TryParse(inputValue, out int parsed))
                newValue = Mathf.Clamp(parsed / 255f, 0f, 1f);

            value = newValue;
            return y + 35f;
        }

        private void SyncInputFields()
        {
            rInput = Mathf.RoundToInt(rValue * 255).ToString();
            gInput = Mathf.RoundToInt(gValue * 255).ToString();
            bInput = Mathf.RoundToInt(bValue * 255).ToString();
            hexInput = ColorUtility.ToHtmlStringRGB(new Color(rValue, gValue, bValue)).Insert(0, "#");
        }

        private void DrawBottomButtons(Rect inRect)
        {
            float buttonSpacing = 10f;
            float buttonWidth = (inRect.width - (buttonSpacing * 4)) / 4f;
            float buttonY = inRect.yMax - ButSize.y;

            Rect cancelRect = new Rect(inRect.x + buttonSpacing, buttonY, buttonWidth, ButSize.y);
            Rect resetRect = new Rect(cancelRect.xMax + buttonSpacing, buttonY, buttonWidth, ButSize.y);
            Rect randomizeRect = new Rect(resetRect.xMax + buttonSpacing, buttonY, buttonWidth, ButSize.y);
            Rect applyRect = new Rect(randomizeRect.xMax + buttonSpacing, buttonY, buttonWidth, ButSize.y);

            if (Widgets.ButtonText(cancelRect, "EMWH_ApparelColorCustomizer_ButtonCancel".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(resetRect, "EMWH_ApparelColorCustomizer_ButtonResetAll".Translate()))
            {
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    if (originalColors.ContainsKey(apparel))
                        apparelColors[apparel] = originalColors[apparel];
                }
                selectedApparel = null;
                CalculateDyeRequirements();
            }

            if (Widgets.ButtonText(randomizeRect, "EMWH_ApparelColorCustomizer_ButtonRandomizeAll".Translate()))
            {
                foreach (Apparel apparel in apparelColors.Keys.ToList())
                {
                    apparelColors[apparel] = new Color(Rand.Value, Rand.Value, Rand.Value);
                }

                if (selectedApparel != null && apparelColors.ContainsKey(selectedApparel))
                {
                    Color selectedColor = apparelColors[selectedApparel];
                    rValue = selectedColor.r;
                    gValue = selectedColor.g;
                    bValue = selectedColor.b;
                }

                CalculateDyeRequirements();
            }

            bool canApply = !insufficientDyeWarning || DebugSettings.godMode;
            GUI.color = canApply ? Color.white : Color.gray;
            if (Widgets.ButtonText(applyRect, "EMWH_ApparelColorCustomizer_ButtonApplyAll".Translate(), active: canApply) && canApply)
            {
                ApplyColors();
                Close();
            }
            GUI.color = Color.white;
        }

        private void ApplyColors()
        {
            if (!DebugSettings.godMode && colorChanger != null && colorChanger.Spawned)
            {
                int available = colorChanger.Map.resourceCounter.GetCount(ThingDefOf.Dye);
                if (available < totalDyeRequired)
                {
                    Messages.Message("EMWH_ApparelColorCustomizer_NotEnoughDye".Translate(), MessageTypeDefOf.RejectInput);
                    return;
                }
            }

            foreach (var kvp in apparelColors)
            {
                Apparel apparel = kvp.Key;
                Color color = kvp.Value;

                if (!color.IndistinguishableFrom(originalColors[apparel]))
                {
                    apparel.SetColor(color);
                    apparel.Notify_ColorChanged();
                }
            }

            pawn.Drawer.renderer.SetAllGraphicsDirty();
            pawn.mindState.Notify_OutfitChanged();
        }

        private void DrawDyeRequirement(Rect rect)
        {
            Widgets.DrawHighlightIfMouseover(rect);

            Widgets.ThingIcon(new Rect(rect.x, rect.y, Text.LineHeight, Text.LineHeight), ThingDefOf.Dye);
            Text.Anchor = TextAnchor.MiddleLeft;
            string dyeText = "EMWH_ApparelColorCustomizer_DyeNeeded".Translate(totalDyeRequired);
            Widgets.Label(new Rect(rect.x + Text.LineHeight + 4f, rect.y, rect.width - Text.LineHeight - 4f, Text.LineHeight), dyeText);
            Text.Anchor = TextAnchor.UpperLeft;

            if (insufficientDyeWarning)
            {
                Rect warningRect = new Rect(rect.x, rect.y + Text.LineHeight, rect.width, Text.LineHeight);
                GUI.color = ColorLibrary.RedReadable;
                Widgets.Label(warningRect, "EMWH_ApparelColorCustomizer_NotEnoughDye".Translate());
                GUI.color = Color.white;
            }

            TooltipHandler.TipRegion(rect, "EMWH_ApparelColorCustomizer_DyeRequirementExplanation".Translate());
        }

        private void DrawPawn(Rect rect)
        {
            Rect toggleRect = rect;
            toggleRect.yMin = rect.yMax - Text.LineHeight * 2f;
            Widgets.CheckboxLabeled(new Rect(toggleRect.x, toggleRect.y, toggleRect.width, toggleRect.height / 2f),
                "EMWH_ApparelColorCustomizer_ShowHeadgear".Translate(), ref showHeadgear);
            Widgets.CheckboxLabeled(new Rect(toggleRect.x, toggleRect.y + toggleRect.height / 2f, toggleRect.width, toggleRect.height / 2f),
                "EMWH_ApparelColorCustomizer_ShowApparel".Translate(), ref showClothes);

            rect.yMax = toggleRect.yMin - 4f;

            Widgets.BeginGroup(rect);
            for (int i = 0; i < 3; i++)
            {
                Rect portraitRect = new Rect(0f, rect.height / 3f * i, rect.width, rect.height / 3f).ContractedBy(4f);
                PortraitsCache.PortraitsCacheUpdate();
                RenderTexture portrait = PortraitsCache.Get(
                    pawn,
                    new Vector2(portraitRect.width, portraitRect.height),
                    new Rot4(2 - i),
                    PortraitOffset,
                    PortraitZoom,
                    renderHeadgear: showHeadgear,
                    renderClothes: showClothes,
                    overrideApparelColors: apparelColors);

                GUI.DrawTexture(portraitRect, portrait);
            }
            Widgets.EndGroup();
        }

        private void DrawApparelGraphic(Apparel apparel, Rect rect, Color color)
        {
            try
            {
                Widgets.DrawBoxSolid(rect, color);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to draw apparel graphic: {ex}");
                Widgets.DrawBoxSolid(rect, color);
            }

            Widgets.DrawHighlightIfMouseover(rect);

            int tooltipId = apparel.GetHashCode() + rect.GetHashCode();
            TooltipHandler.TipRegion(rect, new TipSignal(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(apparel.LabelCap);
                sb.AppendLine("EMWH_ApparelColorCustomizer_TooltipCurrentColor".Translate((color.r * 255).ToString("F0"), (color.g * 255).ToString("F0"), (color.b * 255).ToString("F0")));

                if (originalColors.ContainsKey(apparel))
                {
                    Color original = originalColors[apparel];
                    sb.AppendLine("EMWH_ApparelColorCustomizer_TooltipOriginalColor".Translate((original.r * 255).ToString("F0"), (original.g * 255).ToString("F0"), (original.b * 255).ToString("F0")));
                }

                return sb.ToString();
            }, tooltipId));

            if (Widgets.ButtonInvisible(rect))
            {
                selectedApparel = apparel;
                rValue = color.r;
                gValue = color.g;
                bValue = color.b;
                PlayClickSound();
            }

            Widgets.DrawBox(rect);
        }

        // Helper method for sounds
        private void PlayClickSound()
        {
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

        public override void PostClose()
        {
            base.PostClose();
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}