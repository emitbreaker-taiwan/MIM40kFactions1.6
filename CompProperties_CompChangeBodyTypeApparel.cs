using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_CompChangeBodyTypeApparel : CompProperties
    {
        public BodyTypeDef targetBodyTypeDef;
        public Gender? gender;
        public List<GenderBodyTypeSet> genderBodyTypeSets;

        public CompProperties_CompChangeBodyTypeApparel()
        {
            compClass = typeof(CompChangeBodyType_Apparel);
        }
    }
}
