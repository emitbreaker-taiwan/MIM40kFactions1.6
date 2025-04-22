using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class MIM40kFactionsGasProperties : GasProperties
    {
        public HediffDef hediffDef;
        public StatDef exposureStatFactor = null;
        public float severityAdjustment = 0.07f;
        public int mtbCheckDuration = 60;
    }
}