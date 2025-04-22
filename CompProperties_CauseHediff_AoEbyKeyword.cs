using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_CauseHediff_AoEbyKeyword : CompProperties
    {
        public HediffDef hediff;

        public List<HediffDef> hediffs;

        public HediffDef hediffforMech;

        public List<HediffDef> hediffsforMech;

        public BodyPartDef part;

        public float range;

        public bool onlyTargetMechs;

        public bool ignoreMechs;

        public int checkInterval = 10;

        public SoundDef activeSound;

        public bool canTargetSelf = true;

        public bool drawLines = true;

        public bool onlyBrain;
        public bool applyToSelf;
        public bool onlyApplyToSelf;
        public bool applyToTarget = true;
        public bool replaceExisting;
        public float severity = -1f;
        public bool ignoreSelf;

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

        public bool onlyPawnsInSameFaction = false;
        public bool onlyTargetHostileFactions = false;
        public bool onlyTargetNonPlayerFactions = false;
        public bool onlyTargetNotinSameFactions = false;

        public bool useKeyword = false;
        public bool useKeywrodOnlyForNPC = false;
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

        public CompProperties_CauseHediff_AoEbyKeyword()
        {
            compClass = typeof(CompCauseHediff_AoEbyKeyword);
        }
    }
}
