using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Utility_GeneDefManagement
    {
        public static GeneDef Named(string defName)
        {
            return DefDatabase<GeneDef>.GetNamed(defName);
        }
    }
}
