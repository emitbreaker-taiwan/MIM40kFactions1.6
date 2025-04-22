using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_SpawnThingsOnDamaged : CompProperties
    {
        public float spawnChance = 1f;

        public int spawnCooldownTicks;

        public IntRange spawnThingCountRange = new IntRange(1, 1);

        public List<ThingDef> spawnThingKindOptions = new List<ThingDef>();

        public float spawnRadius = -1f;

        public int spawnMaxAdjacent = -1;

        public bool spawnForbidden;

        public bool showMessageIfOwned = false;

        public bool inheritFaction;

        public FactionDef defaultFactionDef = null;

        public FactionDef forceFactionDef = null;

        public List<ThingDef> targetRaceDefstoCount;

        public CompProperties_SpawnThingsOnDamaged()
        {
            compClass = typeof(CompSpawnThingsOnDamaged);
        }
    }
}
