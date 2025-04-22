using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Utility_JobDefManagement
    {
        public static JobDef Named(string defName)
        {
            return DefDatabase<JobDef>.GetNamed(defName);
        }
    }
}
