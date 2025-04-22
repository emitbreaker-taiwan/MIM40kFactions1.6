using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Utility_HediffManagement
    {
        public static List<BodyPartRecord> GetBodyPartRecords(BodyPartDef partDef, Pawn pawn)
        {
            List<BodyPartRecord> list = new List<BodyPartRecord>();
            foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (notMissingPart.def == partDef)
                {
                    list.Add(notMissingPart);
                }
            }
            return list;
        }
    }
}
