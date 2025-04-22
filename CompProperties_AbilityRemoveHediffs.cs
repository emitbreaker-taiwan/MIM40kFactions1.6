using Verse;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using System.Text;

namespace MIM40kFactions
{
    public class CompProperties_AbilityRemoveHediffs : CompProperties_AbilityEffect
    {
        public bool applyToSelf;
        public bool applyToTarget;

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
        public bool isHereticAstartes = false;
        public List<string> keywords = new List<string>();

        public List<HediffDef> specificHediffs;

        public CompProperties_AbilityRemoveHediffs() => compClass = typeof(CompAbilityEffect_RemoveHediffs);
    }
}
