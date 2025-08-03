using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class Thought_Memory_AsuryaniConsumed : Thought_Memory
    {
        private const int MoodShiftInterval = 60000; // Once per a day (60000 ticks)
        private float currentOffset;
        private int ticksSinceLastShift = 0;

        public override bool ShouldDiscard => false; // Stay forever in memory, until manually removed, pawn dies or move to other path.

        public override void ThoughtInterval()
        {
            base.ThoughtInterval();
            ticksSinceLastShift += 150;

            if (ticksSinceLastShift >= MoodShiftInterval)
            {
                ticksSinceLastShift = 0;
                float baseOffset = def.stages[CurStageIndex].baseMoodEffect;
                currentOffset = Rand.Range(-Mathf.Abs(baseOffset), Mathf.Abs(baseOffset));

                if (Prefs.DevMode)
                {
                    Log.Message($"[MIM Asuryani] {pawn.NameShortColored} Consumed mood fluctuated to {currentOffset:0.00}");
                }
            }
        }

        public override float MoodOffset()
        {
            return ThoughtUtility.ThoughtNullified(pawn, def) ? 0f : currentOffset;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentOffset, "currentOffset", 0f);
            Scribe_Values.Look(ref ticksSinceLastShift, "ticksSinceLastShift", 0);
        }

        public override string LabelCap => $"{base.LabelCap} ({currentOffset:+0.0;-0.0})";
    }
}
