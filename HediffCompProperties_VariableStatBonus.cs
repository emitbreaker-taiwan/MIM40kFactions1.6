using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_VariableStatBonus : HediffCompProperties
    {
        public bool targetStatOffstes = false;
        public bool targetStatFactors = false;

        public StatDef multiplierStatDef;
        public RecordDef multiplierRecordDef;

        public StatDefModifierSet statDefModifierSet;
        public List<StatDefModifierSet> statDefModifierSets;

        public int checkInterval = 600;

        public HediffCompProperties_VariableStatBonus() => compClass = typeof(HediffComp_VariableStatBonus);
    }

    public class StatDefModifierSet
    {
        public StatDef targetOffsetStatDef;
        public List<StatDef> targetOffsetStatDefs;
        public bool isNegativeOffset = false;
        public float multiplierOffsets = 10f;

        public StatDef targetFactorStatDef;
        public List<StatDef> targetFactorStatDefs;
        public bool isNegativeFactor = false;
        public float multiplierFactors = 100f;
    }
}