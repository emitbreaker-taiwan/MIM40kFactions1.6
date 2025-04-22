using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using System.IO;

namespace MIM40kFactions
{
    public class HediffComp_VariableStatBonus : HediffComp
    {
        private float multiplierfromStatDef
        {
            get
            {
                float num = (float)parent.pawn.GetStatValue(Props.multiplierStatDef);
                return num;
            }
        }

        private float multiplierfromRecordDef
        {
            get
            {
                float num = (float)parent.pawn.records.GetValue(Props.multiplierRecordDef);
                return num;
            }
        }

        private int checkInterval;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.statDefModifierSet != null)
            {
                DoInner(Props.statDefModifierSet, Props.targetStatOffstes, Props.targetStatFactors);
            }
            if (Props.statDefModifierSets != null)
            {
                foreach (StatDefModifierSet statDefModifierSet in Props.statDefModifierSets)
                {
                    DoInner(statDefModifierSet, Props.targetStatOffstes, Props.targetStatFactors);
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Utility_PawnValidator.IsPawnDeadValidator(parent.pawn))
            {
                return;
            }

            checkInterval = 600;
            
            if (Props.checkInterval > 0)
            {
                checkInterval = Props.checkInterval;
            }
            
            if (parent.pawn.IsHashIntervalTick(checkInterval))
            {
                if (Props.statDefModifierSet != null)
                {
                    DoInner(Props.statDefModifierSet, Props.targetStatOffstes, Props.targetStatFactors);
                }
                if (Props.statDefModifierSets != null)
                {
                    foreach (StatDefModifierSet statDefModifierSet in Props.statDefModifierSets)
                    {
                        DoInner(statDefModifierSet, Props.targetStatOffstes, Props.targetStatFactors);
                    }
                }
            }
        }

        private void DoInner(StatDefModifierSet statDefModifierSet, bool targetStatOffstes, bool targetStatFactors)
        {
            if (statDefModifierSet == null)
            {
                return;
            }

            if (targetStatOffstes)
            {
                if (statDefModifierSet.targetOffsetStatDef != null)
                {
                    OffsetStatValueAdjustment(statDefModifierSet.targetOffsetStatDef, statDefModifierSet.multiplierOffsets, statDefModifierSet.isNegativeOffset);
                }
                if (statDefModifierSet.targetOffsetStatDefs != null)
                {
                    foreach (StatDef targetStatDef in statDefModifierSet.targetOffsetStatDefs)
                    {
                        OffsetStatValueAdjustment(targetStatDef, statDefModifierSet.multiplierOffsets, statDefModifierSet.isNegativeOffset);
                    }
                }
            }

            if (targetStatFactors)
            {
                if (statDefModifierSet.targetFactorStatDef != null)
                {
                    FactorStatValueAdjustment(statDefModifierSet.targetFactorStatDef, statDefModifierSet.multiplierFactors, statDefModifierSet.isNegativeFactor);
                }
                if (statDefModifierSet.targetFactorStatDefs != null)
                {
                    foreach (StatDef targetStatDef in statDefModifierSet.targetFactorStatDefs)
                    {
                        FactorStatValueAdjustment(targetStatDef, statDefModifierSet.multiplierFactors, statDefModifierSet.isNegativeFactor);
                    }
                }
            }

        }

        private float SetMultiplierValue(float multiplier, bool isNegative = false)
        {
            float value = 0f;

            if (Props.multiplierStatDef != null)
            {
                value = multiplierfromStatDef / multiplier;
            }
            if (Props.multiplierRecordDef != null)
            {
                value = multiplierfromRecordDef / multiplier;
            }

            if (isNegative)
            {
                value *= -1;
            }

            return value;
        }

        private void OffsetStatValueAdjustment(StatDef targetStatDef, float multiplierOffsets = 10f, bool isNegativeOffset = false)
        {
            if (multiplierOffsets < 0)
            {
                multiplierOffsets = 10f;
            }

            if (parent.CurStage.statOffsets.Count < 0 || targetStatDef == null)
            {
                return;
            }

            foreach (StatModifier s in parent.CurStage.statOffsets)
            {
                if (s.stat == targetStatDef)
                {
                    s.value = SetMultiplierValue(multiplierOffsets, isNegativeOffset);
                }
            }
        }

        private void FactorStatValueAdjustment(StatDef targetStatDef, float multiplierFactors = 100f, bool isNegativeFactor = false)
        {
            if (multiplierFactors < 0)
            {
                multiplierFactors = 100f;
            }

            if (parent.CurStage.statFactors.Count < 0 || targetStatDef == null)
            {
                return;
            }

            foreach (StatModifier s in parent.CurStage.statFactors)
            {
                if (s.stat == targetStatDef)
                {
                    if (isNegativeFactor)
                    {
                        if (1f + SetMultiplierValue(multiplierFactors, isNegativeFactor) > 0.01f)
                        {
                            s.value = 1f + SetMultiplierValue(multiplierFactors, isNegativeFactor);
                        }
                        else
                        {
                            s.value = 0.01f;
                        }
                    }
                    else
                    {
                        s.value = SetMultiplierValue(multiplierFactors, isNegativeFactor);
                    }
                }
            }
        }

        public HediffCompProperties_VariableStatBonus Props => (HediffCompProperties_VariableStatBonus)props;
    }
}