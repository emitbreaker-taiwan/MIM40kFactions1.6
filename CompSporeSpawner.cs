using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIM40kFactions.Compatibility;
using RimWorld;
using Verse;
using Verse.Noise;
using static HarmonyLib.Code;
using static MIM40kFactions.CompProperties_SporeHatcher;

namespace MIM40kFactions
{
    public class CompSporeSpawner : ThingComp
    {
        private int ticksUntilSpawn;
        public CompProperties_SporeSpawner PropsSpawner => (CompProperties_SporeSpawner)props;
        private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;
        private int maxOrkoidCount => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().maxOrkoidCount;
        private bool countSpore => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().countSpore;
        private int maxSporeCount => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().maxSporeCount;
        private FactionDef sporeFaction;

        private int MapTargetPawnKindCount
        {
            get
            {
                if (PropsSpawner.targetRaceDefstoCount == null)
                {
                    return 0;
                }

                return Utility_MapPawnCount.GetThingCountByDef(PropsSpawner.targetRaceDefstoCount.FirstOrDefault(), parent.Map);
            }
        }

        private int MapTargetSpore
        {
            get
            {
                int sporeCount = 0;

                if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
                {
                    return sporeCount;
                }

                // Only cache and calculate if the mod is active and `countSpore` is true
                if (countSpore)
                {
                    return Utility_MapPawnCount.GetThingCountByDef(PropsSpawner.thingDefsToSpawn.FirstOrDefault(), parent.Map);
                }

                return 0;
            }
        }

