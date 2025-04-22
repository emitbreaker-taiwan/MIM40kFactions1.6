using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class PowerArmorCustomizationExtension : DefModExtension
    {
        // Can leave empty or add custom properties
        public bool OnlyForFaction = false;
        public TechLevel minTechLevel = TechLevel.Undefined;
    }
}