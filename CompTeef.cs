using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class CompTeef : CompHasGatherableBodyResource
    {
        protected override int GatherResourcesIntervalDays => Props.smashTeefIntervalDays;

        protected override int ResourceAmount => Props.teefAmount;

        protected override ThingDef ResourceDef => Props.teefDef;

        protected override string SaveKey => Props.saveKey;

        public CompProperties_Teef Props => (CompProperties_Teef)props;

        protected override bool Active
        {
            get
            {
                if (!base.Active)
                {
                    return false;
                }

                Pawn pawn = parent as Pawn;
                if (pawn != null)
                {
                    if (!Utility_PawnValidationManager.IsPawnDeadValidator(pawn))
                    {
                        return false;
                    }
                    if (!pawn.RaceProps.Humanlike && !pawn.ageTracker.CurLifeStage.shearable)
                    {
                        return false;
                    }
                    else if (pawn.RaceProps.Humanlike && !Props.humanlikeCanProduce)
                    {
                        return false;
                    }
                }

                if (ModsConfig.AnomalyActive && pawn.IsShambler && !Props.shamblerCanProduce)
                {
                    return false;
                }

                return true;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!Active)
            {
                return null;
            }

            return Props.saveKey.Translate() + ": " + base.Fullness.ToStringPercent();
        }
    }
}
