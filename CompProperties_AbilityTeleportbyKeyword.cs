using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_AbilityTeleportbyKeyword : CompProperties_EffectWithDest
    {
        public IntRange stunTicks;

        public float maxBodySize = 3.5f;

        public bool onlyPawnsInSameFaction = false;
        public bool onlyTargetHostileFactions = false;
        public bool onlyTargetNonPlayerFactions = false;
        public bool onlyTargetNotinSameFactions = false;

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
        public int keywordBonus = 0;

        public bool isHereticAstartes = false;

        public List<int> randomDamage;

        public CompProperties_AbilityTeleportbyKeyword()
        {
            compClass = typeof(CompAbilityEffect_TeleportbyKeyword);
        }
    }
}
