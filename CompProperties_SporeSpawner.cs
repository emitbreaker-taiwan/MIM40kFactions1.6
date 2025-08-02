using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions.Orks
{
    public class CompProperties_SporeSpawner : CompProperties
    {
        public List<ThingDef> thingDefsToSpawn = new List<ThingDef>();

        public IntRange spawnCount = new IntRange(1, 1);

        public IntRange spawnIntervalRange = new IntRange(100, 100);

        public float spawnRadius = -1f;

        public int spawnMaxAdjacent = -1;

        //public int targetPawnKindOnMap = -1;

        public bool spawnForbidden = false;

        public bool requiresPower = false;

        public bool writeTimeLeftToSpawn = false;

        public bool showMessageIfOwned = false;

        public string saveKeysPrefix;

        public bool inheritFaction = true;

        public FactionDef defaultFactionDef = null;

        public FactionDef forceFactionDef = null;

        public List<ThingDef> targetRaceDefstoCount = new List<ThingDef>();

        public List<FactionDef> targetNPCFactions = new List<FactionDef>();

        public bool enableDebug = false;

        public CompProperties_SporeSpawner()
        {
            compClass = typeof(CompSporeSpawner);
        }
    }
}