        [Multiplayer.SyncMethod]
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                ResetCountdown();
            }
        }

        [Multiplayer.SyncMethod]
        public override void CompTick()
        {
            if (!IsValidSpawner()) return;

            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                if (maxOrkoidCount > 0)
                {
                    if (MapTargetPawnKindCount >= maxOrkoidCount)
                    {
                        return;
                    }
                }
                if (countSpore && maxSporeCount > 0)
                {
                    if (MapTargetSpore >= maxSporeCount)
                    {
                        return;
                    }
                }
            }
            else
            {
                TickInterval(1);
            }
        }

        [Multiplayer.SyncMethod]
        public override void CompTickRare()
        {
            if (!IsValidSpawner()) return;

            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                if (maxOrkoidCount > 0)
                {
                    if (MapTargetPawnKindCount >= maxOrkoidCount)
                    {
                        return;
                    }
                }
                if (countSpore && maxSporeCount > 0)
                {
                    if (MapTargetSpore >= maxSporeCount)
                    {
                        return;
                    }
                }
            }
            else
            {
                TickInterval(250);
            }
        }

        [Multiplayer.SyncMethod]
        public override void CompTickLong()
        {
            if (!IsValidSpawner()) return;

            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                if (maxOrkoidCount > 0)
                {
                    if (MapTargetPawnKindCount >= maxOrkoidCount)
                    {
                        return;
                    }
                }
                if (countSpore && maxSporeCount > 0)
                {
                    if (MapTargetSpore >= maxSporeCount)
                    {
                        return;
                    }
                }
            }
            else
            {
                TickInterval(2000);
            }
        }

        private void TickInterval(int interval)
        {
            CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
            if (comp != null)
            {
                if (!comp.Awake)
                {
                    return;
                }
            }
            else if (parent.Position.Fogged(parent.Map))
            {
                return;
            }

            if (!PropsSpawner.requiresPower || PowerOn)
            {
                ticksUntilSpawn -= interval;
                CheckShouldSpawn();
            }
        }

        private void CheckShouldSpawn()
        {
            if (ticksUntilSpawn <= 0)
            {
                ResetCountdown();
                TryDoSpawn();
            }
        }

        [Multiplayer.SyncMethod]
        public bool TryDoSpawn()
        {
            if (!IsValidSpawner()) return false;

            if (PropsSpawner.spawnMaxAdjacent >= 0)
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
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (PropsSpawner.thingDefsToSpawn.Contains(thingList[j].def))
                        {
                            num += thingList[j].stackCount;
                            if (num >= PropsSpawner.spawnMaxAdjacent)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            foreach (ThingDef thingToSapwn in PropsSpawner.thingDefsToSpawn)
            {
                int spawnCount = PropsSpawner.spawnCount.RandomInRange;
                if (TryFindSpawnCell(parent, thingToSapwn, spawnCount, out var result))
                {
                    Thing thing = ThingMaker.MakeThing(thingToSapwn);
                    thing.stackCount = spawnCount;
                    if (thing == null)
                    {
                        Log.Error("Could not spawn anything for " + parent);
                    }

                    if (PropsSpawner.inheritFaction)
                    {
                        if (!thing.def.CanHaveFaction)
                        {

                        }
                        else
                        {
                            if (PropsSpawner.forceFactionDef != null)
                            {
                                thing.SetFaction(Find.FactionManager.FirstFactionOfDef(PropsSpawner.forceFactionDef));
                            }

                            if (parent.Faction == null)
                            {
                                if (PropsSpawner.defaultFactionDef == null || Find.FactionManager.FirstFactionOfDef(PropsSpawner.defaultFactionDef) == null)
                                {
                                    sporeFaction = Faction.OfInsects.def;
                                }

                                if (Find.FactionManager.FirstFactionOfDef(FactionDef.Named("EMOK_PlayerColony")) != null && parent.Map.IsPlayerHome)
                                {
                                    sporeFaction = FactionDef.Named("EMOK_PlayerColony");
                                }
                                else if (Find.FactionManager.FirstFactionOfDef(FactionDef.Named("EMOK_PlayerColony")) == null)
                                {
                                    foreach (Pawn pawn in parent.Map.mapPawns.FreeColonists)
                                    {
                                        if (PropsSpawner.targetRaceDefstoCount.Contains(pawn.kindDef.race))
                                        {
                                            sporeFaction = Faction.OfPlayer.def;
                                        }
                                    }
                                }
                            }

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

                    GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out var lastResultingThing);
                    if (PropsSpawner.spawnForbidden)
                    {
                        lastResultingThing.SetForbidden(value: true);
                    }

                    if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("MessageCompSpawnerSpawnedItem".Translate(thingToSapwn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
                    }

                    return true;
                }
            }

            return false;
        }

        public bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, PropsSpawner.spawnRadius < 0 ? 1f : PropsSpawner.spawnRadius, false).OfType<IntVec3>().ToList().InRandomOrder())
            {
                if (!item.Walkable(parent.Map))
                {
                    continue;
                }

                Building edifice = item.GetEdifice(parent.Map);
                if ((edifice != null && thingToSpawn.IsEdifice()) || (edifice is Building_Door building_Door && !building_Door.FreePassage) || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.Position, item, parent.Map)))
                {
                    continue;
                }

                bool flag = false;
                List<Thing> thingList = item.GetThingList(parent.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    result = item;
                    return true;
                }
            }

            result = IntVec3.Invalid;
            return false;
        }

        [Multiplayer.SyncMethod]
        private void ResetCountdown()
        {
            ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
        }

        public override void PostExposeData()
        {
            string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
            Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
        }

        public override string CompInspectStringExtra()
        {
            if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
            {
                return "HatchesIn".Translate() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
            }

            return null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                foreach (ThingDef thingToSapwn in PropsSpawner.thingDefsToSpawn)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "DEV: Spawn " + thingToSapwn.label;
                    command_Action.action = delegate
                    {
                        ticksUntilSpawn = 0;
                        CheckShouldSpawn();
                    };
                    yield return command_Action;
                }
            }
        }

        private bool IsValidSpawner()
        {
            return parent.Spawned && (!(parent is Pawn pawn) || Utility_PawnValidator.IsPawnDeadValidator(pawn));
        }
    }
}
