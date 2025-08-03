using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class AsuryaniPathLevelEffects
    {
        public List<ThoughtDef> gainedThoughts;
        public List<HediffDef> gainedHediffs;
        public bool persistentHediff = false;
        public List<MentalBreakDef> forcedMentalBreaks;
        public string backstoryToApply;

        public AsuryaniPathLevelEffects()
        {
            gainedThoughts = new List<ThoughtDef>();
            gainedHediffs = new List<HediffDef>();
            forcedMentalBreaks = new List<MentalBreakDef>();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref gainedThoughts, "gainedThoughts", LookMode.Def);
            Scribe_Collections.Look(ref gainedHediffs, "gainedHediffs", LookMode.Def);
            Scribe_Values.Look(ref persistentHediff, "persistentHediff", false);
            Scribe_Collections.Look(ref forcedMentalBreaks, "forcedMentalBreaks", LookMode.Def);
            Scribe_Values.Look(ref backstoryToApply, "backstoryToApply");
        }

        public void ApplyEffects(Pawn pawn)
        {
            if (gainedThoughts != null)
            {
                foreach (var t in gainedThoughts)
                {
                    pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(t);
                }
            }

            if (gainedHediffs != null)
            {
                foreach (var h in gainedHediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(h))
                        pawn.health.AddHediff(h);
                }
            }

            if (!string.IsNullOrEmpty(backstoryToApply))
            {
                List<WorkTypeDef> previouslyEnabledWorkTypes = new List<WorkTypeDef>();
                if (pawn.workSettings != null && pawn.workSettings.EverWork)
                {
                    foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
                    {
                        if (pawn.workSettings.WorkIsActive(workType))
                        {
                            previouslyEnabledWorkTypes.Add(workType);
                        }
                    }
                }

                var newBackstory = DefDatabase<BackstoryDef>.GetNamedSilentFail(backstoryToApply);
                if (newBackstory != null)
                {
                    pawn.story.Adulthood = newBackstory;

                    if (pawn.workSettings == null)
                    {
                        pawn.workSettings = new Pawn_WorkSettings(pawn);
                    }

                    pawn.workSettings.EnableAndInitialize();

                    foreach (var wt in previouslyEnabledWorkTypes)
                    {
                        if (pawn.workSettings.WorkIsActive(wt))
                        {
                            pawn.workSettings.SetPriority(wt, 3);
                        }
                        else
                        {
                            pawn.workSettings.Disable(wt);
                        }
                    }
                }
                else
                {
                    Log.Warning($"[MIM Asuryani] BackstoryDef '{backstoryToApply}' not found for {pawn.NameShortColored}.");
                }
            }

            if (forcedMentalBreaks != null && Rand.Chance(0.2f))
            {
                foreach (var mb in forcedMentalBreaks)
                {
                    pawn.mindState?.mentalStateHandler?.TryStartMentalState(mb.mentalState);
                }
            }
        }

        public void RemoveEffects(Pawn pawn)
        {
            if (pawn == null) return;

            if (gainedThoughts != null)
            {
                foreach (var t in gainedThoughts)
                {
                    pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(t);
                }
            }

            if (!gainedHediffs.NullOrEmpty())
            {
                foreach (var def in gainedHediffs)
                {
                    var existing = pawn.health.hediffSet.GetFirstHediffOfDef(def);
                    if (existing != null)
                        pawn.health.RemoveHediff(existing);
                }
            }
        }
    }
}