using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class ModSettings_MIMWH40kFactions : ModSettings
    {
        public float accuracyModifier = 0;
        public float massModifier = 10f;
        public int maxOrkCount = 100;
        public int maxOrkoidCount = 300;
        public bool countSpore = false;
        public int maxSporeCount = 300;
        public bool debugMode = false; // ✅ New setting
        public bool useRimDarkSize = false;
        [NoTranslate]
        public List<string> globalBlockedThoughtDefs = new List<string>();

        public override void ExposeData()
        {
            Scribe_Values.Look<float>(ref accuracyModifier, "accuracyModifier", 0);
            Scribe_Values.Look<float>(ref massModifier, "massModifier", 10f);
            Scribe_Values.Look<int>(ref maxOrkCount, "maxOrkCount", 100);
            Scribe_Values.Look<int>(ref maxOrkoidCount, "maxOrkCount", 300);
            Scribe_Values.Look<bool>(ref countSpore, "countSpore", false);
            Scribe_Values.Look<int>(ref maxSporeCount, "maxSporeCount", 100);
            Scribe_Values.Look(ref debugMode, "debugMode", false);
            Scribe_Values.Look(ref useRimDarkSize, "useRimDarkSize", false);
            Scribe_Collections.Look(ref globalBlockedThoughtDefs, "globalBlockedThoughtDefs", LookMode.Value);
            base.ExposeData();
        }
    }
}
