using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Gene_HediffGiver : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();

            GeneUtilityExtension modExtension = def.GetModExtension<GeneUtilityExtension>();

            if (modExtension == null)
            {
                return;
            }

            if (modExtension.startingHediffDefs != null)
            {
                if (!ExlcudingHediffValidator(pawn, modExtension.excludeifHediffs))
                {
                    return;
                }
                foreach (StartingHediff startingHediff in modExtension.startingHediffDefs)
                {
                    AddStartingHediffs(pawn, startingHediff);
                }
            }
            if (modExtension.randomHediffDefs != null)
            {
                if (!ExlcudingHediffValidator(pawn, modExtension.excludeifHediffs))
                {
                    return;
                }
                AddRandomHediffs(pawn, modExtension.randomHediffDefs, modExtension.severityRandomHediffDefs);
            }
            if (modExtension.hediffDefstoBodyParts != null)
            {
                if (!ExlcudingHediffValidator(pawn, modExtension.excludeifHediffs))
                {
                    return;
                }
                foreach (HedifftoBodyParts hediffDefstoBodyPart in modExtension.hediffDefstoBodyParts)
                {
                    AddHedifftoBodyParts(pawn, hediffDefstoBodyPart);
                }
            }
        }

        private static bool ExlcudingHediffValidator(Pawn pawn, List<HediffDef> excludeifHediffs = null)
        {
            if (excludeifHediffs != null)
            {
                foreach (HediffDef hediffdef in excludeifHediffs)
                {
                    if (pawn.health.hediffSet.HasHediff(hediffdef))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void AddStartingHediffs(Pawn pawn, StartingHediff startingHediff)
        {
            if (!startingHediff.HasHediff(pawn))
            {
                Hediff hediff = pawn.health.AddHediff(startingHediff.def);
                if (startingHediff.severity.HasValue)
                {
                    if (hediff.def.hediffClass == typeof(Hediff_Level)) 
                    {
                        Hediff_Level hediffLevel = hediff as Hediff_Level;
                        hediffLevel.ChangeLevel((int)startingHediff.severity.Value);
                    }
                    else
                    {
                        hediff.Severity = startingHediff.severity.Value;
                    }
                }
                if (startingHediff.durationTicksRange.HasValue)
                {
                    hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = startingHediff.durationTicksRange.Value.RandomInRange;
                }
            }
        }

        private static void AddRandomHediffs(Pawn pawn, List<HediffDef> randomHediffDefs, float? severityRandomHediffDefs)
        {
            int rand = Rand.Range(0, randomHediffDefs.Count - 1);
            Hediff hediff = pawn.health.AddHediff(randomHediffDefs[rand]);
            if (severityRandomHediffDefs.HasValue)
            {
                if (hediff.def.hediffClass == typeof(Hediff_Level))
                {
                    Hediff_Level hediffLevel = hediff as Hediff_Level;
                    hediffLevel.ChangeLevel((int)severityRandomHediffDefs.Value);
                }
                else
                {
                    hediff.Severity = severityRandomHediffDefs.Value;
                }
            }
        }

        private static void AddHedifftoBodyParts(Pawn pawn, HedifftoBodyParts hediffDefstoBodyPart)
        {
            int index = 0;
            foreach (BodyPartDef part in hediffDefstoBodyPart.parts)
            {
                if (!pawn.RaceProps.body.GetPartsWithDef(part).EnumerableNullOrEmpty<BodyPartRecord>() && index <= pawn.RaceProps.body.GetPartsWithDef(part).Count)
                {
                    Hediff hediff = pawn.health.AddHediff(hediffDefstoBodyPart.def, pawn.RaceProps.body.GetPartsWithDef(part).ToArray()[index]);
                    if (hediffDefstoBodyPart.severity.HasValue)
                    {
                        hediff.Severity = hediffDefstoBodyPart.severity.Value;
                    }
                    if (hediffDefstoBodyPart.durationTicksRange.HasValue)
                    {
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = hediffDefstoBodyPart.durationTicksRange.Value.RandomInRange;
                    }
                    ++index;
                }
            }
        }
    }
}
