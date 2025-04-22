using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_StunbyKeyword : CompAbilityEffect_WithDuration
    {
        public new CompProperties_AbilityGiveHediffbyKeyword Props => (CompProperties_AbilityGiveHediffbyKeyword)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.HasThing)
            {
                base.Apply(target, dest);
                if (Props.isHazardous && Utility_WeaponStatChanger.IsHazardousCalculator())
                {
                    Utility_WeaponStatChanger.ApplySurgicalDamage(parent.pawn, DamageDefOf.Bomb, 30 / 2f, null, parent.pawn);
                    return;
                }

                Pawn pawn = target.Thing as Pawn;

                if (!FactionValidator(pawn))
                {
                    return;
                }
                pawn?.stances.stunner.StunFor(GetDurationSeconds(pawn).SecondsToTicks(), parent.pawn, addBattleLog: false);
            }
        }

        private bool FactionValidator(Pawn targ)
        {
            Faction casterFaction = parent.pawn.Faction;
            Faction targetFaction = targ.Faction;
            if (Props.onlyTargetNotinSameFactions && casterFaction != targetFaction)
                return true;

            if (Props.onlyTargetHostileFactions && targ.HomeFaction.HostileTo(casterFaction))
                return true;

            if (Props.onlyPawnsInSameFaction && casterFaction == targetFaction)
                return true;

            if (Props.onlyTargetNonPlayerFactions && targetFaction != Faction.OfPlayer)
                return true;
            return false;
        }

        private bool CalculateHazardous()
        {
            int isHazardous;
            isHazardous = Rand.RangeInclusive(1, 6);
            if (Props.isHazardous && isHazardous == 1)
            {
                return true;
            }
            return false;
        }
    }
}