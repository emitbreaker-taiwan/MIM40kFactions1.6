using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Utility_XenotypeManager
    {
        public static XenotypeDef XenotypeDefNamed(string defName)
        {
            return DefDatabase<XenotypeDef>.GetNamed(defName);
        }
    }
}