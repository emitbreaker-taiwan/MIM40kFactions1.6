using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MIM40kFactions
{
    public class CompProperties_AbilityDeepStrike : CompProperties_AbilityTeleport
    {
        public float rangetoHostile = 9f;
        public bool ignoreSelf = false;
        public bool applyToTarget = false;
        public bool applyToSelf = false;
        public bool onlyApplyToSelf = true;

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
    }
}
