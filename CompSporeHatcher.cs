using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MIM40kFactions
{
    
    public class CompSporeHatcher : ThingComp
    {
        private float gestateProgress;

        public Pawn hatcheeParent;
        public Pawn otherParent;
        public PawnKindDef hatcherPawn;
        public Faction hatcheeFaction;

        private int maxOrkCount => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().maxOrkCount;

        public CompProperties_SporeHatcher Props => (CompProperties_SporeHatcher)props;

        private CompTemperatureRuinable FreezerComp => parent.GetComp<CompTemperatureRuinable>();

        private bool TemperatureDamaged => FreezerComp?.Ruined ?? false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref gestateProgress, "gestateProgress", 0f);
            Scribe_References.Look(ref hatcheeParent, "hatcheeParent");
            Scribe_References.Look(ref otherParent, "otherParent");
            Scribe_References.Look(ref hatcheeFaction, "hatcheeFaction");
        }

        public override void CompTick()
        {
            if (!TemperatureDamaged)
            {
                if (parent.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
                {
                    // CarryTracker case, not needed right now, skip
                }
                else
                {
                    float num = 1f / (Props.hatcherDaystoHatch * 60000f);
                    gestateProgress += num;

                    if (gestateProgress > 1f)
                    {
                        ProcessHatch();
                    }
                }
            }
        }

        private void ProcessHatch()
        {
            // Cache and reduce redundant calculations
            float rand = Rand.Range(1f, 100f);

            if (Props.enableDebug)
            {
                Log.Message(rand.ToString());
            }

            // Spawning checks (if spawned or not)
            if (parent.ParentHolder is Pawn_CarryTracker pawn_CarryTracker2)
            {
                pawn_CarryTracker2.TryDropCarriedThing(parent.PositionHeld, ThingPlaceMode.Near, out var _);
            }

            if (GetMapOrkCount() > maxOrkCount)
            {
                GenSpawn.Spawn(Props.okoidfungusDef, parent.Position, parent.Map);
                parent.Destroy();
            }
            else
            {
                if (rand > 80f)
                {
                    Hatch();
                }
                else
                {
                    GenSpawn.Spawn(Props.okoidfungusDef, parent.Position, parent.Map);
                    parent.Destroy();
                }
            }
        }

        public void Hatch()
        {
            try
            {
                // Simplified pawn generation logic
                SelectPawnToSpawn();

                if (hatcherPawn == null)
                {
                    return;
                }

                PawnGenerationRequest request = CreatePawnGenerationRequest();

                // Generate the pawn and spawn it
                SpawnPawn(request);
            }
            finally
            {
                parent.Destroy();
            }
        }

        private void SelectPawnToSpawn()
        {
            // Optimize by caching or limiting the selection logic
            if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                hatcherPawn = Props.hatcherPawnDef ?? PawnKindDef.Named("Chicken");
            }
            else
            {
                float rand = Rand.Range(1f, 100f);
                hatcherPawn = GetHatcherPawnBasedOnConditions(rand);
            }

            if (Props.enableDebug)
            {
                Log.Message("Selected PawnKind is" + hatcherPawn.label);
            }
        }

        private PawnKindDef GetHatcherPawnBasedOnConditions(float rand)
        {
            if (GetMapGrotCount() <= 10)
            {
                return rand <= 60f ? Props.squigPawnKindDefs.RandomElement() : Props.grotPawnKindDefs.RandomElement();
            }
            else if (GetMapGrotCount() > 10 && GetMapOrkCount() <= 10)
            {
                return rand <= 60f ? Props.orkPawnKindDefs.RandomElement() : GetRandomSquigOrGrotPawn(rand);
            }
            else
            {
                return rand > 60f ? Props.orkPawnKindDefs.RandomElement() : GetRandomSquigOrGrotPawn(rand);
            }
        }

        private PawnKindDef GetRandomSquigOrGrotPawn(float rand)
        {
            return rand <= 60f ? Props.squigPawnKindDefs.RandomElement() : Props.grotPawnKindDefs.RandomElement();
        }

        private PawnGenerationRequest CreatePawnGenerationRequest()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(hatcherPawn, hatcheeFaction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: false, allowPregnant: false, allowFood: false, allowAddictions: false, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult);
            request.FixedBiologicalAge = 0.1f;
            request.FixedChronologicalAge = 0.1f;
            request.ForceNoGear = true;
            return request;
        }

        private void SpawnPawn(PawnGenerationRequest request)
        {
            for (int i = 0; i < parent.stackCount; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent))
                {
                    if (pawn != null)
                    {
                        AddPawnRelations(pawn);
                    }

                    if (parent.Spawned && Props.filthDef != null)
                    {
                        FilthMaker.TryMakeFilth(parent.Position, parent.Map, Props.filthDef);
                    }
                }
                else
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
            }
        }

        private void AddPawnRelations(Pawn pawn)
        {
            if (hatcheeParent != null)
            {
                if (pawn.playerSettings != null && hatcheeParent.playerSettings != null)
                {
                    pawn.playerSettings.AreaRestrictionInPawnCurrentMap = hatcheeParent.playerSettings.AreaRestrictionInPawnCurrentMap;
                }

                if (pawn.RaceProps.IsFlesh && Props.allowRelationship)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, hatcheeParent);
                }
            }

            if (otherParent != null && (hatcheeParent == null || hatcheeParent.gender != otherParent.gender) && pawn.RaceProps.IsFlesh)
            {
                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, otherParent);
            }
        }

        private int GetMapGrotCount()
        {
            if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                return 0;
            }

            return Utility_MapPawnCount.GetThingCountByDefs(Props.grotRaceThingDefs, parent.Map);
        }

        private int GetMapOrkCount()
        {
            if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                return 0;
            }

            return Utility_MapPawnCount.GetThingCountByDefs(Props.orkRaceThingDefs, parent.Map);
        }

        public override bool AllowStackWith(Thing other)
        {
            CompSporeHatcher comp = ((ThingWithComps)other).GetComp<CompSporeHatcher>();
            if (TemperatureDamaged != comp.TemperatureDamaged)
            {
                return false;
            }

            return base.AllowStackWith(other);
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(parent.stackCount + count);
            float b = ((ThingWithComps)otherStack).GetComp<CompSporeHatcher>().gestateProgress;
            gestateProgress = Mathf.Lerp(gestateProgress, b, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            CompSporeHatcher comp = ((ThingWithComps)piece).GetComp<CompSporeHatcher>();
            comp.gestateProgress = gestateProgress;
            comp.hatcheeParent = hatcheeParent;
            comp.otherParent = otherParent;
            comp.hatcheeFaction = hatcheeFaction;
        }

        public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
        {
            base.PrePreTraded(action, playerNegotiator, trader);
            switch (action)
            {
                case TradeAction.PlayerBuys:
                    hatcheeFaction = Faction.OfPlayer;
                    break;
                case TradeAction.PlayerSells:
                    hatcheeFaction = trader.Faction;
                    break;
            }
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
        {
            base.PostPostGeneratedForTrader(trader, forTile, forFaction);
            hatcheeFaction = forFaction;
        }

        public override string CompInspectStringExtra()
        {
            if (!TemperatureDamaged)
            {
                return "EMOK_SporeProgress".Translate() + ": " + gestateProgress.ToStringPercent() + "\n" + "HatchesIn".Translate() + ": " + "PeriodDays".Translate((Props.hatcherDaystoHatch * (1f - gestateProgress)).ToString("F1"));
            }

            return null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Force hatch Spore";
                command_Action.action = delegate
                {
                    float rand = Rand.Range(1f, 100f);
                    if (Props.enableDebug)
                    {
                        Log.Message(rand);
                    }
                    if (rand > 80f)
                    {
                        Hatch();
                    }
                    else if (rand <= 80f)
                    {
                        GenSpawn.Spawn(Props.okoidfungusDef, parent.Position, parent.Map);
                        parent.Destroy();
                    }
                    gestateProgress = 0f;
                };
                yield return command_Action;
            }
        }
    }
}
