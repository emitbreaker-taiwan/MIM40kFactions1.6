using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MIM40kFactions
{
    public class CompAbilityEffect_ThunderBolt : CompAbilityEffect
    {
        private Mesh boltMesh;

        public new CompProperties_AbilityThunderBolt Props => (CompProperties_AbilityThunderBolt)props;

        private Verb ShootVerb => parent.verb;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (!Utility_TargetValidator.IsValidTargetForAbility(target.Thing, parent.pawn, Props.targetHostilesOnly, Props.targetNeutralBuildings, ShootVerb.verbProps.targetParams.canTargetLocations))
            {
                return;
            }
            LocalTargetInfo currentTarget = ShootVerb.CurrentTarget;
            IntVec3 currentTargetCell = currentTarget.Cell;
            Pawn caster = parent.pawn;
            DamageDef damageDef = Props.damageDef;
            if (damageDef == null)
            {
                damageDef = DamageDefOf.Flame;
            }
            int burstShotCount = Props.burstShotCount;
            int damageAmount = Props.damageAmount;
            if (Props.randomBurst)
            {
                burstShotCount = Rand.RangeInclusive(1, Props.burstShotCount);
            }
            if (Props.sustainedHits > 0)
            {
                int rand = Rand.RangeInclusive(1, 6);
                if (rand == 6)
                    burstShotCount = burstShotCount + Props.sustainedHits;
            }
            if (burstShotCount > 0)
            {
                for (int i = 0; i < burstShotCount; i++)
                {
                    if (Props.useKeyword && Utility_PawnValidationManager.KeywordValidator(target.Pawn, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult, Props.isHereticAstartes))
                    {
                        int rand = Rand.RangeInclusive(1, 6);
                        if (Props.keywordBonus > 0 && Props.keywordBonus <= rand)
                        {
                            damageAmount = Mathf.RoundToInt(damageAmount * 1.5f);
                        }
                    }
                    if (Props.isHazardous && Utility_WeaponStatChanger.IsHazardousCalculator())
                    {
                        currentTargetCell = caster.Position;
                        MoteMaker.ThrowText(caster.DrawPos, caster.Map, "EMWH_WeaponAbilities_Hazardous".Translate(), Color.red);
                    }
                    if (Time.time > Props.millisecbetwenBurst)
                        DoStrike(currentTargetCell, caster.Map, damageDef, damageAmount, ref boltMesh);
                    currentTargetCell.x = currentTargetCell.x + Rand.RangeInclusive(1, 6);
                    currentTargetCell.y = currentTargetCell.y + Rand.RangeInclusive(1, 6);
                    caster.records.Increment(RecordDefOf.ShotsFired);
                }
            }
            else
            {
                if (Props.useKeyword && Utility_PawnValidationManager.KeywordValidator(target.Pawn, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult, Props.isHereticAstartes))
                {
                    int rand = Rand.RangeInclusive(1, 6);
                    if (Props.keywordBonus > 0 && Props.keywordBonus <= rand)
                    {
                        damageAmount = Mathf.RoundToInt(damageAmount * 1.5f);
                    }
                }
                if (Utility_WeaponStatChanger.IsHazardousCalculator())
                    currentTargetCell = caster.Position;
                DoStrike(currentTargetCell, caster.Map, damageDef, damageAmount, ref boltMesh);
                caster.records.Increment(RecordDefOf.ShotsFired);
            }
        }

        private static void DoStrike(IntVec3 strikeLoc, Map map, DamageDef damageDef, int damageAmount, ref Mesh boltMesh)
        {
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
            if (!strikeLoc.IsValid)
            {
                strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
            }

            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (!strikeLoc.Fogged(map))
            {
                GenExplosion.DoExplosion(strikeLoc, map, 1.9f, damageDef, null, damageAmount);
                Vector3 loc = strikeLoc.ToVector3Shifted();
                for (int i = 0; i < 4; i++)
                {
                    FleckMaker.ThrowSmoke(loc, map, 1.5f);
                    FleckMaker.ThrowMicroSparks(loc, map);
                    FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                }
            }

            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (!target.IsValid || !target.HasThing)
                return false;

            return Utility_TargetValidator.IsValidTargetForAbility(target.Thing, parent.pawn, Props.targetHostilesOnly, Props.targetNeutralBuildings, ShootVerb.verbProps.targetParams.canTargetLocations);
        }
    }
}