using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace MIM40kFactions
{
    public class CompAbilityEffect_LivingLightning : CompAbilityEffect
    {
        private HashSet<Faction> affectedFactionCache = new HashSet<Faction>();

        public new CompProperties_AbilityLivingLightning Props => (CompProperties_AbilityLivingLightning)props;

        private Verb ShootVerb => parent.verb;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = parent.pawn.Map;
            Thing conditionCauser = GenSpawn.Spawn(ThingDefOf.Flashstorm, target.Cell, parent.pawn.Map);
            GameCondition_LivingLightning gameCondition_LivingLightning = (GameCondition_LivingLightning)GameConditionMaker.MakeCondition(MIM40kFactionsGameConditionDefOf.EMWH_LivingLightning);
            gameCondition_LivingLightning.centerLocation = target.Cell.ToIntVec2;
            gameCondition_LivingLightning.areaRadiusOverride = new IntRange(Mathf.RoundToInt(parent.def.EffectRadius), Mathf.RoundToInt(parent.def.EffectRadius));
            gameCondition_LivingLightning.Duration = Mathf.RoundToInt(parent.def.EffectDuration(parent.pawn).SecondsToTicks());
            gameCondition_LivingLightning.suppressEndMessage = true;
            gameCondition_LivingLightning.initialStrikeDelay = new IntRange(Props.initialStrikeDelayMin, Props.initialStrikeDelayMax);
            gameCondition_LivingLightning.conditionCauser = conditionCauser;
            gameCondition_LivingLightning.ambientSound = true;
            gameCondition_LivingLightning.caster = parent.pawn;
            map.gameConditionManager.RegisterCondition(gameCondition_LivingLightning);
            ApplyGoodwillImpact(target, gameCondition_LivingLightning.AreaRadius);
        }

        private void ApplyGoodwillImpact(LocalTargetInfo target, int radius)
        {
            if (parent.pawn.Faction != Faction.OfPlayer)
            {
                return;
            }

            affectedFactionCache.Clear();
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, radius, useCenter: true))
            {
                Pawn p;
                if ((p = item as Pawn) != null && item.Faction != null && item.Faction != parent.pawn.Faction && !item.Faction.HostileTo(parent.pawn.Faction) && !affectedFactionCache.Contains(item.Faction) && (base.Props.applyGoodwillImpactToLodgers || !p.IsQuestLodger()))
                {
                    affectedFactionCache.Add(item.Faction);
                    Faction.OfPlayer.TryAffectGoodwillWith(item.Faction, base.Props.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
                }
            }

            affectedFactionCache.Clear();
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Cell.Roofed(parent.pawn.Map))
            {
                if (throwMessages)
                {
                    Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityRoofed".Translate(), target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, historical: false);
                }

                return false;
            }

            return true;
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            TargetingParameters targetParm = ShootVerb.verbProps.targetParams;
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            if (targetParm.canTargetPawns && target.Pawn == null)
                return false;
            if (targetParm.canTargetMechs && target.Pawn.RaceProps.IsMechanoid)
                return true;
            if (targetParm.canTargetHumans && target.Pawn.RaceProps.Humanlike)
                return true;
            if (targetParm.canTargetAnimals && target.Pawn.IsAnimal)
                return true;
            if (targetParm.canTargetLocations)
                return true;
            else return false;
        }
    }
}