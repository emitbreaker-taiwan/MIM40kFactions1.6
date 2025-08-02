using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions.Orks
{
    public class CompProperties_SpawnThingsOnDamaged : CompProperties
    {
        public float spawnChance = 1f;

        public int spawnCooldownTicks = -1;

        public IntRange spawnThingCountRange = new IntRange(1, 1);

        public List<ThingDef> spawnThingKindOptions = new List<ThingDef>();

        public float spawnRadius = -1f;

        public int spawnMaxAdjacent = -1;

        public bool spawnForbidden = false;

        public bool showMessageIfOwned = false;

        public bool inheritFaction = false;

        public FactionDef defaultFactionDef = null;

        public FactionDef forceFactionDef = null;

        public List<ThingDef> targetRaceDefstoCount = new List<ThingDef>();

        public List<FactionDef> targetNPCFactions = new List<FactionDef>();

        public bool enableDebug = false;

        public CompProperties_SpawnThingsOnDamaged()
        {
            compClass = typeof(CompSpawnThingsOnDamaged);
        }
    }
}
