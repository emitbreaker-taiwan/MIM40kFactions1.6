using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse;

namespace MIM40kFactions
{
    public class Hediff_Mutation_Poxwalker : HediffWithComps
    {
        public override void Tick()
        {
            if (!Utility_DependencyManager.IsDeathGuardActive())
                return;
            ++ageTicks;
            if ((double)Severity < 0.77)
                return;
            DoPostAction(pawn);
            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.story?.headType == Utility_HeadTypeDefManagement.Named("EMNC_Invisible") || pawn.story?.headType == Utility_HeadTypeDefManagement.Named("EMWH_Invisible") || pawn.RaceProps.IsAnomalyEntity)
                    DoRespawn(pawn);
                else
                    DoMutation(pawn);
            }
            if (!pawn.RaceProps.Humanlike)
            {
                DoRespawn(pawn);
            }
            else
            {
                Log.Error("Something went wrong with Pawn Race.");
            }
        }

        private static void DoMutation(Pawn EMDG_victim)
        {
            if (Utility_DependencyManager.IsDeathGuardActive())
            {
                if (EMDG_victim == null)
                    return;

                EMDG_victim.kindDef = PawnKindDef.Named("EMCM_DGPoxwalker");
                Faction faction = SelectFaction();
                EMDG_victim.SetFaction(faction);

                Hediff mutationHediff = EMDG_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMDG_Mutation_WalkingPox"));
                EMDG_victim.health.RemoveHediff(mutationHediff);
            }
        }
        private static void DoRespawn(Pawn EMDG_victim)
        {
            if (Utility_DependencyManager.IsDeathGuardActive())
            {
                Faction faction = SelectFaction();
                Ideo ideo = faction.ideos.PrimaryIdeo;

                if (ModsConfig.IdeologyActive)
                {
                    Utility_NonHumanlikeMutation.RespawnHumanlike(EMDG_victim, PawnKindDef.Named("EMCM_DGPoxwalker"), faction, XenotypeDefOf.Baseliner, HediffDef.Named("EMDG_Mutation_WalkingPox"), ideo);
                    return;
                }
                if (!ModsConfig.IdeologyActive)
                {
                    Utility_NonHumanlikeMutation.RespawnHumanlike(EMDG_victim, PawnKindDef.Named("EMCM_DGPoxwalker"), faction, XenotypeDefOf.Baseliner, HediffDef.Named("EMDG_Mutation_WalkingPox"));
                    return;
                }
            }
        }
        private static void DoPostAction(Pawn EMDG_victim)
        {
            if (Utility_DependencyManager.IsDeathGuardActive())
            {
                if (HediffDef.Named("EMCH_Poxwalker") != null)
                {
                    EMDG_victim.health.AddHediff(HediffDef.Named("EMCH_Poxwalker"), EMDG_victim.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso));
                }
            }
        }
        private static Faction SelectFaction()
        {
            Faction faction = new Faction();
            if (Utility_DependencyManager.IsDeathGuardActive())
            {                
                if (Utility_FactionManagement.Named("EMCM_DG_PlayerColony") != null)
                    faction = Faction.OfPlayer;
                else if (Utility_FactionManagement.Named("EMCM_DeathGuard") != null)
                {
                    faction = Utility_FactionManagement.Named("EMCM_DeathGuard");
                }
                else
                    faction = Faction.OfAncientsHostile;
            }
            return faction;
        }
    }
}