using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    internal class Utility_FactionManagement
    {
        public static Faction Named(string defName)
        {            
            List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
            foreach (Faction faction in allFactionsListForReading)
            {
                if (faction.def == DefDatabase<FactionDef>.GetNamed(defName))
                    return faction;
            }
            return null;
        }
    }
}