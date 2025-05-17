using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    [StaticConstructorOnStartup]
    public class MechPowerCellBuildingGizmo : Gizmo
    {
        private CompMechPowerCellBuilding powerCell;

        private const float Width = 160f;

        private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(12, 45, 45, byte.MaxValue));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Empty);

        private const int BarThresholdTickIntervals = 2500;

        public MechPowerCellBuildingGizmo(CompMechPowerCellBuilding carrier)
        {
            powerCell = carrier;
        }

        public override float GetWidth(float maxWidth)
        {
            return 160f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(10f);
            Widgets.DrawWindowBackground(rect);
            string text = (powerCell.Props.labelOverride.NullOrEmpty() ? ((string)"MechPowerCell".Translate()) : powerCell.Props.labelOverride);
            Rect rect3 = new Rect(rect2.x, rect2.y, rect2.width, Text.CalcHeight(text, rect2.width) + 8f);
            Text.Font = GameFont.Small;
            Widgets.Label(rect3, text);
            Rect barRect = new Rect(rect2.x, rect3.yMax, rect2.width, rect2.height - rect3.height);
            Widgets.FillableBar(barRect, powerCell.PercentFull, BarTex, EmptyBarTex, doBorder: true);
            for (int i = 2500; i < powerCell.Props.totalPowerTicks; i += 2500)
            {
                DoBarThreshold((float)i / (float)powerCell.Props.totalPowerTicks);
            }

            Text.Anchor = (TextAnchor)4;
            Widgets.Label(barRect, Mathf.CeilToInt((float)powerCell.PowerTicksLeft / 2500f) + (string)"LetterHour".Translate());
            Text.Anchor = (TextAnchor)0;
            string tooltip;
            if (!powerCell.Props.tooltipOverride.NullOrEmpty())
            {
                tooltip = powerCell.Props.tooltipOverride;
            }
            else
            {
                tooltip = "MechPowerCellTip".Translate();
            }

            TooltipHandler.TipRegion(rect2, () => tooltip, Gen.HashCombineInt(powerCell.GetHashCode(), 34242369));
            return new GizmoResult(GizmoState.Clear);
            void DoBarThreshold(float percent)
            {
                Rect rect4 = default(Rect);
                rect4.x = barRect.x + 3f + (barRect.width - 8f) * percent;
                rect4.y = barRect.y + barRect.height - 9f;
                rect4.width = 2f;
                rect4.height = 6f;
                GUI.DrawTexture(rect4, (Texture)BaseContent.BlackTex);
            }
        }
    }
}
