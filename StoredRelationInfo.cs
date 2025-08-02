using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class StoredRelationInfo : IExposable
    {
        public string otherPawnName;
        public Gender otherPawnGender;
        public string otherPawnKindDefName;
        public FactionDef otherPawnFactionDef;
        public PawnRelationDef relationDef;

        public void ExposeData()
        {
            Scribe_Values.Look(ref otherPawnName, "otherPawnName");
            Scribe_Values.Look(ref otherPawnGender, "otherPawnGender", Gender.None);
            Scribe_Values.Look(ref otherPawnKindDefName, "otherPawnKindDefName");
            Scribe_Defs.Look(ref otherPawnFactionDef, "otherPawnFactionDef");
            Scribe_Defs.Look(ref relationDef, "relationDef");
        }
    }
}
