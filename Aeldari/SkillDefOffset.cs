using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class SkillDefOffset : IExposable
    {
        public SkillDef skillDef;
        public float offset;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref skillDef, "skillDef");
            Scribe_Values.Look(ref offset, "offset", 0f);
        }
    }
}
