using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using System;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class Utility_PawnValidator
    {
        public static bool IsPawnDeadValidator(Pawn pawn, bool checkUnconciousness = false)
        {
            if (pawn == null || pawn.Dead || pawn.health == null || pawn.Map == null)
            {
                return false; // ✨ Prevent ticking if dead/null
            }

            if (checkUnconciousness)                
            {
                if (!pawn.Awake() || pawn.health.InPainShock || !pawn.Spawned)
                {
                    return false;
                }
            }

            return true;
        }
        public static bool FactionValidator(Pawn target, Pawn caster, bool onlyTargetNotinSameFactions = false, bool onlyTargetHostileFactions = false, bool onlyPawnsInSameFaction = false, bool onlyTargetNonPlayerFactions = false)
        {
            if (onlyTargetNotinSameFactions && ((target.Faction == caster.Faction) || (target.Faction == caster.Faction)))
            {
                return false;
            }

            if (onlyTargetHostileFactions && (target.Faction.AllyOrNeutralTo(caster.Faction) || target.Faction == caster.Faction || (target.HostFaction != null && target.HostFaction == caster.Faction) || (target.SlaveFaction != null && target.SlaveFaction == caster.Faction)))
            {
                return false;
            }

            if (onlyPawnsInSameFaction && target.Faction != caster.Faction)
            {
                return false;
            }

            if (onlyTargetNonPlayerFactions && target.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return true;
        }

        public static bool FactionValidator(Pawn target, Thing caster, bool onlyTargetNotinSameFactions = false, bool onlyTargetHostileFactions = false, bool onlyPawnsInSameFaction = false, bool onlyTargetNonPlayerFactions = false)
        {
            if (onlyTargetNotinSameFactions && target.Faction == caster.Faction)
            {
                return false;
            }

            if (onlyTargetHostileFactions && target.Faction.AllyOrNeutralTo(caster.Faction))
            {
                return false;
            }

            if (onlyPawnsInSameFaction && target.Faction != caster.Faction)
            {
                return false;
            }

            if (onlyTargetNonPlayerFactions && target.Faction == Faction.OfPlayer)
            {
                return false;
            }
            return true;
        }

        public static bool XenotypeValidator(Pawn target, XenotypeDef excludeXenotype = null, List<XenotypeDef> excludeXenotypes = null, XenotypeDef targetXenotype = null, List<XenotypeDef> targetXenotypes = null)
        {
            if (!ModsConfig.BiotechActive)
            {
                return true;
            }

            if (!target.RaceProps.Humanlike)
            {
                return false;
            }

            if (excludeXenotype != null && target.genes.Xenotype == excludeXenotype)
            {
                return false;
            }
            if (excludeXenotypes != null)
            {
                if (excludeXenotypes.Contains(target.genes.Xenotype))
                    return false;
            }
            if (targetXenotype != null && target.genes.Xenotype != targetXenotype)
            {
                return false;
            }

            if (targetXenotypes != null)
            {
                return targetXenotypes.Contains(target.genes.Xenotype);
            }

            return true;
        }

        public static bool HediffValidator(Pawn target, HediffDef requiredHediff = null, List<HediffDef> requiredHediffs = null)
        {
            if (requiredHediff != null)
            {
                if (target.health.hediffSet.GetFirstHediffOfDef(requiredHediff) == null)
                {
                    return false;
                }
            }
            if (requiredHediffs != null)
            {
                foreach (HediffDef hediffDef in requiredHediffs)
                {
                    if (target.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public static bool KeywordValidator
        (
            Pawn target,
            List<string> keywords = null,
            bool isVehicle = false,
            bool isMonster = false,
            bool isPsychic = false,
            bool isPsyker = false,
            bool isCharacter = false,
            bool isAstartes = false,
            bool isInfantry = false,
            bool isWalker = false,
            bool isLeader = false,
            bool isFly = false,
            bool isAircraft = false,
            bool isChaos = false,
            bool isDaemon = false,
            bool isDestroyerCult = false,
            bool isHereticAstartes = false
        )
        {
            if (target == null || !target.Spawned)
            {
                return false;
            }
            if (isAstartes && !AstartesValidator(target))
            {
                return false;
            }
            if (isHereticAstartes && !HereticAstartesValidator(target))
            {
                return false;
            }
            if (isWalker && ModsConfig.IsActive("OskarPotocki.VFE.Pirates"))
            {
                if (target.apparel.WornApparel != null)
                {
                    foreach (Thing apparel in target.apparel.WornApparel)
                    {
                        if (apparel.def.tradeTags == null)
                        {
                            continue;
                        }
                        if (apparel.def.tradeTags.Contains("Warcasket"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            if (isVehicle)
            {
                if (ModsConfig.IsActive("OskarPotocki.VFE.Pirates") && target.apparel.WornApparel != null)
                {
                    foreach (Thing apparel in target.apparel.WornApparel)
                    {
                        if (apparel.def.tradeTags == null)
                        {
                            continue;
                        }
                        if (apparel.def.tradeTags.Contains("Warcasket"))
                        {
                            return true;
                        }
                    }
                }
                //if (ModsConfig.IsActive("SmashPhil.VehicleFramework") && target.def.thingClass.ToString() == "Vehicles.VehiclePawn")
                //{
                //    return true;
                //}
                if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.NC.Core"))
                {
                    if (target.kindDef.race == ThingDef.Named("EMNC_NecronCanoptekSpyderVanillaRace"))
                    {
                        return true;
                    }
                    if (target.kindDef.race == ThingDef.Named("EMNC_NecronCanoptekScarabSwarmVanillaRace"))
                    {
                        return true;
                    }
                }
                if (target.RaceProps.IsMechanoid)
                {
                    return true;
                }
                return false;
            }
            if (isPsyker && ModsConfig.RoyaltyActive && target.GetMainPsylinkSource() == null)
            {
                return false;
            }

            KeywordExtension modExtension = target.kindDef.GetModExtension<KeywordExtension>();

            if (modExtension == null || target.IsDessicated() || target.Dead || !target.Spawned)
            {
                return false;
            }

            if (isMonster && !modExtension.isMonster)
            {
                return false;
            }
            if (isPsychic && !modExtension.isPsychic)
            {
                return false;
            }
            if (isCharacter && !modExtension.isCharacter)
            {
                return false;
            }
            if (isInfantry && !modExtension.isInfantry)
            {
                return false;
            }
            if (isLeader && !modExtension.isLeader)
            {
                return false;
            }
            if (isFly && !modExtension.isFly)
            {
                return false;
            }
            if (isAircraft && !modExtension.isAircraft)
            {
                return false;
            }
            if (isChaos && !modExtension.isChaos)
            {
                return false;
            }
            if (isDaemon && !modExtension.isDaemon)
            {
                return false;
            }
            if (isDestroyerCult && !modExtension.isDestroyerCult)
            {
                return false;
            }
            if (keywords.NullOrEmpty())
            {
                return true;
            }
            if (!keywords.NullOrEmpty() && modExtension.keywords.NullOrEmpty())
            {
                return false;
            }
            if (!keywords.NullOrEmpty() && !modExtension.keywords.NullOrEmpty())
            {
                bool flag = false;

                foreach (string keyword in keywords)
                {
                    if (modExtension.keywords.Contains(keyword))
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AstartesValidator(Pawn target)
        {
            if (target.health.hediffSet.HasHediff(HediffDef.Named("EMWH_GeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMWH_GeneSeedNPC")))
            {
                return true;
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.BA"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_BAGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_BAGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.BT"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_BTGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_BTGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.CF"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_CFGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_CFGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.IF"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_IFGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_IFGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.IH"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_IHGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_IHGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.RG"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_RGGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_RGGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.SA"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_SAGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_SAGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.SW"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_SWGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_SWGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.WS"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_WSGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMSM_WSGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.BiotechActive && ModsConfig.IsActive("Phonicmas.40kGenes"))
            {
                if (target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_BlackCarapace")))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HereticAstartesValidator(Pawn target)
        {
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.DG"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMDG_GeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMDG_GeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.EC"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_ECGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_ECGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.FA"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_FAGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_FAGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_TSGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_TSGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.WE"))
            {
                if (target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_WEGeneSeed")) || target.health.hediffSet.HasHediff(HediffDef.Named("EMCM_WEGeneSeedNPC")))
                {
                    return true;
                }
            }
            if (ModsConfig.BiotechActive && ModsConfig.IsActive("Phonicmas.40kGenes") && target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_BlackCarapace")))
            {
                if (target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_KhorneMark")))
                {
                    return true;
                }
                if (target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_NurgleMark")))
                {
                    return true;
                }
                if (target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_TzeentchMark")))
                {
                    return true;
                }
                if (target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_SlaaneshMark")))
                {
                    return true;
                }
                if (target.genes.HasXenogene(Utility_GeneDefManagement.Named("BEWH_UndividedMark")))
                {
                    return true;
                }
            }
            return false;
        }

    }

}