using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class SanitizerTrackerRegistry : IExposable
    {
        public static SanitizerTrackerRegistry Instance = new SanitizerTrackerRegistry();

        // ✅ Changed field types from interface to concrete
        private SanitizerApparelTracker apparelTracker = new SanitizerApparelTracker();
        private SanitizerConsumableTracker consumableTracker = new SanitizerConsumableTracker();
        private SanitizerRaceTracker raceTracker = new SanitizerRaceTracker();


        public void ExposeData()
        {
            Scribe_Deep.Look(ref apparelTracker, "apparelTracker");
            Scribe_Deep.Look(ref consumableTracker, "consumableTracker");
            Scribe_Deep.Look(ref raceTracker, "raceTracker");

            // Defensive re-init (in case fields fail to load)
            if (apparelTracker == null) apparelTracker = new SanitizerApparelTracker();
            if (consumableTracker == null) consumableTracker = new SanitizerConsumableTracker();
            if (raceTracker == null) raceTracker = new SanitizerRaceTracker();
        }

        public ISanitizerTracker Apparel => apparelTracker;
        public ISanitizerTracker Consumable => consumableTracker;
        public ISanitizerTracker Race => raceTracker;
    }
}
