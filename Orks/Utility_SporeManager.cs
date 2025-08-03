using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Orks
{
    public class Utility_SporeManager
    {
        public static bool IsOKCoreActive()
        {
            try
            {
                var type = Type.GetType("MIM40kFactions.Utility_DependencyManager, MIM40kFactions1.6");
                if (type == null) return false;

                var method = type.GetMethod("IsOKCoreActive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (method == null) return false;

                var result = method.Invoke(null, null);
                return result is bool b && b;
            }
            catch
            {
                return false;
            }
        }

        public static FactionDef SporeFactionManager(Thing thing, Thing parent, FactionDef defaultFactionDef, FactionDef forceFactionDef,  List<ThingDef> targetRaceDefstoCount, List<FactionDef> targetNPCFactions)
        {
            if (!IsOKCoreActive())
            {
                Log.Error("[MIM Debug] SporeManager: OKCore is not active. Cannot determine faction. Fallback to OfInsects");
                return Faction.OfInsects.def;
            }

            if (thing == null || parent == null || parent.Map == null)
            {
                Log.Error("[MIM Debug] SporeManager: Parent or Map is null for " + thing);
                return null;
            }

            if (thing == null)
            {
                Log.Error("[MIM Debug] SporeManager: Spore is null");
                return null;
            }

            if (forceFactionDef != null && Find.FactionManager.FirstFactionOfDef(forceFactionDef) != null)
            {
                return forceFactionDef;
            }

            if (defaultFactionDef != null && Find.FactionManager.FirstFactionOfDef(defaultFactionDef) != null)
            {
                return defaultFactionDef;
            }

            if (parent.Faction == null)
            {
                if (Find.FactionManager.FirstFactionOfDef(FactionDef.Named("EMOK_PlayerColony")) != null && parent.Map.IsPlayerHome)
                {
                    return FactionDef.Named("EMOK_PlayerColony");
                }

                if (Find.FactionManager.FirstFactionOfDef(FactionDef.Named("EMOK_PlayerColony")) == null)
                {
                    foreach (Pawn pawn in parent.Map.mapPawns.FreeColonists)
                    {
                        if (targetRaceDefstoCount.Contains(pawn.kindDef.race))
                        {
                            return Faction.OfPlayer.def;
                        }
                    }

                    foreach (FactionDef factionDef in targetNPCFactions)
                    {
                        if (Find.FactionManager.FirstFactionOfDef(factionDef) != null)
                        {
                            return factionDef;
                        }
                    }
                }
            }

            if (parent.Faction != null && Find.FactionManager.FirstFactionOfDef(parent.Faction.def) != null)
            {
                return parent.Faction.def;
            }

            return Faction.OfInsects.def;
        }
    }
}
