using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions.Orks
{
    public class CompSpawnThingsOnDamaged : ThingComp
    {
        public CompProperties_SpawnThingsOnDamaged Props => (CompProperties_SpawnThingsOnDamaged)props;
        private int lastSpawnedTick = -99999;
        private int cachedMapTargetPawnKindCount = -1;
        private int cachedPawnCountTick = -9999;

        private int maxOrkoidCount => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().maxOrkoidCount;
        private FactionDef sporeFaction;

        private const int SpawnCooldown = 60; // Cooldown for spawn in ticks (1 second = 60 ticks)

        private int MapTargetPawnKindCount
        {
            get
            {
                // Only recalculate when the tick changes
                if (cachedPawnCountTick != Find.TickManager.TicksGame)
                {
                    cachedPawnCountTick = Find.TickManager.TicksGame;

                    // Use the utility method to get the pawn count
                    cachedMapTargetPawnKindCount = Utility_MapPawnCount.GetThingCountByDefs(Props.targetRaceDefstoCount, parent.Map);
                }
                return cachedMapTargetPawnKindCount;
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            // Check spawn chance and cooldown before spawning
            if (Rand.Chance(Props.spawnChance) && Find.TickManager.TicksGame >= lastSpawnedTick + SpawnCooldown)
            {
                TryDoSpawn();
                lastSpawnedTick = Find.TickManager.TicksGame;
            }
        }

        public bool TryDoSpawn()
        {
            if (!parent.Spawned)
            {
                return false;
            }

            // Prevent spawning too many orkoids (if the mod is active)
            if (Utility_DependencyManager.IsOKCoreActive())
            {
                if (maxOrkoidCount > 0 && MapTargetPawnKindCount >= maxOrkoidCount)
                {
                    return false;
                }
            }

            // Check for spawn limits based on adjacency
            if (Props.spawnMaxAdjacent >= 0)
            {
                if (!CheckMaxAdjacentSpawn())
                {
                    return false;
                }
            }

            // Try to spawn the items
            foreach (ThingDef thingToSpawn in Props.spawnThingKindOptions)
            {
                int spawnCount = Props.spawnThingCountRange.RandomInRange;
                if (TryFindSpawnCell(parent, thingToSpawn, spawnCount, out var result))
                {
                    Thing thing = ThingMaker.MakeThing(thingToSpawn);
                    thing.stackCount = spawnCount;

                    if (thing == null)
                    {
                        Log.Error("Could not spawn anything for " + parent);
                        continue;
                    }

                    if (Props.inheritFaction)
                    {
                        sporeFaction = Utility_SporeManager.SporeFactionManager(thing, parent, Props.defaultFactionDef, Props.forceFactionDef, Props.targetRaceDefstoCount, Props.targetNPCFactions);

                        var compSporeHatcher = thing.TryGetComp<CompSporeHatcher>();

                        if (compSporeHatcher != null)
                        {
                            compSporeHatcher.hatcheeFaction = Find.FactionManager.FirstFactionOfDef(sporeFaction);
                        }

                        if (Props.enableDebug)
                        {
                            Log.Message("Spawning " + thing.def.label + " at " + result + " for " + parent + " has " + sporeFaction);
                        }
                    }
                    GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out var lastResultingThing);

                    // Mark as forbidden if specified
                    if (Props.spawnForbidden)
                    {
                        lastResultingThing.SetForbidden(true);
                    }

                    // Show message if owned by the player
                    if (Props.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("MessageCompSpawnerSpawnedItem".Translate(thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
                    }

                    return true;
                }
            }

            return false;
        }

        private bool CheckMaxAdjacentSpawn()
        {
            int num = 0;
            for (int i = 0; i < 9; i++)
            {
                IntVec3 c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(parent.Map) || c.Filled(parent.Map) || !c.Walkable(parent.Map))
                {
                    continue;
                }

                Building edifice = c.GetEdifice(parent.Map);
                if ((edifice is Building_Door building_Door && !building_Door.FreePassage) || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.Position, c, parent.Map)))
                {
                    continue;
                }

                List<Thing> thingList = c.GetThingList(parent.Map);
                foreach (Thing thing in thingList)
                {
                    if (Props.spawnThingKindOptions.Contains(thing.def))
                    {
                        num += thing.stackCount;
                        if (num >= Props.spawnMaxAdjacent)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void HandleItemFaction(Thing thing)
        {
            if (Props.inheritFaction)
            {
                if (!thing.def.CanHaveFaction) return;

                // Force specific faction assignment if provided
                if (Props.forceFactionDef != null)
                {
                    thing.SetFaction(Find.FactionManager.FirstFactionOfDef(Props.forceFactionDef));
                    return;
                }

                // Assign default faction based on parent faction or specific conditions
                if (parent.Faction == null)
                {
                    sporeFaction = AssignFactionBasedOnConditions();
                }

                // Ensure faction consistency
                if (parent.Faction != null && thing.Faction != parent.Faction)
                {
                    sporeFaction = parent.Faction.def;
                }

                Faction targetFaction = Find.FactionManager.FirstFactionOfDef(sporeFaction);
                if (thing.Faction != targetFaction)
                {
                    thing.SetFaction(targetFaction);
                }
            }
        }

        private FactionDef AssignFactionBasedOnConditions()
        {
            // Custom faction assignment logic
            if (Props.defaultFactionDef == null || Find.FactionManager.FirstFactionOfDef(Props.defaultFactionDef) == null)
            {
                return Faction.OfInsects.def;
            }

            if (Find.FactionManager.FirstFactionOfDef(FactionDef.Named("EMOK_PlayerColony")) != null && parent.Map.IsPlayerHome)
            {
                return FactionDef.Named("EMOK_PlayerColony");
            }

            return Faction.OfPlayer.def;
        }

        public bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            // Cache cells that are more likely to be valid.
            List<IntVec3> possibleCells = GenRadial.RadialCellsAround(parent.Position, Props.spawnRadius < 0 ? 1f : Props.spawnRadius, false).ToList();

            // Prioritize walkable, unoccupied, valid cells
            foreach (IntVec3 cell in possibleCells.InRandomOrder())
            {
                if (!cell.Walkable(parent.Map)) continue;
                if (!IsCellValidForSpawn(cell, thingToSpawn, spawnCount, parent)) continue;

                result = cell;
                return true;
            }

            result = IntVec3.Invalid;
            return false;
        }

        private bool IsCellValidForSpawn(IntVec3 cell, ThingDef thingToSpawn, int spawnCount, Thing parent)
        {
            // Check if cell is walkable
            if (!cell.Walkable(parent.Map)) return false;

            // Check if there is an obstacle like doors or impassable terrain
            Building edifice = cell.GetEdifice(parent.Map);
            if ((edifice is Building_Door building_Door && !building_Door.FreePassage) || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.Position, cell, parent.Map)))
            {
                return false;
            }

            // Check if the spawn stack count will exceed limit
            List<Thing> thingList = cell.GetThingList(parent.Map);
            return !thingList.Any(thing => thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref lastSpawnedTick, "lastSpawnedTick", -99999);
        }
    }
}
