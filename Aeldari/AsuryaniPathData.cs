using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class AsuryaniPathData : IExposable
    {
        public AsuryaniPathDef pathDef;
        public AsuryaniPathDef nextPlannedPath;

        public bool requestedToLeave = false;
        public int ticksEntered = -1;
        public int ticksExited = -1;
        public Dictionary<RecordDef, float> lastRecordCounts = new Dictionary<RecordDef, float>();

        public bool completed = false;
        public bool isExarch = false;
        public bool isLost = false;

        public AsuryaniPathData() { }

        public AsuryaniPathData(AsuryaniPathDef def)
        {
            pathDef = def;
            ticksEntered = Find.TickManager.TicksGame;
            lastRecordCounts = new Dictionary<RecordDef, float>();
        }

        public void RequestLeave() => requestedToLeave = true;
        public void CancelLeave() => requestedToLeave = false;

        public void MarkComplete()
        {
            completed = true;
            ticksExited = Find.TickManager.TicksGame;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref pathDef, "pathDef");
            Scribe_Defs.Look(ref nextPlannedPath, "nextPlannedPath");
            Scribe_Values.Look(ref requestedToLeave, "requestedToLeave", false);
            Scribe_Values.Look(ref ticksEntered, "ticksEntered", -1);
            Scribe_Values.Look(ref ticksExited, "ticksExited", -1);
            Scribe_Collections.Look(ref lastRecordCounts, "lastRecordCounts", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref completed, "completed", false);
            Scribe_Values.Look(ref isExarch, "isExarch", false);
            Scribe_Values.Look(ref isLost, "isLost", false);
        }
    }
}
