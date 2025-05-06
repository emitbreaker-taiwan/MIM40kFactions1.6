using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;
using static System.Collections.Specialized.BitVector32;
using AlienRace;
using static UnityEngine.GraphicsBuffer;
using RimWorld.Planet;
using static HarmonyLib.Code;
using Verse.Noise;
using Verse.AI;
using System.Reflection;

namespace MIM40kFactions
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.emitbreaker.MIM.WH40k.Core");
            //EMWH_ApparelRestrictionPatch
            //harmony.Patch(AccessTools.Method(typeof(Apparel), "PawnCanWear", new System.Type[2]
            //    {
            //            typeof (Pawn),
            //            typeof (bool)
            //    }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnCanWearbyTraitPostfix")));
            //harmony.Patch(AccessTools.Method(typeof(Apparel), "PawnCanWear", new System.Type[2]
            //    {
            //            typeof (Pawn),
            //            typeof (bool)
            //    }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnCanWearbyArmorPostfix")));

            //harmony.Patch(AccessTools.Method(typeof(Apparel), "PawnCanWear", new System.Type[2]
            //    {
            //            typeof (Pawn),
            //            typeof (bool)
            //    }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnCanWearbyapparelTagsPostfix")));

            //harmony.Patch(AccessTools.Method(typeof(Apparel), "PawnCanWear", new System.Type[2]
            //    {
            //            typeof (Pawn),
            //            typeof (bool)
            //    }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnCanWearbyRacePostfix")));
            //harmony.Patch(AccessTools.Method(typeof(Apparel), "PawnCanWear", new System.Type[2]
            //    {
            //            typeof (Pawn),
            //            typeof (bool)
            //    }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnCannotWearbyRacePostfix")));
            //harmony.Patch(AccessTools.Method(typeof(Apparel), "PawnCanWear", new System.Type[2]
            //    {
            //            typeof (Pawn),
            //            typeof (bool)
            //    }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnCanWearbyHediffPostfix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn_ApparelTracker), "Wear", new System.Type[3] { typeof(Apparel), typeof(bool), typeof(bool) }), new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("PawnApparelTrackerWearPrefix")));
            harmony.Patch(AccessTools.Method(typeof(PreceptComp_Apparel), "GiveApparelToPawn", new System.Type[2]{typeof (Pawn),typeof (Precept_Apparel)}), new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("GiveApparelToPawnPrefix")));

            //harmony.Patch(AccessTools.Method(typeof(PawnApparelGenerator), "GenerateStartingApparelFor", new System.Type[2] { typeof(Pawn), typeof(PawnGenerationRequest) }), postfix: new HarmonyMethod(typeof(EMWH_ApparelRestrictionPatch).GetMethod("GenerateStartingApparelForPostfix")));

            //EMWH_EquipmentRestrictionPatch
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("PawnCanEquipbyTraitPostfix")));
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("PawnCanEquipbyArmorPostfix")));
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("PawnCanEquipbyapparelTagsPostfix")));
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("PawnCanEquipbyRacePostfix")));
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("PawnCannotEquipbyRacePostfix")));
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("PawnCanEquipbyHediffPostfix")));
            harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain", new System.Type[3] { typeof(Pawn), typeof(Apparel), typeof(List<float>) }), postfix: new HarmonyMethod(typeof(EMWH_EquipmentRestrictionPatch).GetMethod("ApparelScoreGainPostfix")));

            //EMWH_WeaponRestrictionPatch
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new System.Type[4]
                {
                        typeof (Thing),
                        typeof (Pawn),
                        typeof (string).MakeByRefType(),
                        typeof (bool)
                }), postfix: new HarmonyMethod(typeof(EMWH_WeaponRestrictionPatch).GetMethod("PawnCannotEquipbyPostfix")));

            //EMWH_GeneRestrictionPatch
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "AddGene", new System.Type[2] { typeof(Gene), typeof(bool) }), new HarmonyMethod(typeof(EMWH_GeneRestrictionPatch).GetMethod("AddNecronGenePrefix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "AddGene", new System.Type[2] { typeof(Gene), typeof(bool) }), new HarmonyMethod(typeof(EMWH_GeneRestrictionPatch).GetMethod("AddGenetoKindPrefix")));
            
            //EMWH_PawnBodySnatcher
            harmony.Patch(AccessTools.Method(typeof(AgeInjuryUtility), "GenerateRandomOldAgeInjuries"), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GenerateRandomOldAgeInjuriesPrefix")));
            //harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Body), "GraphicFor", new System.Type[1] { typeof(Pawn) }), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GraphicForBodyPreFix")));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Body), "GraphicFor", new System.Type[1] { typeof(Pawn) }), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GraphicForBodyPreFix")), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GraphicForBodyPostFix")));
            //harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Head), "GraphicFor", new System.Type[1] { typeof(Pawn) }), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GraphicForHeadPreFix")));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderNode_Head), "GraphicFor", new System.Type[1] { typeof(Pawn) }), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GraphicForHeadPreFix")), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GraphicForHeadPostfix")));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderNodeWorker_Eye), "ScaleFor", new System.Type[2] { typeof(PawnRenderNode), typeof(PawnDrawParms) }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("ScaleForEyesPostFix")));
            harmony.Patch(AccessTools.Method(typeof(PawnRenderNodeWorker_Eye), "OffsetFor", new System.Type[3] { typeof(PawnRenderNode), typeof(PawnDrawParms), typeof(Vector3).MakeByRefType() }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("OffsetForEyesPostFix")));
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GetXenotypeForGeneratedPawn", new System.Type[1] { typeof(PawnGenerationRequest) }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GetXenotypeForGeneratedPawnPostfix")));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), "HumanlikeHeadWidthForPawn", new System.Type[1] { typeof(Pawn) }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("HumanlikeHeadWidthForPawnPostFix")));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), "HumanlikeBodyWidthForPawn", new System.Type[1] { typeof(Pawn) }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("HumanlikeBodyWidthForPawnPostFix")));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeHairSetForPawn", new System.Type[3] { typeof(Pawn), typeof(float), typeof(float) }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GetHumanlikeHairSetForPawnPostFix")));
            harmony.Patch(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), "GetHumanlikeBeardSetForPawn", new System.Type[3] { typeof(Pawn), typeof(float), typeof(float) }), postfix: new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("GetHumanlikeBeardSetForPawnPostFix")));
            harmony.Patch(AccessTools.Method(typeof(CompAbilityEffect_FleckOnTarget), "SpawnFleck", new System.Type[2] { typeof(LocalTargetInfo), typeof(FleckDef) }), new HarmonyMethod(typeof(EMWH_PawnBodySnatcher).GetMethod("SpawnFleckPreFix")));

            //EMWH_AbilityRestrictionPatch
            harmony.Patch(AccessTools.Method(typeof(Pawn_AbilityTracker), "GainAbility", new System.Type[1] { typeof(AbilityDef) }), new HarmonyMethod(typeof(EMWH_AbilityRestrictionPatch).GetMethod("GainAbilityPrefix")));
            harmony.Patch(AccessTools.Method(typeof(CompUseEffect_GainAbility), "DoEffect", new System.Type[1] { typeof(Pawn) }), new HarmonyMethod(typeof(EMWH_AbilityRestrictionPatch).GetMethod("DoEffectPrefix")));

            //EMWH_DamageWorkerPatch
            harmony.Patch(AccessTools.Method(typeof(DamageWorker), "Apply", new System.Type[2] { typeof(DamageInfo), typeof(Thing) }), postfix: new HarmonyMethod(typeof(EMWH_DamageWorkerPatch).GetMethod("ApplyPostfix")));

            //EMWH_ThoughtPatch
            harmony.Patch(AccessTools.Method(typeof(ThoughtUtility), "Witnessed", new System.Type[2] { typeof(Pawn), typeof(Pawn) }), postfix: new HarmonyMethod(typeof(EMWH_ThoughtPatch).GetMethod("WitnessedPostfix")));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_GroinUncovered), "HasUncoveredGroin", new System.Type[1] { typeof(Pawn) }), postfix: new HarmonyMethod(typeof(EMWH_ThoughtPatch).GetMethod("HasUncoveredGroinPostfix")));
            harmony.Patch(AccessTools.Method(typeof(CompNeuralSupercharger), "CanAutoUse", new System.Type[1] { typeof(Pawn) }), postfix: new HarmonyMethod(typeof(EMWH_ThoughtPatch).GetMethod("CanAutoUsePostfix")));
            harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_AgeReversalDemanded), "CanHaveThought", new System.Type[1] { typeof(Pawn) }), postfix: new HarmonyMethod(typeof(EMWH_ThoughtPatch).GetMethod("CanHaveThoughtAgeReversalPostfix")));

            //EMWH_MassPatch
            harmony.Patch(AccessTools.Method(typeof(MassUtility), "Capacity", new System.Type[2] { typeof(Pawn), typeof(StringBuilder) }), postfix: new HarmonyMethod(typeof(EMWH_MassPatch).GetMethod("CapacityPostfix")));

            //EMWH_HealthScalePatch
            harmony.Patch(AccessTools.PropertyGetter(typeof(Pawn), "HealthScale"), postfix: new HarmonyMethod(typeof(EMWH_HealthScalePatch).GetMethod("HealthScalePostfix")));

            //EMWH_EmbrasuresAttachmentPatch
            harmony.Patch(AccessTools.Method(typeof(Placeworker_AttachedToWall), "AllowsPlacing", new System.Type[6] { typeof(BuildableDef), typeof(IntVec3), typeof(Rot4), typeof(Map), typeof(Thing), typeof(Thing) }), postfix: new HarmonyMethod(typeof(EMWH_EmbrasuresAttachmentPatch).GetMethod("AllowsPlacingPostfix")));

            //EMWH_StartingHediffsPatch
            harmony.Patch(AccessTools.Method(typeof(HealthUtility), "AddStartingHediffs", new System.Type[2] { typeof(Pawn), typeof(List<StartingHediff>) }), postfix: new HarmonyMethod(typeof(EMWH_StartingHediffsPatch).GetMethod("AddStartingHediffsPostfix")));

            //EMWH_ApparelUtilityLayerPatch
            harmony.Patch(AccessTools.PropertyGetter(typeof(ApparelLayerDef), "IsUtilityLayer"), postfix: new HarmonyMethod(typeof(EMWH_ApparelUtilityLayerPatch).GetMethod("IsUtilityLayerPostfix")));

            //EMWH_PawnRedressPatch
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "IsValidCandidateToRedress", new System.Type[2] { typeof(Pawn), typeof(PawnGenerationRequest) }), postfix: new HarmonyMethod(typeof(EMWH_PawnRedressPatch).GetMethod("IsValidCandidateToRedressPostfix")));
 
            //EMWH_PawnSanitizerPatch
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), "TryGainMemory", new System.Type[3] { typeof(ThoughtDef), typeof(Pawn), typeof(Precept) }), prefix: new HarmonyMethod(typeof(EMWH_PawnSanitizerPatch).GetMethod("ThoughtBlockerPrefix")));
            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), "TryGainMemory", new System.Type[3] { typeof(ThoughtDef), typeof(Pawn), typeof(Precept) }), prefix: new HarmonyMethod(typeof(EMWH_PawnSanitizerPatch).GetMethod("ThoughtGainFilterPrefix")));
            harmony.Patch(AccessTools.Method(typeof(SituationalThoughtHandler), "TryCreateThought", new System.Type[1] { typeof(ThoughtDef) }), prefix: new HarmonyMethod(typeof(EMWH_PawnSanitizerPatch).GetMethod("SituationalThoughtBlockerPrefix")));
            harmony.Patch(AccessTools.Method(typeof(InspirationHandler), "TryStartInspiration", new System.Type[3] { typeof(InspirationDef), typeof(string), typeof(bool) }), prefix: new HarmonyMethod(typeof(EMWH_PawnSanitizerPatch).GetMethod("BlockInspirationPrefix")));
            harmony.Patch(AccessTools.Method(typeof(MentalStateHandler), "TryStartMentalState", new System.Type[9] { typeof(MentalStateDef), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(Pawn), typeof(bool), typeof(bool), typeof(bool) }), prefix: new HarmonyMethod(typeof(EMWH_PawnSanitizerPatch).GetMethod("BlockMentalStatePrefix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn), "Kill", new System.Type[2] { typeof(DamageInfo?), typeof(Hediff) }), prefix: new HarmonyMethod(typeof(EMWH_PawnSanitizerPatch).GetMethod("Pawn_Kill_CleanupSanitizerPrefix")));

            //EMWH_PawnRedressPatch
            harmony.Patch(AccessTools.Method(typeof(TattooDef), "GraphicFor", new System.Type[2] { typeof(Pawn), typeof(Color) }), prefix: new HarmonyMethod(typeof(EMWH_KillHARPatch).GetMethod("KillHARTranspilerforNonHARPanwsPrefix")));

        }
    }

    public static class EMWH_PawnBodySnatcher
    {
        [HarmonyPrefix]
        public static bool GenerateRandomOldAgeInjuriesPrefix(Pawn pawn)
        {
            EMNC_Necron_ValidatiorExtension modExtension = pawn.def.GetModExtension<EMNC_Necron_ValidatiorExtension>();
            bool immnueToage = false;
            if (modExtension != null)
            {
                immnueToage = true;
            }

            return !immnueToage;
        }

        [HarmonyPrefix]
        public static void GraphicForBodyPreFix(Pawn pawn)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return;
            }

            var modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
            if (modExtension == null)
            {
                return;
            }

            // ✅ Apply appearance tweaks (hair, beard, tattoos) and bodyTypeDef swap
            DoModextension(pawn, modExtension);
        }

        /// <summary>
        /// Applies body, hair, tattoo modifications to a pawn based on BodySnatcherExtension.
        /// </summary>
        private static void DoModextension(Pawn pawn, BodySnatcherExtension modExtension)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return;
            }

            if (modExtension == null)
            {
                return;
            }

            // ✅ Ensure pawn has a valid bodyType
            if (pawn.story?.bodyType == null)
            {
                pawn.story.bodyType = BodyTypeDefOf.Hulk;
            }

            // ✅ Apply bodyTypeDef if specified
            if (modExtension.bodyTypeDef != null && Utility_GeneManager.GeneValidator(pawn, modExtension))
            {
                BodySnatcher(pawn, modExtension);
            }

            // ✅ Remove - No more direct body graphic path changes here
            // Graphic override is now handled dynamically during rendering.

            // ✅ Apply appearance modifications
            if (pawn.style != null)
            {
                if (modExtension.invisibleHead)
                {
                    if (modExtension.noBeard)
                        pawn.style.beardDef = BeardDefOf.NoBeard;
                    if (modExtension.noFaceTattoo)
                        pawn.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
                }
                if (modExtension.noBodyTattoo && !modExtension.pathBody.NullOrEmpty())
                    pawn.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
            }

            if (pawn.story != null && modExtension.invisibleHead && modExtension.noHair)
            {
                // only switch to Bald if not already set, so you don’t stomp user changes repeatedly
                if (pawn.story.hairDef == null || pawn.story.hairDef != HairDefOf.Bald)
                {
                    pawn.story.hairDef = HairDefOf.Bald;
                }
            }
        }

        [HarmonyPostfix]
        public static void GraphicForBodyPostFix(Pawn pawn, ref Graphic __result)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return;
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
            if (modExtension == null || modExtension.pathBody.NullOrEmpty())
                return;

            Shader shader = ShaderUtility.GetSkinShader(pawn);
            Vector2 drawSize = Vector2.one;
            if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                drawSize = modExtension.drawSize == default ? Vector2.one : modExtension.drawSize;
            }
            Color color = Color.white;

            if (modExtension.useGeneSkinColor)
            {
                color = pawn.story.SkinColor;
            }

            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                __result = GraphicDatabase.Get<Graphic_Multi>(modExtension.pathBody, shader, drawSize, color);
            }
            else
            {
                __result = GraphicDatabase.Get<Graphic_Multi>(modExtension.pathBody, shader, drawSize, color);
            }
        }

        private static void BodySnatcher(Pawn pawn, BodySnatcherExtension modExtension)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return;
            }

            if (modExtension.bodyTypeDef != null)
            {
                if (pawn.story.bodyType != modExtension.bodyTypeDef)
                {
                    if (DefDatabase<BodyTypeDef>.AllDefsListForReading.Contains(modExtension.bodyTypeDef))
                    {
                        pawn.story.bodyType = modExtension.bodyTypeDef;
                    }
                    else
                    {
                        Log.Warning("Selected bodyType does not exist. Keeping original bodyType.");
                    }
                }
            }

            // ❗ NO: Don't mutate bodyType.graphicPath anymore
            // Graphic override now handled dynamically.

            if (modExtension.bodyTypeDef == null && modExtension.pathBody.NullOrEmpty())
            {
                Log.Error("Both bodyTypeDef and pathBody are null. Operation failed.");
            }
        }

        /// <summary>
        /// Old GraphicForBodyPreFix for just in case
        /// </summary>
        //[HarmonyPrefix]
        //public static void GraphicForBodyPreFix(Pawn pawn)
        //{
        //    if (pawn == null || !pawn.RaceProps.Humanlike)
        //    {
        //        return;
        //    }
        //    if (pawn.IsDessicated())
        //    {
        //        return;
        //    }

        //    if (pawn.RaceProps.Humanlike)
        //    {
        //        BodyGraphicRecovery(pawn);
        //    }

        //    BodySnatcherExtension modExtension = Utility_BodySnatcherExtensionManager.GetBodySnatcherExtension(pawn);
        //    if (modExtension == null)
        //    {
        //        return;
        //    }
        //    DoModextension(pawn, modExtension);
        //}

        /// <summary>
        /// Old BodyGraphicRecovery for just in case
        /// </summary>
        //private static void BodyGraphicRecovery(Pawn pawn)
        //{
        //    string pathBodyMale = "Things/Pawn/Humanlike/Bodies/Naked_Male";
        //    string pathBodyMaleDessicated = "Things/Pawn/Humanlike/Bodies/Dessicated/Dessicated_Male";
        //    string pathBodyFemale = "Things/Pawn/Humanlike/Bodies/Naked_Female";
        //    string pathBodyFemaleDessicated = "Things/Pawn/Humanlike/Bodies/Dessicated/Dessicated_Thin";
        //    string pathBodyThin = "Things/Pawn/Humanlike/Bodies/Naked_Thin";
        //    string pathBodyThinDessicated = "Things/Pawn/Humanlike/Bodies/Dessicated/Dessicated_Thin";
        //    string pathBodyHulk = "Things/Pawn/Humanlike/Bodies/Naked_Hulk";
        //    string pathBodyHulkDessicated = "Things/Pawn/Humanlike/Bodies/Dessicated/Dessicated_Male";
        //    string pathBodyFat = "Things/Pawn/Humanlike/Bodies/Naked_Fat";
        //    string pathBodyFatDessicated = "Things/Pawn/Humanlike/Bodies/Dessicated/Dessicated_Male";

        //    if (pawn.story.bodyType == BodyTypeDefOf.Male)
        //    {
        //        pawn.story.bodyType.bodyNakedGraphicPath = pathBodyMale;
        //        pawn.story.bodyType.bodyDessicatedGraphicPath = pathBodyMaleDessicated;
        //    }
        //    if (pawn.story.bodyType == BodyTypeDefOf.Female)
        //    {
        //        pawn.story.bodyType.bodyNakedGraphicPath = pathBodyFemale;
        //        pawn.story.bodyType.bodyDessicatedGraphicPath = pathBodyFemaleDessicated;
        //    }
        //    if (pawn.story.bodyType == BodyTypeDefOf.Thin)
        //    {
        //        pawn.story.bodyType.bodyNakedGraphicPath = pathBodyThin;
        //        pawn.story.bodyType.bodyDessicatedGraphicPath = pathBodyThinDessicated;
        //    }
        //    if (pawn.story.bodyType == BodyTypeDefOf.Hulk)
        //    {
        //        pawn.story.bodyType.bodyNakedGraphicPath = pathBodyHulk;
        //        pawn.story.bodyType.bodyDessicatedGraphicPath = pathBodyHulkDessicated;
        //    }
        //    if (pawn.story.bodyType == BodyTypeDefOf.Fat)
        //    {
        //        pawn.story.bodyType.bodyNakedGraphicPath = pathBodyFat;
        //        pawn.story.bodyType.bodyDessicatedGraphicPath = pathBodyFatDessicated;
        //    }
        //}

        /// <summary>
        /// Old DoModextension for just in case
        /// </summary>
        //private static void DoModextension(Pawn pawn, BodySnatcherExtension modExtension)
        //{
        //    bool flag = false;
        //    if (pawn.story.bodyType == modExtension.bodyTypeDef)
        //    {
        //        return;
        //    }

        //    if (modExtension != null && modExtension.bodyTypeDef == null && modExtension.pathBody.NullOrEmpty())
        //    {
        //        return;
        //    }

        //    if (pawn.story.bodyType == null)
        //    {
        //        pawn.story.bodyType = BodyTypeDefOf.Hulk;
        //    }

        //    if (modExtension != null && GeneValidator(pawn, flag, modExtension))
        //    {
        //        BodySnatcher(pawn, modExtension);
        //    }

        //    if (modExtension.noBeard)
        //    {
        //        pawn.style.beardDef = BeardDefOf.NoBeard;
        //    }

        //    if (modExtension.noHair)
        //    {
        //        pawn.story.hairDef = HairDefOf.Bald;
        //    }

        //    if (modExtension.noFaceTattoo)
        //    {
        //        pawn.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
        //    }

        //    if (modExtension.noBodyTattoo)
        //    {
        //        pawn.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
        //    }

        //    Shader shader = ShaderUtility.GetSkinShader(pawn);
        //    Vector2 drawSize = Vector2.one;
        //    Color color = pawn.story.SkinColor;
        //    if (modExtension.invisibleSkin)
        //    {
        //        color = new Color((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue, 0.0f);
        //    }

        //    BodyGraphicReplacer(pawn, shader, drawSize, color, modExtension);
        //}

        /// <summary>
        /// Old GeneValidator for just in case
        /// </summary>
        //public static bool GeneValidator(Pawn pawn, bool flag, BodySnatcherExtension modExtension)
        //{
        //    if (!ModsConfig.BiotechActive || modExtension.requiredGene == null)
        //    {
        //        return true;
        //    }

        //    if (ModsConfig.BiotechActive && modExtension.requiredGene != null)
        //    {
        //        foreach (Gene gene in pawn.genes?.GenesListForReading)
        //        {
        //            if (gene.def == modExtension.requiredGene)
        //            {
        //                flag = true;
        //            }
        //        }
        //    }
        //    if (flag == true)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        Log.Warning("Seleted pawn does not have necessary Gene.");
        //        return false;
        //    }
        //}

        /// <summary>
        /// Old BodySnatcher for just in case
        /// </summary>
        //private static void BodySnatcher(Pawn pawn, BodySnatcherExtension modExtension)
        //{
        //    if (modExtension.bodyTypeDef != null)
        //    {
        //        BodyTypeDef original = pawn.story.bodyType;
        //        if (original != modExtension.bodyTypeDef)
        //        {
        //            bool flag = false;
        //            List<BodyTypeDef> bodyTypedefListForReading = DefDatabase<BodyTypeDef>.AllDefsListForReading;
        //            foreach (BodyTypeDef bodyType in bodyTypedefListForReading)
        //            {
        //                if (bodyType == modExtension.bodyTypeDef)
        //                {
        //                    flag = true;
        //                }
        //            }
        //            if (flag == true)
        //            {
        //                pawn.story.bodyType = modExtension.bodyTypeDef;
        //            }
        //            else
        //            {
        //                Log.Warning("Seleted bodyType does not exist. Set original bodyType.");
        //                pawn.story.bodyType = original;
        //                return;
        //            }
        //        }
        //    }

        //    if (!modExtension.pathBody.NullOrEmpty() && PathBodyValidator(modExtension))
        //    {
        //        pawn.story.bodyType.bodyNakedGraphicPath = modExtension.pathBody;
        //        pawn.story.bodyType.bodyDessicatedGraphicPath = modExtension.pathBody;
        //    }

        //    if (modExtension.bodyTypeDef == null && modExtension.pathBody == null)
        //    {
        //        Log.Error("Both bodyTypeDef and pathBody null. Operation failed");
        //        return;
        //    }
        //}

        /// <summary>
        /// Old PathBodyValidator for just in case
        /// </summary>
        //private static bool PathBodyValidator(BodySnatcherExtension modExtension)
        //{
        //    if (modExtension.bodyTypeDef == null)
        //    {
        //        return true;
        //    }
        //    if (modExtension.bodyTypeDef != null && modExtension.usepathBodywithBodyDef)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// Old BodyGraphicReplacer for just in case
        /// </summary>
        //private static Graphic BodyGraphicReplacer(Pawn pawn, Shader shader, Vector2 drawSize, Color color, BodySnatcherExtension modExtension)
        //{
        //    if (modExtension.drawSize != null && !ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
        //    {
        //        drawSize = modExtension.drawSize;
        //    }

        //    if (shader == null)
        //    {
        //        return null;
        //    }

        //    if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
        //    {
        //        return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, shader);
        //    }

        //    if (ModsConfig.AnomalyActive && pawn.IsMutant && !pawn.mutant.Def.bodyTypeGraphicPaths.NullOrEmpty())
        //    {
        //        return GraphicDatabase.Get<Graphic_Multi>(pawn.mutant.Def.GetBodyGraphicPath(pawn), shader, drawSize, color);
        //    }

        //    if (ModsConfig.AnomalyActive && pawn.IsCreepJoiner && pawn.story.bodyType != null && !pawn.creepjoiner.form.bodyTypeGraphicPaths.NullOrEmpty())
        //    {
        //        return GraphicDatabase.Get<Graphic_Multi>(pawn.creepjoiner.form.GetBodyGraphicPath(pawn), shader, drawSize, color);
        //    }

        //    if (pawn.story?.bodyType?.bodyNakedGraphicPath == null)
        //    {
        //        return null;
        //    }

        //    return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, shader, drawSize, color);
        //}

        [HarmonyPrefix]
        public static void GraphicForHeadPreFix(Pawn pawn)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return;
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
            if (modExtension == null)
                return;

            if (!modExtension.invisibleHead && modExtension.headTypeDef == null)
                return;

            // ✅ Set invisible head if requested
            if (modExtension.invisibleHead)
            {
                pawn.story.headType = Utility_HeadTypeDefManagement.Named("EMWH_Invisible");
                return;
            }

            if (pawn.story.headType == null)
            {
                pawn.story.headType = Utility_HeadTypeDefManagement.Named("Male_AverageNormal");
            }

            if (modExtension.headTypeDef != null && Utility_GeneManager.GeneValidator(pawn, modExtension))
            {
                if (DefDatabase<HeadTypeDef>.AllDefsListForReading.Contains(modExtension.headTypeDef))
                {
                    pawn.story.headType = modExtension.headTypeDef;
                }
                else
                {
                    Log.Warning("Selected headType does not exist. Keeping original headType.");
                }
            }
        }

        [HarmonyPostfix]
        public static void GraphicForHeadPostfix(Pawn pawn, ref Graphic __result)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn) || pawn.health?.hediffSet == null || !pawn.health.hediffSet.HasHead)
            {
                return;
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
            if (modExtension == null)
                return;

            if (modExtension.invisibleHead)
            {
                Shader shader = ShaderUtility.GetSkinShader(pawn);
                Vector2 drawSize = Vector2.one;
                Color color = Color.clear; // Fully transparent

                __result = GraphicDatabase.Get<Graphic_Multi>("PawnRenderNode/InvisibleHead", shader, drawSize, color);
            }
        }
        /// <summary>
        /// Old GraphicForHeadPreFix for just in case
        /// </summary>
        //[HarmonyPrefix]
        //public static void GraphicForHeadPreFix(Pawn pawn)
        //{
        //    if (pawn == null || !pawn.RaceProps.Humanlike)
        //    {
        //        return;
        //    }
        //    if (pawn.IsDessicated())
        //    {
        //        return;
        //    }

        //    BodySnatcherExtension modExtension = Utility_BodySnatcherExtensionManager.GetBodySnatcherExtension(pawn);

        //    if (modExtension == null)
        //    {
        //        return;
        //    }

        //    if (!modExtension.invisibleHead && modExtension.headTypeDef == null)
        //    {
        //        return;
        //    }

        //    if (modExtension.invisibleHead)
        //    {
        //        pawn.story.headType = Utility_HeadTypeDefManagement.Named("EMWH_Invisible");
        //        return;
        //    }

        //    bool flag = false;
        //    if (pawn.story.headType == null)
        //    {
        //        pawn.story.headType = Utility_HeadTypeDefManagement.Named("Male_AverageNormal");
        //    }
        //    HeadTypeDef original = pawn.story.headType;

        //    if (modExtension != null && GeneValidator(pawn, flag, modExtension) == true && modExtension.headTypeDef != null)
        //    {
        //        List<HeadTypeDef> headtypedefListForReading = DefDatabase<HeadTypeDef>.AllDefsListForReading;
        //        foreach (HeadTypeDef headType in headtypedefListForReading)
        //        {
        //            if (headType == modExtension.headTypeDef)
        //            {
        //                flag = true;
        //            }
        //        }

        //        if (flag == true)
        //        {
        //            pawn.story.headType = modExtension.headTypeDef;
        //        }
        //        else
        //        {
        //            Log.Warning("Seleted headType does not exist. Set original headtype.");
        //            pawn.story.headType = original;
        //            return;
        //        }
        //    }
        //}

        /// <summary>
        /// Old HumanlikeHeadWidthForPawnPostFix for just in case
        /// </summary>
        //[HarmonyPostfix]
        //public static void HumanlikeHeadWidthForPawnPostFix(Pawn pawn, ref float __result)
        //{
        //    if (!pawn.RaceProps.Humanlike)
        //    {
        //        return;
        //    }
        //    float multiplier = 1f;
        //    BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
        //    if (modExtension == null)
        //    {
        //        return;
        //    }
        //    if (modExtension != null)
        //    {
        //        if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.useVEFCoreScaler)
        //        {
        //            return;
        //        }

        //        if (modExtension.headdrawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
        //        {
        //            return;
        //        }
        //        if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.headdrawSize.x > 1.3f)
        //        {
        //            modExtension.headdrawSize.x = 1.3f;
        //        }
        //        multiplier = modExtension.headdrawSize.x;
        //    }
        //    __result *= multiplier;
        //}

        /// <summary>
        /// Old HumanlikeBodyWidthForPawnPostFix for just in case
        /// </summary>
        //[HarmonyPostfix]
        //public static void HumanlikeBodyWidthForPawnPostFix(Pawn pawn, ref float __result)
        //{
        //    if (!pawn.RaceProps.Humanlike)
        //    {
        //        return;
        //    }

        //    float multiplier = 1f;
        //    BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
        //    if (modExtension == null)
        //    {
        //        return;
        //    }
        //    if (modExtension != null)
        //    {
        //        if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.useVEFCoreScaler)
        //        {
        //            return;
        //        }

        //        if (modExtension.drawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
        //        {
        //            return;
        //        }
        //        if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.drawSize.x > 1.3f)
        //        {
        //            modExtension.drawSize.x = 1.3f;
        //        }
        //        multiplier = modExtension.drawSize.x;
        //    }

        //    __result *= multiplier;
        //}

        /// <summary>
        /// Old GetHumanlikeHairSetForPawnPostFix for just in case
        /// </summary>
        //[HarmonyPostfix]
        //public static void GetHumanlikeHairSetForPawnPostFix(Pawn pawn, ref GraphicMeshSet __result)
        //{
        //    if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
        //    {
        //        return;
        //    }

        //    if (pawn.health.State == PawnHealthState.Dead || pawn.Dead)
        //    {
        //        return;
        //    }

        //    if (pawn.story.hairDef == HairDefOf.Bald || pawn.story.hairDef == null)
        //    {
        //        return;
        //    }

        //    float modifier = 1f;
        //    BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);

        //    if (modExtension == null)
        //    {
        //        return;
        //    }

        //    if (modExtension != null)
        //    {
        //        if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.useVEFCoreScaler)
        //        {
        //            return;
        //        }

        //        if (modExtension.headdrawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
        //        {
        //            return;
        //        }
        //        if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.headdrawSize.x > 1.3f)
        //        {
        //            modExtension.headdrawSize.x = 1.3f;
        //        }
        //        modifier = modExtension.headdrawSize.x;
        //    }
        //    if (pawn.story.headType.hairMeshSize == null)
        //    {
        //        pawn.story.headType.hairMeshSize = new Vector2(1.5f, 1.5f);
        //    }

        //    Vector2 vector = pawn.story.headType.hairMeshSize * modifier;
        //    if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
        //    {
        //        vector *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
        //    }

        //    __result = MeshPool.GetMeshSetForSize(vector.x, vector.y);
        //}

        /// <summary>
        /// Old GetHumanlikeBeardSetForPawnPostFix for just in case
        /// </summary>
        //[HarmonyPostfix]
        //public static void GetHumanlikeBeardSetForPawnPostFix(Pawn pawn, ref GraphicMeshSet __result)
        //{
        //    if (!pawn.RaceProps.Humanlike)
        //    {
        //        return;
        //    }

        //    if (pawn.health.State == PawnHealthState.Dead || pawn.Dead)
        //    {
        //        return;
        //    }

        //    if (pawn.IsDessicated())
        //    {
        //        return;
        //    }

        //    if (pawn.style.beardDef == BeardDefOf.NoBeard || pawn.style.beardDef == null)
        //    {
        //        return;
        //    }

        //    float modifier = 1f;
        //    BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);

        //    if (modExtension == null)
        //    {
        //        return;
        //    }
        //    if (modExtension != null)
        //    {
        //        if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.useVEFCoreScaler)
        //        {
        //            return;
        //        }

        //        if (modExtension.headdrawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
        //        {
        //            return;
        //        }
        //        if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.headdrawSize.x > 1.3f)
        //        {
        //            modExtension.headdrawSize.x = 1.3f;
        //        }
        //        modifier = modExtension.headdrawSize.x;
        //    }
        //    if (pawn.story.headType.hairMeshSize == null)
        //    {
        //        pawn.story.headType.hairMeshSize = new Vector2(1.5f, 1.5f);
        //    }

        //    Vector2 vector = pawn.story.headType.beardMeshSize * modifier;
        //    if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
        //    {
        //        vector *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
        //    }

        //    __result = MeshPool.GetMeshSetForSize(vector.x, vector.y);
        //}

        /// <summary>
        /// Old ScaleForEyesPostFix for just in case
        /// </summary>
        //[HarmonyPostfix]
        //public static void ScaleForEyesPostFix(PawnRenderNode node, PawnDrawParms parms, ref Vector3 __result)
        //{
        //    if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(parms.pawn))
        //    {
        //        return;
        //    }

        //    BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(parms.pawn);
        //    Vector2 drawSize = new Vector2();

        //    if (modExtension == null)
        //    {
        //        return;
        //    }

        //    if (modExtension.invisibleHead)
        //    {
        //        return;
        //    }

        //    if (modExtension == null || modExtension.drawSize.x < 0)
        //    {
        //        return;
        //    }

        //    if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.useVEFCoreScaler)
        //    {
        //        return;
        //    }

        //    if (modExtension.drawSize == null || !Utility_GeneManager.GeneValidator(parms.pawn, modExtension))
        //    {
        //        return;
        //    }

        //    drawSize = modExtension.drawSize;

        //    if (modExtension.headdrawSize.x > 0f)
        //    {
        //        drawSize = modExtension.headdrawSize;
        //    }

        //    if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && drawSize.x > 1.3f)
        //    {
        //        drawSize.x = 1.3f;
        //    }

        //    Vector3 originalSize = __result;
        //    __result = new Vector3(originalSize.x * drawSize.x, originalSize.y * drawSize.x, originalSize.z * drawSize.x);
        //}

        /// <summary>
        /// Old OffsetForEyesPostFix for just in case
        /// </summary>
        [HarmonyPostfix]
        public static void OffsetForEyesPostFix(PawnRenderNode node, PawnDrawParms parms, ref Vector3 __result)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(parms.pawn))
            {
                return;
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(parms.pawn);

            if (modExtension == null)
            {
                return;
            }

            if (modExtension.invisibleHead)
            {
                return;
            }

            if (modExtension == null || modExtension.drawSize.x < 0)
            {
                return;
            }

            if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                return;
            }

            if (modExtension.drawSize == null || !Utility_GeneManager.GeneValidator(parms.pawn, modExtension))
            {
                return;
            }

            float multiplier = 1f;

            multiplier = modExtension.drawSize.x;
            if (modExtension.headdrawSize != null && modExtension.headdrawSize.x > 0f)
            {
                multiplier = modExtension.headdrawSize.x;
            }

            Vector3 originalSize = __result;
            __result = new Vector3(originalSize.x * multiplier, originalSize.y * multiplier, originalSize.z * multiplier);
        }

        /// <summary>
        /// Old SpawnFleckPreFix for just in case
        /// </summary>
        //[HarmonyPrefix]
        //public static void SpawnFleckPreFix(LocalTargetInfo target, CompAbilityEffect_FleckOnTarget __instance)
        //{
        //    if (__instance == null)
        //    {
        //        return;
        //    }

        //    if (!target.HasThing || target == null || target.Pawn == null || !target.Pawn.RaceProps.Humanlike)
        //    {
        //        return;
        //    }

        //    float modifier = 1f;
        //    BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(target.Pawn);

        //    if (modExtension == null)
        //    {
        //        return;
        //    }

        //    if (modExtension != null)
        //    {
        //        if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.useVEFCoreScaler)
        //        {
        //            return;
        //        }

        //        if (modExtension.drawSize == null || !Utility_GeneManager.GeneValidator(target.Pawn, modExtension))
        //        {
        //            return;
        //        }
        //        if (!ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") && modExtension.drawSize.x > 1.3f)
        //        {
        //            modExtension.drawSize.x = 1.3f;
        //        }
        //        modifier = modExtension.drawSize.x;
        //    }
        //    __instance.Props.scale = modifier;
        //}

        [HarmonyPostfix]
        public static void HumanlikeHeadWidthForPawnPostFix(Pawn pawn, ref float __result)
        {
            if (!pawn.RaceProps.Humanlike)
                return;

            // Grab your body-snatcher extension once
            var modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);

            if (modExtension == null)
            {
                return;
            }            

            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                // ─── VFE is enabled: use *old* drawSize logic ────────────
                float multiplier = 1f;

                // Only apply if drawSize set and gene rules permit
                if (modExtension.drawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
                {
                    return;
                }

                multiplier = modExtension.drawSize.x;
                if (modExtension.headdrawSize != null && modExtension.headdrawSize.x > 0f)
                {
                    multiplier = modExtension.headdrawSize.x;
                }

                __result *= multiplier;
            }
            else
            {
                // ─── VFE is **not** enabled: use your *new* head-scaling logic ───
                float scaling = Utility_BodySnatcherManager.GetScaling(pawn, isHeadPart: true);
                __result *= scaling;
            }
        }

        [HarmonyPostfix]
        public static void HumanlikeBodyWidthForPawnPostFix(Pawn pawn, ref float __result)
        {
            if (!pawn.RaceProps.Humanlike)
                return;

            // Grab your body-snatcher extension once
            var modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);

            if (modExtension == null)
            {
                return;
            }

            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                // ─── VFE is enabled: use *old* drawSize logic ────────────
                float multiplier = 1f;

                // Only apply if drawSize set and gene rules permit
                if (modExtension.drawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
                {
                    return;
                }

                multiplier = modExtension.drawSize.x;

                __result *= multiplier;
            }
            else
            {
                float scaling = Utility_BodySnatcherManager.GetScaling(pawn, isHeadPart: false);
                __result *= scaling;
            }
        }

        [HarmonyPostfix]
        public static void GetHumanlikeHairSetForPawnPostFix(Pawn pawn, ref GraphicMeshSet __result)
        {
            // skip if no hair or dessicated
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
                return;

            var modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
            if (modExtension == null)
                return;
            if (modExtension.headdrawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
                return;
            if (pawn.story != null && modExtension.invisibleHead && modExtension.noHair)
            {
                // only switch to Bald if not already set, so you don’t stomp user changes repeatedly
                if (pawn.story.hairDef == null || pawn.story.hairDef != HairDefOf.Bald)
                {
                    pawn.story.hairDef = HairDefOf.Bald;
                }
            }

            // when VFE Core is active: fall back to your old headdrawSize logic
            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                float multiplier = 1f;
                
                multiplier = modExtension.headdrawSize.x;

                // determine base hair‐mesh size
                if (pawn.story.headType.hairMeshSize == null)
                {
                    pawn.story.headType.hairMeshSize = new Vector2(1.5f, 1.5f);
                }

                Vector2 baseSize = pawn.story.headType.hairMeshSize;

                // apply life‐stage headSizeFactor
                if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
                    multiplier *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;

                Vector2 size = baseSize * multiplier;
                __result = MeshPool.GetMeshSetForSize(size.x, size.y);
            }
            else
            {
                // when VFE Core is NOT active: use your new uniform scaling
                float scaling = Utility_BodySnatcherManager.GetScaling(pawn, isHeadPart: true);

                // start from beardMeshSize (which is hair mesh for head)
                if (pawn.story.headType.hairMeshSize == null)
                {
                    pawn.story.headType.hairMeshSize = new Vector2(1.5f, 1.5f);
                }

                Vector2 baseSize = pawn.story.headType.hairMeshSize;

                // biotech headSizeFactor
                if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
                    baseSize *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;

                Vector2 finalSize = baseSize * scaling;
                __result = MeshPool.GetMeshSetForSize(finalSize.x, finalSize.y);
            }
        }

        [HarmonyPostfix]
        public static void GetHumanlikeBeardSetForPawnPostFix(Pawn pawn, ref GraphicMeshSet __result)
        {
            // skip non‐valid pawns
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
                return;
            if (pawn.style.beardDef == BeardDefOf.NoBeard || pawn.style.beardDef == null)
                return;

            // ── VFE Core enabled? use old headdrawSize logic ─────────────────
            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                var modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);
                if (modExtension == null || modExtension.useVEFCoreScaler)
                    return;
                if (modExtension.headdrawSize == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
                    return;

                float modifier = 1f;

                modifier = modExtension.headdrawSize.x;

                if (pawn.story.headType.hairMeshSize == null)
                {
                    pawn.story.headType.hairMeshSize = new Vector2(1.5f, 1.5f);
                }

                Vector2 baseSize = pawn.story.headType.hairMeshSize;

                if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
                    modifier *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;

                Vector2 size = baseSize * modifier;
                __result = MeshPool.GetMeshSetForSize(size.x, size.y);
            }
            else
            {
                // ── VFE Core disabled? use new uniform scaling ──────────────────
                float scaling = Utility_BodySnatcherManager.GetScaling(pawn, isHeadPart: true);

                if (pawn.story.headType.hairMeshSize == null)
                {
                    pawn.story.headType.hairMeshSize = new Vector2(1.5f, 1.5f);
                }

                Vector2 baseSize = pawn.story.headType.hairMeshSize;

                Vector2 finalSize = baseSize * scaling;

                if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
                    finalSize *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;

                __result = MeshPool.GetMeshSetForSize(finalSize.x, finalSize.y);
            }
        }

        [HarmonyPostfix]
        public static void ScaleForEyesPostFix(PawnRenderNode node, PawnDrawParms parms, ref Vector3 __result)
        {
            // ─── 1) Only valid, non-dessicated humanlike pawns ────────────────────
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(parms.pawn))
                return;

            // ─── 2) If VFE Core is active, run the “old” drawSize logic ───────────
            if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core"))
            {
                var modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(parms.pawn);
                if (modExtension == null)
                    return;

                // Don’t scale invisible-head pawns or those opting into VFE’s own scaler
                if (modExtension.invisibleHead || modExtension.useVEFCoreScaler)
                    return;

                // Require a positive drawSize and valid genes
                if (modExtension.drawSize == null || modExtension.drawSize.x < 0f
                    || !Utility_GeneManager.GeneValidator(parms.pawn, modExtension))
                    return;

                // Use head-specific override if set
                float multiplier = modExtension.drawSize.x;
                if (modExtension.headdrawSize.x > 0f)
                    multiplier = modExtension.headdrawSize.x;

                // Apply life-stage head size factor
                if (ModsConfig.BiotechActive
                    && parms.pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
                {
                    multiplier *= parms.pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
                }

                // ─── Injected: multiply the original size by the old-logic multiplier ───
                Vector3 orig = __result;
                __result = new Vector3(orig.x * multiplier, orig.y * multiplier, orig.z * multiplier);
            }
            else
            {
                // ─── 3) If VFE Core is not active, use your new uniform head scaling ────
                float scaling = Utility_BodySnatcherManager.GetScaling(parms.pawn, isHeadPart: true);
                Vector3 orig = __result;
                __result = new Vector3(orig.x * scaling, orig.y * scaling, orig.z * scaling);
            }
        }

        //[HarmonyPostfix]
        //public static void OffsetForEyesPostFix(PawnRenderNode node, PawnDrawParms parms, ref Vector3 __result)
        //{
        //    if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(parms.pawn))
        //    {
        //        return;
        //    }

        //    float scaling = Utility_BodySnatcherManager.GetScaling(parms.pawn, isHeadPart: true);

        //    Vector3 originalSize = __result;
        //    __result = new Vector3(originalSize.x * scaling, originalSize.y * scaling, originalSize.z * scaling);
        //}

        [HarmonyPrefix]
        public static void SpawnFleckPreFix(LocalTargetInfo target, CompAbilityEffect_FleckOnTarget __instance)
        {
            if (__instance == null || target.Pawn == null || !target.Pawn.RaceProps.Humanlike)
            {
                return;
            }

            float scaling = Utility_BodySnatcherManager.GetScaling(target.Pawn, isHeadPart: false);
            __instance.Props.scale *= scaling;
        }

        [HarmonyPostfix]
        public static void GetXenotypeForGeneratedPawnPostfix(PawnGenerationRequest request, ref XenotypeDef __result)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            BodySnatcherExtension modExtension = request.KindDef.GetModExtension<BodySnatcherExtension>();

            if (modExtension != null)
            {
                return;
            }

            if (modExtension == null)
            {
                if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CH.CD"))
                {
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_PinkHorror")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_BlueHorror")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_Changecaster")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_Fateskimmer")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_Fluxmaster")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_Flamer")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_ExaltedFlamer")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_Changeling")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMCD_LordofChange")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                }
                if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.GC.Core"))
                {
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMGC_PurestrainGenestealer")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMGC_Patriarch")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                }
                if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.NC.Core"))
                {
                    if (__result == (Utility_XenotypeManager.XenotypeDefNamed("EMNC_Necrons")))
                    {
                        __result = XenotypeDefOf.Baseliner;
                    }
                }
            }
        }
    }

    public static class EMWH_ApparelRestrictionPatch
    {
        //    [HarmonyPostfix]
        //    public static void PawnCanWearbyTraitPostfix(Apparel __instance, Pawn pawn, ref bool __result)
        //    {
        //        if (!__result || !__instance.def.IsApparel)
        //        {
        //            return;
        //        }
        //        __result = Utility_ApparelRestriction.PawnCanWearbyTrait(__instance, pawn, __result);
        //    }

        //    [HarmonyPostfix]
        //    public static void PawnCanWearbyArmorPostfix(Apparel __instance, Pawn pawn, ref bool __result)
        //    {
        //        if (!__result || !__instance.def.IsApparel)
        //        {
        //            return;
        //        }
        //        __result = Utility_ApparelRestriction.PawnCanWearbyArmor(__instance, pawn, __result);
        //    }



        //[HarmonyPostfix]
        //public static void PawnCanWearbyapparelTagsPostfix(Apparel __instance, Pawn pawn, ref bool __result)
        //{
        //    if (!__result || !__instance.def.IsApparel)
        //    {
        //        return;
        //    }
        //    __result = Utility_ApparelRestriction.PawnCanWearbyapparelTags(__instance, pawn, __result);
        //}



        //    [HarmonyPostfix]
        //    public static void PawnCanWearbyHediffPostfix(Apparel __instance, Pawn pawn, ref bool __result)
        //    {
        //        if (!__result || !__instance.def.IsApparel)
        //        {
        //            return;
        //        }
        //        __result = Utility_ApparelRestriction.PawnCanWearbyHediff(__instance, pawn, __result);
        //    }

        //    [HarmonyPostfix]
        //    public static void PawnCannotWearbyRacePostfix(Apparel __instance, Pawn pawn, ref bool __result)
        //    {
        //        EMWH_ApparelRestriction_ValidatiorExtension modExtension = pawn.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();

        //        if (modExtension == null)
        //        {
        //            return;
        //        }
        //        if (!__result || !__instance.def.IsApparel)
        //        {
        //            return;
        //        }
        //        __result = Utility_ApparelRestriction.PawnCannotWearbyRace(__instance, pawn, __result);
        //    }

        //    [HarmonyPostfix]
        //    public static void PawnCanWearbyRacePostfix(Apparel __instance, Pawn pawn, ref bool __result)
        //    {
        //        if (!__result || !__instance.def.IsApparel)
        //        {
        //            return;
        //        }
        //        __result = Utility_ApparelRestriction.PawnCanWearbyRace(__instance, pawn, __result);
        //    }

        [HarmonyPrefix]
        public static bool PawnApparelTrackerWearPrefix(Pawn_ApparelTracker __instance, Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
        {
            Pawn pawn = __instance.pawn;

            // Ensure the pawn is already spawned
            if (!pawn.Spawned)
            {
                // Skip logic for non-spawned pawns
                return true; // Continue with vanilla behavior
            }

            // Run the PawnCanWearbyapparelTags check for the new apparel
            if (!Utility_ApparelRestriction.PawnCanWearbyapparelTags(newApparel, pawn))
            {
                // If the apparel cannot be worn (due to the logic in PawnCanWearbyapparelTags), we drop or unequip conflicting apparel
                if (dropReplacedApparel)
                {
                    // Try to drop any conflicting apparel
                    foreach (Apparel existingApparel in pawn.apparel.WornApparel)
                    {
                        if (ApparelUtility.CanWearTogether(newApparel.def, existingApparel.def, pawn.RaceProps.body) == false)
                        {
                            // Drop or remove conflicting apparel
                            if (!__instance.TryDrop(existingApparel, out _))
                            {
                                Log.Error($"Could not drop {existingApparel} for pawn {pawn.Name}");
                            }
                        }
                    }
                }
                else
                {
                    // If we don't want to drop, simply remove conflicting apparel
                    foreach (Apparel existingApparel in pawn.apparel.WornApparel)
                    {
                        if (ApparelUtility.CanWearTogether(newApparel.def, existingApparel.def, pawn.RaceProps.body) == false)
                        {
                            __instance.Remove(existingApparel);
                        }
                    }
                }

                // Return false to prevent the new apparel from being worn
                return false;
            }

            // If the apparel can be worn (conditions are met), continue with vanilla logic
            return true;
        }

        [HarmonyPrefix]
        public static bool GiveApparelToPawnPrefix(Pawn pawn, Precept_Apparel precept)
        {
            IdeoApparelPreventExtension modExtension = pawn.kindDef.GetModExtension<IdeoApparelPreventExtension>();

            if (modExtension != null)
            {
                return false;
            }

            return true;
        }
    }

    public static class EMWH_EquipmentRestrictionPatch
    {        
        [HarmonyPostfix]
        public static void PawnCanEquipbyTraitPostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || !thing.def.IsApparel)
            {
                return;
            }
            __result = Utility_ApparelRestriction.PawnCanEquipbyTrait(thing, pawn, ref cantReason);
        }

        [HarmonyPostfix]
        public static void PawnCanEquipbyArmorPostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || !thing.def.IsApparel)
            {
                return;
            }
            __result = Utility_ApparelRestriction.PawnCanEquipbyArmor(thing, pawn, ref cantReason);
        }

        [HarmonyPostfix]
        public static void PawnCanEquipbyapparelTagsPostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || !thing.def.IsApparel)
            {
                return;
            }
            __result = Utility_ApparelRestriction.PawnCanEquipbyapparelTags(thing, pawn, ref cantReason);
        }

        [HarmonyPostfix]
        public static void PawnCanEquipbyHediffPostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || !thing.def.IsApparel)
            {
                return;
            }
            __result = Utility_ApparelRestriction.PawnCanEquipbyHediff(thing, pawn, ref cantReason);
        }

        [HarmonyPostfix]
        public static void PawnCannotEquipbyRacePostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            EMWH_ApparelRestriction_ValidatiorExtension modExtension = pawn.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();

            if (modExtension == null)
            {
                return;
            }
            if (!__result || !thing.def.IsApparel)
            {
                return;
            }
            __result = Utility_ApparelRestriction.PawnCannotEquipbyRace(thing, pawn, ref cantReason);
        }

        [HarmonyPostfix] 
        public static void PawnCanEquipbyRacePostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || !thing.def.IsApparel)
            {
                return;
            }
            __result = Utility_ApparelRestriction.PawnCanEquipbyRace(thing, pawn, ref cantReason);
        }

        [HarmonyPostfix]
        public static void ApparelScoreGainPostfix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (!EquipmentUtility.CanEquip(ap, pawn))
                __result = -1000f;
        }

        //[HarmonyPostfix]
        //public static void CanUsePairPostfix(ThingStuffPair pair, Pawn pawn, ref bool __result)
        //{
        //    if (!__result || !ApparelRestrictionValidator(pair.thing, pawn))
        //    {
        //        return;
        //    }
        //    string cantReason = null;
        //    if (!Utility_ApparelRestriction.PawnCanEquipbyTrait(ThingMaker.MakeThing(pair.thing), pawn, ref cantReason, __result))
        //    {
        //        __result = false;
        //    }
        //    if (!Utility_ApparelRestriction.PawnCanEquipbyArmor(ThingMaker.MakeThing(pair.thing), pawn, ref cantReason, __result))
        //    {
        //        __result = false;
        //    }
        //    if (!Utility_ApparelRestriction.PawnCanEquipbyapparelTags(ThingMaker.MakeThing(pair.thing), pawn, ref cantReason, __result))
        //    {
        //        __result = false;
        //    }
        //    if (!Utility_ApparelRestriction.PawnCanEquipbyRace(ThingMaker.MakeThing(pair.thing), pawn, ref cantReason, __result))
        //    {
        //        __result = false;
        //    }
        //    if (!Utility_ApparelRestriction.PawnCannotEquipbyRace(ThingMaker.MakeThing(pair.thing), pawn, ref cantReason, __result))
        //    {
        //        __result = false;
        //    }
        //}

        //private static bool ApparelRestrictionValidator(ThingDef thingDef, Pawn pawn)
        //{
        //    bool flag = true;

        //    ApparelRestrictionbyTraitsExtension modExtension = thingDef.GetModExtension<ApparelRestrictionbyTraitsExtension>();
        //    ApparelRestrictionbyArmorsExtension modExtension2 = thingDef.GetModExtension<ApparelRestrictionbyArmorsExtension>();
        //    ApparelRestrictionbyapparelTagsExtension modExtension3 = thingDef.GetModExtension<ApparelRestrictionbyapparelTagsExtension>();
        //    ApparelRestrictionbyRaceExtension modExtension4 = thingDef.GetModExtension<ApparelRestrictionbyRaceExtension>();
        //    EMWH_ApparelRestriction_ValidatiorExtension modExtension5 = pawn.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();
        //    EMWH_ApparelRestriction_ValidatiorExtension modExtension6 = thingDef.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();
        //    if (modExtension == null &&  modExtension2 == null && modExtension3 == null && modExtension4 == null && modExtension5 == null && modExtension6 == null)
        //    {
        //        flag = false; 
        //    }

        //    return flag;
        //}
    }

    public static class EMWH_GeneRestrictionPatch
    {
        [HarmonyPrefix]
        public static bool AddNecronGenePrefix(Gene gene, Pawn ___pawn)
        {
            if (!ModsConfig.BiotechActive)
            {
                return true;
            }

            if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.NC.Core"))
            {
                return true;
            }

            return Utility_GeneRestriction.CanHaveNecronGene(gene.def, ___pawn.def);
        }        

        [HarmonyPrefix]
        public static bool AddGenetoKindPrefix(Gene gene, Pawn ___pawn)
        {
            if (!ModsConfig.BiotechActive)
            {
                return true;
            }

            if (gene == null)
            {
                return true;
            }

            return Utility_GeneRestriction.CanAddGenetoPawnKind(gene.def, ___pawn);
        }
    }

    public static class EMWH_WeaponRestrictionPatch
    {
        [HarmonyPostfix]
        public static void PawnCannotEquipbyPostfix(Thing thing, Pawn pawn, ref string cantReason, ref bool __result)
        {
            if (!__result || !thing.def.IsWeapon)
            {
                return;
            }
            __result = Utility_WeaponRestriction.PawnCannotEquipby(thing, pawn, ref cantReason);
        }
    }

    public static class EMWH_AbilityRestrictionPatch
    {
        [HarmonyPrefix]
        public static bool GainAbilityPrefix(AbilityDef def, Pawn_AbilityTracker __instance)
        {
            if (def == null || __instance == null)
            {
                return true; // Skip further processing if any is null
            }

            AbilityRestrictionExtension modExtension = def.GetModExtension<AbilityRestrictionExtension>();

            if (modExtension == null)
            {
                return true; // Allow the original method to run if there's no restriction
            }

            if (AbilityRequirementValidator(modExtension, __instance))
            {
                return true; // Allow the original method to run if the ability passes the validator
            }

            GainAlternativeAbility(modExtension, __instance);

            return false;
        }
 
        private static bool AbilityRequirementValidator(AbilityRestrictionExtension modExtension, Pawn_AbilityTracker __instance)
        {
            if (ModsConfig.BiotechActive && modExtension.requiredGenes != null)
            {
                List<Gene> genesListForReading = __instance.pawn.genes.GenesListForReading;
                foreach (Gene gene in genesListForReading)
                {
                    if (modExtension.requiredGenes.Contains(gene.def))
                        return true;
                }
            }
            if (ModsConfig.BiotechActive && modExtension.requiredXenotypes != null)
            {
                if (modExtension.requiredXenotypes.Contains(__instance.pawn.genes.Xenotype))
                    return true;
            }
            if (modExtension.requiredHediffs != null)
            {
                foreach (Hediff hediff in __instance.pawn.health.hediffSet.hediffs)
                {
                    if (modExtension.requiredHediffs.Contains(hediff.def))
                        return true;
                }
            }
            return false;
        }

        private static void GainAlternativeAbility(AbilityRestrictionExtension modExtension, Pawn_AbilityTracker __instance)
        {
            AbilityDef def;
            List<AbilityDef> allAbilities = DefDatabase<AbilityDef>.AllDefsListForReading;
            List<AbilityDef> possibleAlternative = new List<AbilityDef>();
            foreach (AbilityDef abilityDef in allAbilities)
            {
                if (abilityDef.IsPsycast)
                {
                    if (abilityDef.HasModExtension<AbilityRestrictionExtension>())
                    {
                        if (AbilityRequirementValidator(modExtension, __instance))
                            possibleAlternative.Add(abilityDef);
                    }
                    else
                        possibleAlternative.Add(abilityDef);
                }
            }
            int rand = Rand.RangeInclusive(0, possibleAlternative.Count - 1);
            def = possibleAlternative[rand];
            __instance.GainAbility(def);
            __instance.Notify_TemporaryAbilitiesChanged();
        }
 
        [HarmonyPrefix]
        public static bool DoEffectPrefix(Pawn user, CompUseEffect_GainAbility __instance)
        {
            AbilityRestrictionExtension modExtension = __instance.Props.ability.GetModExtension<AbilityRestrictionExtension>();
            if (modExtension == null)
                return true;
            if (ModsConfig.BiotechActive && modExtension.requiredGenes != null)
            {
                List<Gene> genesListForReading = user.genes.GenesListForReading;
                foreach (Gene gene in genesListForReading)
                {
                    if (modExtension.requiredGenes.Contains(gene.def))
                        return true;
                }
            }
            if (ModsConfig.BiotechActive && modExtension.requiredXenotypes != null)
            {
                if (modExtension.requiredXenotypes.Contains(user.genes.Xenotype))
                    return true;
            }
            if (modExtension.requiredHediffs != null)
            {
                foreach (Hediff hediff in user.health.hediffSet.hediffs)
                {
                    if (modExtension.requiredHediffs.Contains(hediff.def))
                        return true;
                }
            }
            Messages.Message("MessageAbilityCannotGain".Translate(user.LabelShort, user.Named("PAWN1")), MessageTypeDefOf.RejectInput, historical: false);
            return false;
        }
    }

    public static class EMWH_DamageWorkerPatch
    {
        [HarmonyPostfix]
        public static void ApplyPostfix(DamageInfo dinfo, Thing victim)
        {
            DamageImmunityExtension modExtension = dinfo.Def.GetModExtension<DamageImmunityExtension>();
            if (modExtension == null)
                return;
            if (modExtension != null && modExtension.hediffs != null)
            {
                foreach (HediffDef hediffDef in modExtension.hediffs)
                {
                    if (victim is Pawn)
                    {
                        Pawn p = victim as Pawn;
                        if (!p.Spawned || p.health.immunity.AnyGeneMakesFullyImmuneTo(hediffDef) || p.health.immunity.ImmunityRecordExists(hediffDef) == true)
                            return;
                        if (modExtension.exposureStatFactor != null)
                            IngestHediffToPawn(hediffDef, p, modExtension.severityAdjustment, modExtension.exposureStatFactor);
                        else
                            IngestHediffToPawn(hediffDef, p, modExtension.severityAdjustment);
                    }
                }
            }
        }

        private static void IngestHediffToPawn(HediffDef hediffDef, Pawn pawn, float severityAdjustment, StatDef exposureStatFactor = null)
        {
            if (exposureStatFactor != null)
            {
                severityAdjustment *= 1f - pawn.GetStatValue(exposureStatFactor);
            }
            if ((double)severityAdjustment != 0.0)
            {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null)
                    pawn.health.AddHediff(hediffDef);
                else
                    HealthUtility.AdjustSeverity(pawn, hediffDef, severityAdjustment);
            }
        }
    }

    public static class EMWH_ThoughtPatch
    {
        [HarmonyPostfix]
        public static void WitnessedPostfix(Pawn victim, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            EMNC_Necron_ValidatiorExtension modExtension = victim.def.GetModExtension<EMNC_Necron_ValidatiorExtension>();
            if (modExtension == null)
            {
                return;
            }
            if (modExtension != null)
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        public static void HasUncoveredGroinPostfix(Pawn p, ref bool __result)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            ThoughtRestrictionExtension modExtension = p.def.GetModExtension<ThoughtRestrictionExtension>();
            if (modExtension == null)
            {
                return;
            }

            if (modExtension != null && modExtension.isUncoveredGroin)
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        public static void CanAutoUsePostfix(Pawn pawn, ref bool __result)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            ThoughtRestrictionExtension modExtension = pawn.def.GetModExtension<ThoughtRestrictionExtension>();
            if (modExtension == null)
            {
                return;
            }

            if (modExtension != null && modExtension.isNeuralSupercharger)
            {
                if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.NC.Core") && (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMNC_Biotransference_Lord")) != null || pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMNC_Biotransference_Cryptek")) != null))
                {
                    __result = true;
                }
                else
                { 
                    __result = false; 
                }
            }
        }

        [HarmonyPostfix]
        public static void CanHaveThoughtAgeReversalPostfix(Pawn pawn, ref bool __result)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            ThoughtRestrictionExtension modExtension = pawn.def.GetModExtension<ThoughtRestrictionExtension>();
            if (modExtension == null)
            {
                return;
            }

            if (modExtension != null && modExtension.isAgeReversal)
            {
                __result = false;
            }
        }
    }

    public static class EMWH_MassPatch
    {
        [HarmonyPostfix]
        public static void CapacityPostfix(Pawn p, StringBuilder explanation, ref float __result)
        {
            if (p == null || p.Dead || p.IsDessicated())
            {
                return;
            }

            if (__result > p.BodySize * 35f)
            {
                return;
            }

            float modifier = 1f;
            float additionalCarry = 1f;

            if (p.RaceProps.Humanlike)
            {
                BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(p);
                MassCarriedExtension modExtension2 = Utility_GetModExtension.GetMassCarriedExtension(p);

                if (modExtension == null && modExtension2 == null)
                {
                    return;
                }

                if (modExtension != null)
                {
                    if (modExtension.drawSize == null || modExtension.drawSize.x <= 1f)
                    {
                        modifier = 1f;
                    }
                    else
                    {
                        modifier = modExtension.drawSize.x;
                    }

                    __result = Utility_MassCalculation.CarryMassCalculation(p, __result, modifier);
                }

                if (modExtension2 != null)
                {
                    if (modExtension2.massCarryBuff > 0f)
                    {
                        additionalCarry = __result + modExtension2.massCarryBuff;
                    }
                    else
                    {
                        additionalCarry = __result;
                    }

                    __result = additionalCarry;
                }
            }
            else if (p.RaceProps.packAnimal)
            {
                BodySnatcherExtension modExtension = p.def.GetModExtension<BodySnatcherExtension>();
                MassCarriedExtension modExtension2 = Utility_GetModExtension.GetMassCarriedExtension(p);

                if (modExtension == null && modExtension2 == null)
                {
                    return;
                }

                if (modExtension != null)
                {
                    if (modExtension.animalCarryMassModifier <= 0)
                    {
                        modifier = 1f;
                    }
                    else
                    {
                        modifier = modExtension.animalCarryMassModifier;
                    }

                    __result = Utility_MassCalculation.CarryMassCalculation(p, __result, modifier);
                }

                if (modExtension2 != null)
                {
                    if (modExtension2.massCarryBuffAnimal > 0f)
                    {
                        additionalCarry = __result + modExtension2.massCarryBuffAnimal;
                    }
                    else
                    {
                        additionalCarry = __result;
                    }

                    __result = additionalCarry;
                }
            }
        }
    }

    public static class EMWH_HealthScalePatch
    {
        [HarmonyPostfix]
        public static void HealthScalePostfix(Pawn __instance, ref float __result)
        {
            if (!__instance.RaceProps.Humanlike)
            {
                return;
            }
            if (__instance.RaceProps.baseHealthScale > 1f)
            {
                return;
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(__instance);

            if (modExtension == null)
            {
                return;
            }
            if (modExtension.drawSize == null)
            {
                modExtension.drawSize.x = 1f;
            }
            __result *= modExtension.drawSize.x;
        }
    }

    public static class EMWH_EmbrasuresAttachmentPatch
    {
        [HarmonyPostfix]
        public static void AllowsPlacingPostfix(ref IntVec3 loc, ref Rot4 rot, ref Map map, ref AcceptanceReport __result)
        {
            if (__result == true)
                return;
            IntVec3 c = loc + GenAdj.CardinalDirections[rot.AsInt];
            if (!c.InBounds(map))
            {
                return;
            }
            List<Thing> thingList = c.GetThingList(map);
            for (int j = 0; j < thingList.Count; j++)
            {
                ThingDef thingDef2 = GenConstruct.BuiltDefOf(thingList[j].def) as ThingDef;
                if (thingDef2 != null && thingDef2.building != null)
                {
                    if (!thingDef2.building.supportsWallAttachments)
                    {
                        return;
                    }
                    else if (thingDef2.Fillage == FillCategory.Full)
                    {
                        return;
                    }
                    else if (thingDef2.Fillage != FillCategory.Full)
                    {
                        EmbrasuresAttachmentExtension modExtension = thingDef2.GetModExtension<EmbrasuresAttachmentExtension>();
                        if (modExtension == null)
                            return;
                        if (modExtension != null && modExtension.canbeAttached)
                            __result = true;
                    }
                }
            }
        }
    }

    public static class EMWH_StartingHediffsPatch
    {
        [HarmonyPostfix]
        public static void AddStartingHediffsPostfix(Pawn pawn, List<StartingHediff> startingHediffs)
        {
            foreach (StartingHediff startingHediff in startingHediffs)
            {
                StartingHediffExtension modExtension = startingHediff.def.GetModExtension<StartingHediffExtension>();
                if (modExtension == null)
                {
                    return;
                }
                Hediff_Level hediffLevel = (Hediff_Level)pawn.health.hediffSet.GetFirstHediffOfDef(startingHediff.def);
                if (hediffLevel == null || !modExtension.setMaxLevel)
                {
                    return;
                }
                hediffLevel.ChangeLevel((int)hediffLevel.def.maxSeverity);
                if (modExtension.setXenotype)
                {
                    HediffComp_GiveXenotype giveXenotypeComp = hediffLevel.GetComp<HediffComp_GiveXenotype>();
                    if (giveXenotypeComp != null)
                    {
                        if (giveXenotypeComp.Props.targetRaceDef != null && pawn.kindDef.race == ThingDefOf.Human)
                        {
                            if (ModsConfig.BiotechActive && pawn.genes.Xenotype == XenotypeDefOf.Baseliner)
                                pawn.kindDef.race = giveXenotypeComp.Props.targetRaceDef;
                        }
                        if (ModsConfig.BiotechActive)
                        {
                            if (pawn.genes.Xenotype == null)
                                return;
                            if (giveXenotypeComp.Props.targetxenotypeDef == null && giveXenotypeComp.Props.targetxenotypeDefs == null)
                                return;
                            if (pawn.genes.Xenotype != XenotypeDefOf.Baseliner)
                            {
                                XenotypeDef targetXenotype;
                                if (giveXenotypeComp.Props.targetxenotypeDefs != null)
                                {
                                    targetXenotype = giveXenotypeComp.XenotypeSelector(giveXenotypeComp.Props.targetxenotypeDefs);
                                }
                                else
                                {
                                    targetXenotype = giveXenotypeComp.Props.targetxenotypeDef;
                                }
                                for (int i = 0; i < targetXenotype.genes.Count; i++)
                                {
                                    pawn.genes.AddGene(targetXenotype.genes[i], true);
                                }
                                pawn.genes.SetXenotype(targetXenotype);
                            }
                            else
                            {
                                if (giveXenotypeComp.Props.targetxenotypeDefs != null)
                                    pawn.genes.SetXenotype(giveXenotypeComp.XenotypeSelector(giveXenotypeComp.Props.targetxenotypeDefs));
                                else
                                    pawn.genes.SetXenotype(giveXenotypeComp.Props.targetxenotypeDef);
                                return;
                            }
                        }
                        else return;
                    }
                }
            }
        }
    }

    public static class EMWH_ApparelUtilityLayerPatch
    {
        [HarmonyPostfix]
        public static void IsUtilityLayerPostfix(ApparelLayerDef __instance, ref bool __result)
        {
            ApparelUtilityLayerExtension modExtension = __instance.GetModExtension<ApparelUtilityLayerExtension>();
            if (modExtension != null && !__result)
            {
                __result = true;
            }
        }
    }

    public static class EMWH_PawnRedressPatch
    {        
        [HarmonyPostfix]
        public static void IsValidCandidateToRedressPostfix(Pawn pawn, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(pawn);

            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.GC.Core"))
            {
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_PurestrainGenestealer") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_PurestrainGenestealer")))
                {
                    if (ModsConfig.BiotechActive && (pawn.genes.Xenotype == Utility_XenotypeManager.XenotypeDefNamed("EMGC_PurestrainGenestealer") || pawn.genes.Xenotype == Utility_XenotypeManager.XenotypeDefNamed("EMGC_Patriarch")))
                    {
                        __result = true;
                    }
                    else
                    {
                        __result = false;
                    }
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_Patriarch") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_Patriarch")))
                {
                    __result = false;
                }
            }

            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CH.CD") && ModsConfig.BiotechActive)
            {
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_PinkHorror") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_PinkHorror")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_BlueHorror") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_BlueHorror")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_Changecaster") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_Changecaster")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_Fateskimmer") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_Fateskimmer")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_Fluxmaster") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_Fluxmaster")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_Flamer") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_Flamer")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_ExaltedFlamer") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_ExaltedFlamer")))
                {
                    __result = false;
                }
                if (modExtension == null && (pawn.story.headType == Utility_HeadTypeDefManagement.Named("Male_ExaltedFlamer") || pawn.story.headType == Utility_HeadTypeDefManagement.Named("Female_ExaltedFlamer")))
                {
                    __result = false;
                }
            }

            if (modExtension == null && pawn.story.headType == Utility_HeadTypeDefManagement.Named("EMWH_Invisible"))
            {
                __result = false;
            }

            if (modExtension == null)
            {
                return;
            }
        }
    }
    
    public static class EMWH_PawnSanitizerPatch
    {
        [HarmonyPrefix]
        public static bool ThoughtBlockerPrefix(ThoughtDef def, Pawn ___pawn)
        {
            if (!LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtBlocking)
            {
                return true;
            }

            if (___pawn?.health?.hediffSet == null)
            {
                return true;
            }

            foreach (var h in ___pawn.health.hediffSet.hediffs)
            {
                var comp = h.TryGetComp<HediffComp_PawnSanitizer>();
                if (comp != null && comp.Props.preventNewThoughts)
                {
                    return false; // Block memory gain
                }
            }

            return true;
        }

        [HarmonyPrefix]
        public static bool ThoughtGainFilterPrefix(ThoughtDef def, Pawn otherPawn, MemoryThoughtHandler __instance)
        {
            if (!LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtFiltering)
                return true;

            return def == null || !Utility_ThoughtBlockerRegistry.IsBlocked(def);
        }

        [HarmonyPrefix]
        public static bool SituationalThoughtBlockerPrefix(ThoughtDef def, ref Thought_Situational __result)
        {
            if (LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtFiltering && Utility_ThoughtBlockerRegistry.IsBlocked(def))
            {
                __result = null;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        public static bool BlockInspirationPrefix(Pawn ___pawn, InspirationDef def)
        {
            if (LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtFiltering && Utility_ThoughtBlockerRegistry.IsBlocked(def))
            {
                return false;
            }
            return true;
        }
        

        [HarmonyPrefix]
        public static bool BlockMentalStatePrefix(Pawn ___pawn, MentalStateDef stateDef)
        {
            if (LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtFiltering && Utility_ThoughtBlockerRegistry.IsBlocked(stateDef))
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        public static void Pawn_Kill_CleanupSanitizerPrefix(Pawn __instance)
        {
            if (__instance == null || __instance.Destroyed)
            { 
                return; 
            }

            // Compatibility: Skip non-humanoids or special pawns
            if (!__instance.RaceProps?.Humanlike ?? true)
            {
                return;
            }

            // Optional: Skip vehicles
            if (__instance.GetType().Namespace?.Contains("Vehicles") == true)
            {
                return;
            }

            var reg = SanitizerTrackerRegistry.Instance;
            if (reg == null)
            {
                return;
            }

            if (reg.Apparel != null)
            {
                try
                {
                    reg.Apparel.ClearForPawn(__instance);
                }
                catch (Exception ex)
                {
                    Utility_ThoughtBlockerRegistry.LogIfDebug($"Error clearing apparel tracker for {__instance.LabelShortCap}: {ex}");
                }
            }

            if (reg.Consumable != null)
            {
                try
                {
                    reg.Consumable.ClearForPawn(__instance);
                }
                catch (Exception ex)
                {
                    Utility_ThoughtBlockerRegistry.LogIfDebug($"Error clearing consumable tracker for {__instance.LabelShortCap}: {ex}");
                }
            }

            if (reg.Race != null)
            {
                try
                {
                    reg.Race.ClearForPawn(__instance);
                }
                catch (Exception ex)
                {
                    Utility_ThoughtBlockerRegistry.LogIfDebug($"Error clearing race tracker for {__instance.LabelShortCap}: {ex}");
                }
            }
            // Add more if other tracker types are added
        }
    }

    public static class EMWH_KillHARPatch
    {
        [HarmonyPrefix]
        public static bool KillHARTranspilerforNonHARPanwsPrefix(TattooDef __instance, ref Graphic __result, Pawn pawn, Color color)
        {
            // 💡 Skip rendering tattoos entirely for non-HAR pawns
            if (pawn?.def?.GetType().Name != "ThingDef_AlienRace")
            {
                __result = null;
                return false; // Prevents HAR's transpiled logic from running
            }

            return true; // Allow HAR logic to run for HAR pawns
        }
    }
}