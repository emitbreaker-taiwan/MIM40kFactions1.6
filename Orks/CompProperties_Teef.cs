using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MIM40kFactions.Orks
{
    public class CompProperties_Teef : CompProperties
    {
        public int smashTeefIntervalDays;

        public int teefAmount = 10;

        public ThingDef teefDef;

        [NoTranslate]
        public string saveKey;

        public bool humanlikeCanProduce = false;

        public bool shamblerCanProduce = false;

        public CompProperties_Teef()
        {
            compClass = typeof(CompTeef);
        }
    }
}
