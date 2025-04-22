using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_SpawnThing : CompAbilityEffect
    {
        public CompProperties_AbilitySpawnThing p => (CompProperties_AbilitySpawnThing)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (p.amount > 0)
            {
                for (int i = 0; i < p.amount; i++)
                {
                    SpawnThing(p.thingDef, target, parent.pawn.Map);
                    if (p.sendSkipSignal)
                    {
                        return;
                    }
                    CompAbilityEffect_Teleport.SendSkipUsedSignal(target, parent.pawn);
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!target.Cell.Filled(parent.pawn.Map) || target.Cell.GetEdifice(parent.pawn.Map) == null)
            {
                return true;
            }
            if (p.wipeMode.HasValue && (!target.Cell.Filled(parent.pawn.Map) || target.Cell.GetEdifice(parent.pawn.Map) == null))
            {
                return true;
            }
            if (throwMessages)
            {
                Messages.Message((string)("CannotUseAbility".Translate((NamedArgument)this.parent.def.label) + ": " + "AbilityOccupiedCells".Translate()), (LookTargets)target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
            }
            return false;
        }

        public virtual void SpawnThing(ThingDef thingDef, LocalTargetInfo target, Map map)
        {
            Faction targetFaction = parent.pawn.Faction;

            if (p.forcedFaction != null)
            {
                targetFaction.def = p.forcedFaction;
            }

            ThingDef stuff = GenStuff.RandomStuffFor(thingDef);

            if (p.stuffDef != null)
            {
                stuff = p.stuffDef;
            }

            Thing thing = ThingMaker.MakeThing(thingDef, stuff);

            if (p.thingStyleDef != null)
            {
                thing.StyleDef = p.thingStyleDef;
            }

            thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
            if (thing.def.Minifiable && p.canBeMinified)
            {
                thing = thing.MakeMinified();
            }

            if (thing.def.CanHaveFaction)
            {
                if (thing.def.building != null && thing.def.building.isInsectCocoon)
                {
                    thing.SetFaction(Faction.OfInsects);
                }
                else
                {
                    thing.SetFaction(targetFaction);
                }
            }

            CompMechPowerCellBuilding compMechPowerCellBuilding = thing.TryGetComp<CompMechPowerCellBuilding>();
            if (compMechPowerCellBuilding != null)
            {
                compMechPowerCellBuilding.Props.totalPowerTicks = p.totalPowerTicks;
            }

            thing.stackCount = p.stackCount;

            if (p.wipeMode.HasValue)
            {
                GenSpawn.Spawn(thingDef, target.Cell, map, p.wipeMode.Value);
            }
            else
            {
                GenPlace.TryPlaceThing(thing, target.Cell, map, (!p.direct) ? ThingPlaceMode.Near : ThingPlaceMode.Direct);
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            if (!target.Cell.Filled(parent.pawn.Map) || target.Cell.GetEdifice(parent.pawn.Map) == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}