using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class CompSpiritStoneDecay : ThingComp
    {
        public CompProperties_SpiritStoneDecay Props => (CompProperties_SpiritStoneDecay)props;

        private int decayProgressTicks = 0;
        private bool isDecaying = true; // True if exposed, false if stored/interred

        public bool IsDecaying => isDecaying;

        public int RemainingDecayTimeTicks => Mathf.Max(0, Props.decayDurationTicks - decayProgressTicks);

        public float DecayProgressPercent => (float)decayProgressTicks / Props.decayDurationTicks;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref decayProgressTicks, "decayProgressTicks", 0);
            Scribe_Values.Look(ref isDecaying, "isDecaying", true);
        }

        public override void CompTickRare()
        {
            if (!Utility_AsuryaniPath.IsAeldariCoreActive())
            {
                return;
            }

            base.CompTickRare();

            if (parent.ParentHolder is Building_SpiritStoneVault)
            {
                SetDecaying(false);
            }
            else
            {
                SetDecaying(true);
            }

            if (!isDecaying) return;

            decayProgressTicks += GenTicks.TickRareInterval;

            if (decayProgressTicks >= Props.decayDurationTicks)
            {
                TriggerSoulLostConsequences();
            }

            if (Props.debugMode)
            {
                Log.Message($"[MIM Debug] isDecaying={isDecaying}, holder={parent.ParentHolder?.GetType().Name}");
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            SetDecaying(true);
        }

        public void SetDecaying(bool decaying)
        {
            if (isDecaying != decaying)
            {
                isDecaying = decaying;
            }
        }

        private void TriggerSoulLostConsequences()
        {
            CompSpiritStone compSpiritStone = parent.TryGetComp<CompSpiritStone>();
            string pawnName = compSpiritStone?.pawnNameFull ?? "EMAE_UnknownSoul".Translate();

            if (Props.soulLostThought != null)
            {
                foreach (Pawn p in parent.Map.mapPawns.AllPawnsSpawned)
                {
                    if (Props.aeldariRaces.Contains(p.def) && p.needs?.mood != null)
                    {
                        p.needs.mood.thoughts.memories.TryGainMemory(Props.soulLostThought);
                        if (Props.debugMode)
                        {
                            Log.Message($"[MIM Aeldari] Aeldari pawn {p.LabelCap} gained thought: {Props.soulLostThought.label} due to {pawnName}'s soul loss.");
                        }
                    }
                }
            }
            else
            {
                Log.Warning("[MIM Aeldari] ThoughtDef 'Aeldari_SoulLostToSlaanesh' not found. Cannot apply mood debuff for lost soul.");
            }

            parent.Destroy(DestroyMode.Vanish);

            string messageTitle = "EMAE_AeldariSoulLostTitle".Translate();
            string messageText = "EMAE_AeldariSoulLostText".Translate(pawnName);

            Messages.Message(messageText, MessageTypeDefOf.NegativeEvent, historical: false);

            if (Props.debugMode)
            {
                Log.Warning($"[MIM Aeldari] Soul of {pawnName} lost to Slaanesh after {Props.decayDurationTicks} ticks of exposure (CompTickRare).");
            }
        }

        public override string CompInspectStringExtra()
        {
            string timeString;
            if (isDecaying)
            {
                float remainingDaysFloat = (float)RemainingDecayTimeTicks / GenDate.TicksPerDay;

                int remainingDays = Mathf.CeilToInt(remainingDaysFloat);

                timeString = $"{remainingDays} " + "DaysLower".Translate(); 

                if (remainingDays <= 0 && RemainingDecayTimeTicks > 0)
                {
                    timeString = "<1 " + "DayLower".Translate();
                }
                else if (RemainingDecayTimeTicks <= 0)
                {
                    timeString = "0 " + "DaysLower".Translate();
                }
            }
            else
            {
                timeString = "EMAE_SoulDecayPaused".Translate();
            }

            return "EMAE_SoulDecayStatus".Translate(timeString, DecayProgressPercent.ToStringPercent());
        }
    }
}
