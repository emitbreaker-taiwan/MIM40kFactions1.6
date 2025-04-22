using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_GiveHediffsInRangebyKeyword : HediffCompProperties
    {
        public float range;
        public HediffDef hediff;
        public ThingDef mote;
        public TargetingParameters targetingParameters;
        public bool hideMoteWhenNotDrafted;
        public float initialSeverity = 1f;
        public bool replaceExisting = true;

        public bool onlyPawnsInSameFaction = false;
        public bool onlyTargetHostileFactions = false;
        public bool onlyTargetNonPlayerFactions = false;
        public bool onlyTargetNotinSameFactions = false;

        [MayRequireBiotech]
        public XenotypeDef excludeXenotype;
        [MayRequireBiotech]
        public List<XenotypeDef> excludeXenotypes;
        [MayRequireBiotech]
        public XenotypeDef targetXenotype;
        [MayRequireBiotech]
        public List<XenotypeDef> targetXenotypes;

        public HediffDef requiredHediff;
        public List<HediffDef> requiredHediffs;

        public bool useKeyword = false;
        public bool isVehicle = false;
        public bool isMonster = false;
        public bool isPsychic = false;
        public bool isPsyker = false;
        public bool isCharacter = false;
        public bool isAstartes = false;
        public bool isInfantry = false;
        public bool isWalker = false;
        public bool isLeader = false;
        public bool isFly = false;
        public bool isAircraft = false;
        public bool isChaos = false;
        public bool isDaemon = false;
        public bool isDestroyerCult = false;
        public List<string> keywords = new List<string>();

        public bool isHereticAstartes = false;

        public bool isHazardous = false;
        public HediffDef criticalEffect;
        public HediffDef fumbleEffect;

        public bool disabledWhenUnconscious = false;

        public HediffCompProperties_GiveHediffsInRangebyKeyword() => this.compClass = typeof(HediffComp_GiveHediffsInRangebyKeyword);

    }
}
