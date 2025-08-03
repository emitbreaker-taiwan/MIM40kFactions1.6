using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class StoredPathHistoryEntry : IExposable
    {
        public string defName;
        public string label;
        public bool completed;
        public bool isExarch;
        public bool isLost;
        public int ticksEntered;
        public int ticksExited;

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref completed, "completed");
            Scribe_Values.Look(ref isExarch, "isExarch");
            Scribe_Values.Look(ref isLost, "isLost");
            Scribe_Values.Look(ref ticksEntered, "ticksEntered");
            Scribe_Values.Look(ref ticksExited, "ticksExited");
        }
    }
}
