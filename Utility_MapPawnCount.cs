using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public static class Utility_MapPawnCount
    {
        private static int cachedTick = -9999;
        private static Dictionary<ThingDef, int> cachedThingCountByDef = new Dictionary<ThingDef, int>();

        // Generalized method to count pawns by ThingDef (could be PawnKindDef or any other def type)
        public static int GetThingCountByDef(ThingDef def, Map map)
        {
            // Invalidate cache and update tick when necessary
            if (cachedTick != Find.TickManager.TicksGame)
            {
                UpdateTick();    // Sync the tick across all clients
                ClearCache();    // Clear the cache when the tick changes
            }

            // If the thing count for the given def is not cached, recalculate it
            if (!cachedThingCountByDef.ContainsKey(def))
            {
                cachedThingCountByDef[def] = map.listerThings.AllThings.Count(thing => thing.def == def);
            }

            return cachedThingCountByDef[def];
        }

        // ⚡ New: sums counts for multiple defs by re-using per-def cache
        /// <summary>
        /// Outline: returns the total number of things whose def is in `defs`, using
        /// the existing per-def cache in GetThingCountByDef.
        /// </summary>
        public static int GetThingCountByDefs(IEnumerable<ThingDef> defs, Map map)
        {
            if (defs == null || map == null)
                return 0;

            int totalCount = 0;
            foreach (var def in defs)
            {
                totalCount += GetThingCountByDef(def, map);
            }
            return totalCount;
        }

        // ⚡ New overload for convenience
        public static int GetThingCountByDefs(Map map, params ThingDef[] defs)
        {
            return GetThingCountByDefs((IEnumerable<ThingDef>)defs, map);
        }

        // Sync method for clearing cache to ensure consistency
        public static void ClearCache()
        {
            cachedThingCountByDef.Clear();
        }

        // Ensure cache synchronization when the game tick updates
        public static void UpdateTick()
        {
            cachedTick = Find.TickManager.TicksGame;
        }
    }
}
