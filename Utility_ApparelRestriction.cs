using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public static class Utility_ApparelRestriction
    {
        private static StringBuilder errorPart = new StringBuilder();

        public static bool PawnCanEquipbyTrait(Thing thing, Pawn pawn, ref string cantReason)
        {
            ApparelRestrictionbyTraitsExtension modExtension = thing.def.GetModExtension<ApparelRestrictionbyTraitsExtension>();
            cantReason = null;

            if (modExtension == null)
            {
                return true;
            }

            if (modExtension.requiredTraits != null && pawn.story?.traits != null && pawn.story.traits.allTraits.Any(x => modExtension.requiredTraits.Contains(x.def)))
            {
                return true;
            }

            errorPart.Clear();
            errorPart.Append((string)"EMWH_RequiredTraitisMissing".Translate() + ": ");
            string stringSub = string.Empty;
            foreach (TraitDef traitDef in modExtension.requiredTraits)
            {
                stringSub = stringSub.NullOrEmpty() ? traitDef.label.ToString() : stringSub + ", " + traitDef.label.ToString();
            }
            errorPart.Append(stringSub);
            cantReason = errorPart.ToString();

            return false;
        }

        public static bool PawnCanEquipbyArmor(Thing thing, Pawn pawn, ref string cantReason)
        {
            ApparelRestrictionbyArmorsExtension modExtension = thing.def.GetModExtension<ApparelRestrictionbyArmorsExtension>();
            cantReason = null;

            if (modExtension == null)
            {
                return true;
            }
            if (modExtension.requiredArmors != null && pawn.apparel.WornApparel != null)
            {
                foreach (Apparel apparel1 in pawn.apparel.WornApparel)
                {
                    if (modExtension.requiredArmors.Contains(apparel1.def))
                    {
                        return true;
                    }
                }
            }

            errorPart.Clear();
            errorPart.Append((string)"EMWH_RequiredArmorisMissing".Translate() + ": ");
            string stringSub = string.Empty;
            foreach (ThingDef armorDef in modExtension.requiredArmors)
            {
                stringSub = stringSub.NullOrEmpty() ? armorDef.label.ToString() : stringSub + ", " + armorDef.label.ToString();
            }
            errorPart.Append(stringSub);
            cantReason = errorPart.ToString();

            return false;
        }

        public static bool PawnCanEquipbyapparelTags(Thing thing, Pawn pawn, ref string cantReason)
        {
            ApparelRestrictionbyapparelTagsExtension modExtension = thing.def.GetModExtension<ApparelRestrictionbyapparelTagsExtension>();
            cantReason = null;
            string stringSub = string.Empty;

            if (modExtension == null)
            {
                return true;
            }
            List<string> requiredapparelTags = modExtension.requiredapparelTags;

            if (modExtension.requiredapparelTags != null && pawn.apparel.WornApparel != null)
            {
                foreach (Apparel apparel1 in pawn.apparel.WornApparel)
                {
                    ApparelProperties apparel = apparel1.def.apparel;
                    if (requiredapparelTags != null && apparel.tags.Intersect<string>((IEnumerable<string>)requiredapparelTags).Any<string>())
                    {
                        if (modExtension.requiredHediffDefs != null)
                        {
                            List<HediffDef> requiredHediffDefs = modExtension.requiredHediffDefs;

                            if (modExtension.requiredHediffDefs != null && pawn.health.hediffSet.hediffs.Any(x => modExtension.requiredHediffDefs.Contains(x.def)))
                            {
                                return true;
                            }
                            else
                            {
                                if (Utility_DependencyManager.IsRimDarkActive() && pawn.genes.HasActiveGene(Utility_GeneManager.GeneDefNamed("BEWH_BlackCarapace")) && modExtension.requiredHediffDefs.Contains(HediffDef.Named("EMWH_BlackCarapace")))
                                {
                                    return true;
                                }
                                errorPart.Clear();
                                errorPart.Append((string)"EMWH_RequiredHediffisMissing".Translate() + ": ");
                                if (modExtension.errorMessageHediffAlt != null)
                                {
                                    stringSub = stringSub.NullOrEmpty() ? modExtension.errorMessageHediffAlt.Translate() : stringSub + ", " + modExtension.errorMessageHediffAlt.Translate();
                                }
                                else
                                {
                                    foreach (HediffDef hediffdef in modExtension.requiredHediffDefs)
                                    {
                                        stringSub = stringSub.NullOrEmpty() ? hediffdef.label.ToString() : stringSub + ", " + hediffdef.label.ToString();
                                    }
                                }
                                errorPart.Append(stringSub);
                                cantReason = errorPart.ToString();
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            errorPart.Clear();
            errorPart.Append((string)"EMWH_RequiredArmorisMissing".Translate() + ": ");
            if (modExtension.errorMessageAlt != null)
            {
                stringSub = stringSub.NullOrEmpty() ? modExtension.errorMessageAlt.Translate() : stringSub + ", " + modExtension.errorMessageAlt.Translate();
            }
            else
            {
                foreach (string apparelTag in modExtension.requiredapparelTags)
                {
                    stringSub = stringSub.NullOrEmpty() ? apparelTag : stringSub + ", " + apparelTag;
                }
            }
            errorPart.Append(stringSub);
            cantReason = errorPart.ToString();
            return false;
        }

        public static bool PawnCanEquipbyHediff(Thing thing, Pawn pawn, ref string cantReason)
        {
            ApparelRestrictionbyHediffExtension modExtension = thing.def.GetModExtension<ApparelRestrictionbyHediffExtension>();
            cantReason = null;

            if (modExtension == null)
            {
                return true;
            }
            List<HediffDef> requiredHediffDefs = modExtension.requiredHediffDefs;

            if (modExtension.requiredHediffDefs != null && pawn.health.hediffSet.hediffs.Any(x => modExtension.requiredHediffDefs.Contains(x.def)))
            {
                return true;
            }

            errorPart.Clear();
            errorPart.Append((string)"EMWH_RequiredHediffisMissing".Translate() + ": ");
            string stringSub = string.Empty;
            if (modExtension.errorMessageAlt != null)
            {
                stringSub = stringSub.NullOrEmpty() ? modExtension.errorMessageAlt.Translate() : stringSub + ", " + modExtension.errorMessageAlt.Translate();
            }
            else
            {
                foreach (HediffDef hediffdef in modExtension.requiredHediffDefs)
                {
                    stringSub = stringSub.NullOrEmpty() ? hediffdef.label.ToString() : stringSub + ", " + hediffdef.label.ToString();
                }
            }
            errorPart.Append(stringSub);
            cantReason = errorPart.ToString();
            return false;
        }

        public static bool PawnCanEquipbyRace(Thing thing, Pawn pawn, ref string cantReason)
        {
            ApparelRestrictionbyRaceExtension modExtension = thing.def.GetModExtension<ApparelRestrictionbyRaceExtension>();
            cantReason = null;

            if (modExtension == null)
            {
                return true;
            }
            List<ThingDef> allowedRaces = modExtension.allowedRaces;

            if (allowedRaces != null)
            {
                foreach (ThingDef allowedRace in allowedRaces)
                {
                    if (pawn.def == allowedRace)
                    {
                        return true;
                    }
                }
            }
            cantReason = "EMWH_CannotEquipbyRace".Translate();
            return false;
        }

        public static bool PawnCannotEquipbyRace(Thing thing, Pawn pawn, ref string cantReason)
        {
            EMWH_ApparelRestriction_ValidatiorExtension modExtension = pawn.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();
            EMWH_ApparelRestriction_ValidatiorExtension modExtension2 = thing.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();

            cantReason = null;

            if (modExtension == null)
            {
                return true;
            }

            if (modExtension2 != null)
            {
                if (modExtension.keywords != null)
                {
                    foreach (string keyword in modExtension.keywords)
                    {
                        if (modExtension2.keywords.Contains(keyword))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }

            cantReason = "EMWH_CannotEquipbyRace".Translate();
            return false;
        }

        //public static bool PawnCanWearbyTrait(Apparel __instance, Pawn pawn, bool __result)
        //{
        //    ApparelRestrictionbyTraitsExtension modExtension = __instance.def.GetModExtension<ApparelRestrictionbyTraitsExtension>();

        //    if (modExtension == null)
        //    {
        //        return true;
        //    }

        //    if (modExtension.requiredTraits != null && pawn.story?.traits != null && pawn.story.traits.allTraits.Any(x => modExtension.requiredTraits.Contains(x.def)))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //public static bool PawnCanWearbyArmor(Apparel __instance, Pawn pawn, bool __result)
        //{
        //    ApparelRestrictionbyArmorsExtension modExtension = __instance.def.GetModExtension<ApparelRestrictionbyArmorsExtension>();

        //    if (modExtension == null)
        //    {
        //        return true;
        //    }
        //    if (modExtension.requiredArmors != null && pawn.apparel.WornApparel != null)
        //    {
        //        foreach (Apparel apparel1 in pawn.apparel.WornApparel)
        //        {
        //            if (modExtension.requiredArmors.Contains(apparel1.def))
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        public static bool PawnCanWearbyapparelTags(Apparel __instance, Pawn pawn)
        {
            ApparelRestrictionbyapparelTagsExtension modExtension = __instance.def.GetModExtension<ApparelRestrictionbyapparelTagsExtension>();

            if (modExtension == null)
            {
                return true;
            }
            List<string> requiredapparelTags = modExtension.requiredapparelTags;

            if (modExtension.requiredapparelTags != null && pawn.apparel.WornApparel != null)
            {
                foreach (Apparel apparel1 in pawn.apparel.WornApparel)
                {
                    ApparelProperties apparel = apparel1.def.apparel;
                    if (requiredapparelTags != null && apparel.tags.Intersect<string>((IEnumerable<string>)requiredapparelTags).Any<string>())
                    {
                        if (modExtension.requiredHediffDefs != null)
                        {
                            List<HediffDef> requiredHediffDefs = modExtension.requiredHediffDefs;

                            if (modExtension.requiredHediffDefs != null && pawn.health.hediffSet.hediffs.Any(x => modExtension.requiredHediffDefs.Contains(x.def)))
                            {
                                return true;
                            }
                            else
                            {
                                if (Utility_DependencyManager.IsRimDarkActive() && pawn.genes.HasActiveGene(Utility_GeneManager.GeneDefNamed("BEWH_BlackCarapace")) && modExtension.requiredHediffDefs.Contains(HediffDef.Named("EMWH_BlackCarapace")))
                                {
                                    return true;
                                }
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void ValidateArmorDependencies(Pawn pawn)
        {
            if (pawn.apparel?.WornApparel == null)
            {
                return;
            }

            // Create a copy of the WornApparel list to avoid concurrent modification
            List<Apparel> wornApparelCopy = new List<Apparel>(pawn.apparel.WornApparel);
            List<Apparel> toDrop = new List<Apparel>();

            foreach (Apparel apparel in wornApparelCopy)
            {
                ApparelRestrictionbyArmorsExtension modExtension = apparel.def.GetModExtension<ApparelRestrictionbyArmorsExtension>();
                if (modExtension?.requiredArmors != null)
                {
                    bool hasDependency = false;

                    foreach (Apparel wornApparel in wornApparelCopy)
                    {
                        if (wornApparel == apparel) continue;

                        if (modExtension.requiredArmors.Contains(wornApparel.def))
                        {
                            hasDependency = true;
                            break;
                        }
                    }

                    if (!hasDependency)
                    {
                        toDrop.Add(apparel);
                    }
                }
            }

            foreach (Apparel apparel in toDrop)
            {
                if (pawn.apparel.WornApparel.Contains(apparel)) // Ensure the item is still in the list
                {
                    if (!pawn.apparel.TryDrop(apparel, out _, pawn.PositionHeld, forbid: false))
                    {
                        Log.Error($"Failed to drop apparel {apparel.Label} for pawn {pawn.Name}.");
                    }
                    else
                    {
                        Messages.Message("EMWH_ArmorDependencyRemoved".Translate(apparel.Label), MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }

        public static void ValidateApparelDependencies(Pawn pawn)
        {
            if (pawn.apparel?.WornApparel == null)
            {
                return;
            }

            // Create a copy of the WornApparel list to avoid concurrent modification
            List<Apparel> wornApparelCopy = new List<Apparel>(pawn.apparel.WornApparel);
            List<Apparel> toDrop = new List<Apparel>();

            foreach (Apparel apparel in wornApparelCopy)
            {
                ApparelRestrictionbyapparelTagsExtension modExtension = apparel.def.GetModExtension<ApparelRestrictionbyapparelTagsExtension>();
                if (modExtension?.requiredapparelTags != null)
                {
                    bool hasDependency = false;

                    foreach (Apparel wornApparel in wornApparelCopy)
                    {
                        if (wornApparel == apparel) continue;

                        ApparelProperties wornApparelProps = wornApparel.def.apparel;
                        if (wornApparelProps?.tags != null && wornApparelProps.tags.Intersect(modExtension.requiredapparelTags).Any())
                        {
                            hasDependency = true;
                            break;
                        }
                    }

                    if (!hasDependency)
                    {
                        toDrop.Add(apparel);
                    }
                }
            }

            foreach (Apparel apparel in toDrop)
            {
                if (pawn.apparel.WornApparel.Contains(apparel)) // Ensure the item is still in the list
                {
                    if (!pawn.apparel.TryDrop(apparel, out _, pawn.PositionHeld, forbid: false))
                    {
                        Log.Error($"Failed to drop apparel {apparel.Label} for pawn {pawn.Name}.");
                    }
                    else
                    {
                        Messages.Message("EMWH_ApparelDependencyRemoved".Translate(apparel.Label), MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }

        //public static bool PawnCanWearbyHediff(Apparel __instance, Pawn pawn, bool __result)
        //{
        //    ApparelRestrictionbyHediffExtension modExtension = __instance.def.GetModExtension<ApparelRestrictionbyHediffExtension>();

        //    if (modExtension == null)
        //    {
        //        return true;
        //    }
        //    List<HediffDef> requiredHediffDefs = modExtension.requiredHediffDefs;

        //    if (modExtension.requiredHediffDefs != null && pawn.health.hediffSet.hediffs.Any(x => modExtension.requiredHediffDefs.Contains(x.def)))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //public static bool PawnCanWearbyRace(Apparel __instance, Pawn pawn, bool __result)
        //{
        //    ApparelRestrictionbyRaceExtension modExtension = __instance.def.GetModExtension<ApparelRestrictionbyRaceExtension>();

        //    if (modExtension == null)
        //    {
        //        return true;
        //    }
        //    List<ThingDef> allowedRaces = modExtension.allowedRaces;

        //    if (allowedRaces != null)
        //    {
        //        foreach (ThingDef allowedRace in allowedRaces)
        //        {
        //            if (pawn.def == allowedRace)
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        //public static bool PawnCannotWearbyRace(Apparel __instance, Pawn pawn, bool __result)
        //{
        //    EMWH_ApparelRestriction_ValidatiorExtension modExtension = pawn.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();
        //    EMWH_ApparelRestriction_ValidatiorExtension modExtension2 = __instance.def.GetModExtension<EMWH_ApparelRestriction_ValidatiorExtension>();

        //    if (modExtension == null)
        //    {
        //        return true;
        //    }

        //    if (modExtension2 != null)
        //    {
        //        if (modExtension.keywords != null)
        //        {
        //            foreach (string keyword in modExtension.keywords)
        //            {
        //                if (modExtension2.keywords.Contains(keyword))
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}