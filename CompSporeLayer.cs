using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;
using MIM40kFactions.Compatibility;

namespace MIM40kFactions
{
    public class CompSporeLayer : ThingComp
    {
        public CompProperties_SporeLayer Props => (CompProperties_SporeLayer)props;
        private float sporeProgress;
        private int fertilizationCount;
        private Pawn fertilizedBy;
        private int maxOrkoidCount => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().maxOrkoidCount;

        private bool Active
        {
            get
            {
                Pawn pawn = parent as Pawn;
                if (Props.sporeLayFemaleOnly && pawn != null && pawn.gender != Gender.Female)
                {
                    return false;
                }

                if (pawn != null && !pawn.RaceProps.Humanlike && !pawn.ageTracker.CurLifeStage.milkable)
                {
                    return false;
                }

                if (pawn.Sterile() && !Props.sporeLaySterile)
                {
                    return false;
                }

                if (ModsConfig.AnomalyActive && pawn.IsShambler && !Props.sporeLayShambler)
                {
                    return false;
                }

                if (pawn.Downed || pawn.Dead || pawn.IsDessicated())
                {
                    return false;
                }

                // Use the utility method for pawn count with cached results
                if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core") && maxOrkoidCount > 0 && parent.Map != null)
                {
                    int orkCount = Utility_MapPawnCount.GetThingCountByDefs(Props.targetRaceDefstoCount, parent.Map);
                    if (orkCount >= maxOrkoidCount)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool CanLayNow
        {
            get
            {
                if (!Active)
                {
                    return false;
                }

                return sporeProgress >= 1f;
            }
        }

        public bool FullyFertilized => fertilizationCount >= Props.sporeFertilizationCountMax;

        private bool ProgressStoppedBecauseUnfertilized
        {
            get
            {
                if (Props.requireFertilization && Props.sporeFertilizationCountMax < 1f && fertilizationCount == 0)
                {
                    return sporeProgress >= Props.sporeProgressUnfertilizedMax;
                }

                return false;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref sporeProgress, "sporeProgress", 0f);
            Scribe_Values.Look(ref fertilizationCount, "fertilizationCount", 0);
            Scribe_References.Look(ref fertilizedBy, "fertilizedBy");
        }

        [Multiplayer.SyncMethod]
        public override void CompTick()
        {
            if (Active)
            {
                float num = 1f / (Props.sporeLayIntervalDays * 60000f);
                if (parent is Pawn pawn)
                {
                    num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
                }

                sporeProgress += num;
                if (sporeProgress > 1f)
                {
                    sporeProgress = 1f;
                }

                if (ProgressStoppedBecauseUnfertilized && Props.sporeProgressCanBeStoppedBecauseUnfertilized)
                {
                    sporeProgress = Props.sporeProgressUnfertilizedMax;
                }
            }
        }

        [Multiplayer.SyncMethod]
        public void Fertilize(Pawn male)
        {
            if (Props.requireFertilization)
            {
                fertilizationCount = Props.sporeFertilizationCountMax;
                fertilizedBy = parent as Pawn;
            }
        }

        //To Do: Fix for Grots and Squigs
        public ThingDef NextSporeType()
        {
            if (Props.requireFertilization && fertilizationCount > 0)
            {
                return Props.sporeFertilizedDef;
            }

            return Props.sporeUnfertilizedDef;
        }

        [Multiplayer.SyncMethod]
        public virtual Thing ProduceSpore()
        {
            if (!Active && Props.enableDebug)
            {
                Log.Error("LayEgg while not Active: " + parent);
            }

            sporeProgress = 0f;
            int randomInRange = Props.sporeCountRange.RandomInRange;
            if (randomInRange == 0)
            {
                return null;
            }

            //To Do: Fix for Grots and Squigs
            Thing thing;
            if (fertilizationCount > 0)
            {
                thing = ThingMaker.MakeThing(Props.sporeFertilizedDef);
                fertilizationCount = Mathf.Max(0, fertilizationCount - randomInRange);
            }
            else
            {
                thing = ThingMaker.MakeThing(Props.sporeUnfertilizedDef);
            }

            thing.stackCount = randomInRange;

            //To Do: Fix for Grots and Squigs
            CompSporeHatcher compSporeHatcher = thing.TryGetComp<CompSporeHatcher>();
            if (compSporeHatcher != null)
            {
                compSporeHatcher.hatcheeFaction = parent.Faction;
                if (parent is Pawn hatcheeParent)
                {
                    compSporeHatcher.hatcheeParent = hatcheeParent;
                }

                if (fertilizedBy != null)
                {
                    compSporeHatcher.otherParent = fertilizedBy;
                }
            }

            return thing;
        }

        public override string CompInspectStringExtra()
        {
            if (!Active)
            {
                return null;
            }

            string text = "EMOK_SporeProgress".Translate() + ": " + sporeProgress.ToStringPercent();
            if (fertilizationCount > 0)
            {
                text += "\n" + "EMOK_Fertilized".Translate();
            }
            else if (ProgressStoppedBecauseUnfertilized)
            {
                text += "\n" + "ProgressStoppedUntilFertilized".Translate();
            }

            return text;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            fertilizationCount = 0;
            fertilizedBy = null;
            sporeProgress = 0f; // Optional: wipe progress too
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Forece lay " + Props.sporeUnfertilizedDef.label;
                command_Action.action = delegate
                {
                    Pawn pawn = parent as Pawn;
                    Thing thing = pawn.GetComp<CompSporeLayer>().ProduceSpore();
                    GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, delegate (Thing t, int i)
                    {
                        if (pawn.Faction != Faction.OfPlayer)
                        {
                            t.SetForbidden(value: true);
                        }
                    });
                };
                yield return command_Action;
            }
        }
    }
}
