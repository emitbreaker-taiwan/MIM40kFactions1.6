using MIM40kFactions.Compatibility;
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
        [Multiplayer.SyncMethod]
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

        // Sync method for clearing cache to ensure consistency
        [Multiplayer.SyncMethod]
        public static void ClearCache()
        {
            cachedThingCountByDef.Clear();
        }

        // Ensure cache synchronization when the game tick updates
        [Multiplayer.SyncMethod]
        public static void UpdateTick()
        {
            cachedTick = Find.TickManager.TicksGame;
        }
    }
}
