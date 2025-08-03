using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class AsuryaniPathDef : Def
    {
        public AsuryaniPathCategory category;
        public float weight = 1f;
        public bool canBeExarch = false;
        public bool isStartingPath = false;
        public bool isFemaleOnly = false;
        public bool isMaleOnly = false;

        // Path Immersion Effects
        public List<AsuryaniPathRecordEntry> relevantRecords = new List<AsuryaniPathRecordEntry>();
        public AsuryaniPathLevelEffects levelEffects_Novice;
        public AsuryaniPathLevelEffects levelEffects_Disciplined;
        public AsuryaniPathLevelEffects levelEffects_Consumed;
        public AsuryaniPathLevelEffects levelEffects_Exarch;

        // Path Transition Options
        public List<AsuryaniPathTransitionEntry> transitionOptions = new List<AsuryaniPathTransitionEntry>();

        // Path Entry Conditions
        public List<AsuryaniPathTransitionEntry> entryConditions = new List<AsuryaniPathTransitionEntry>();
    }
}
