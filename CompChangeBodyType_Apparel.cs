using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompChangeBodyType_Apparel : ThingComp
    {
        private CompProperties_CompChangeBodyTypeApparel Props
        {
            get => (CompProperties_CompChangeBodyTypeApparel)props;
        }
        private BodyTypeDef originalBodyTypeDef;
        private BodyTypeDef targetBodyTypeDef;
        private Vector2 originalBodyDrawSize;
        private Vector2 originalHeadDrawSize;
        private Vector2 baseBodyDrawSize;
        private Vector2 baseHeadDrawSize;

        public override void Notify_Equipped(Pawn pawn)
        {
            originalBodyTypeDef = pawn.story.bodyType;
            ChangeBodyTypeExtension submodExtension = Utility_GetModExtension.GetChangeBodyTypeExtension(pawn);
            if (submodExtension == null)
            {
                return;
            }
            if (submodExtension != null)
            {
                DefineOriginalSize(submodExtension);
                SetSubmodDrawSize(Props.targetBodyTypeDef, submodExtension);
            }
            if (Props.targetBodyTypeDef != null && GenderValidator(Props.gender, pawn))
            {
                targetBodyTypeDef = Props.targetBodyTypeDef;
            }
            if (Props.genderBodyTypeSets != null)
            {
                foreach (var genderSet in Props.genderBodyTypeSets)
                {
                    if (pawn.gender == genderSet.gender)
                    {
                        SetSubmodDrawSize(genderSet.def, submodExtension);
                        targetBodyTypeDef = genderSet.def;
                    }
                }
            }
            if (targetBodyTypeDef == null)
            {
                targetBodyTypeDef = pawn.story.bodyType;
            }
            if (targetBodyTypeDef != null)
            {
                pawn.story.bodyType = targetBodyTypeDef;
            }
        }
        private void DefineOriginalSize(ChangeBodyTypeExtension submodExtension)
        {
            submodExtension.originalbodyTypeDef = originalBodyTypeDef;
            if (originalBodyTypeDef == BodyTypeDefOf.Male)
            {
                originalBodyDrawSize = submodExtension.drawSizeMale;
                originalHeadDrawSize = submodExtension.headdrawSizeMale;
            }
            if (originalBodyTypeDef == BodyTypeDefOf.Female)
            {
                originalBodyDrawSize = submodExtension.drawSizeFemale;
                originalHeadDrawSize = submodExtension.headdrawSizeFemale;
            }
            if (originalBodyTypeDef == BodyTypeDefOf.Fat)
            {
                originalBodyDrawSize = submodExtension.drawSizeFat;
                originalHeadDrawSize = submodExtension.headdrawSizeFat;
            }
            if (originalBodyTypeDef == BodyTypeDefOf.Hulk)
            {
                originalBodyDrawSize = submodExtension.drawSizeHulk;
                originalHeadDrawSize = submodExtension.headdrawSizeHulk;
            }
            if (originalBodyTypeDef == BodyTypeDefOf.Thin)
            {
                originalBodyDrawSize = submodExtension.drawSizeThin;
                originalHeadDrawSize = submodExtension.headdrawSizeThin;
            }
            else
            {
                originalBodyDrawSize.x = 1f;
                originalBodyDrawSize.y = 1f;
                originalHeadDrawSize.x = 1f;
                originalHeadDrawSize.y = 1f;
            }
        }
        private void SetSubmodDrawSize(BodyTypeDef bodytypeDef, ChangeBodyTypeExtension submodExtension)
        {
            if (bodytypeDef == BodyTypeDefOf.Male)
            {
                baseBodyDrawSize = submodExtension.drawSizeMale;
                baseHeadDrawSize = submodExtension.headdrawSizeMale;

                submodExtension.drawSize = submodExtension.drawSizeMale;
                submodExtension.headdrawSize = submodExtension.headdrawSizeMale;

                submodExtension.drawSizeOriginal = originalBodyDrawSize;
                submodExtension.headdrawSizeOriginal = originalHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Female)
            {
                baseBodyDrawSize = submodExtension.drawSizeFemale;
                baseHeadDrawSize = submodExtension.headdrawSizeFemale;

                submodExtension.drawSize = submodExtension.drawSizeFemale;
                submodExtension.headdrawSize = submodExtension.headdrawSizeFemale;

                submodExtension.drawSizeOriginal = originalBodyDrawSize;
                submodExtension.headdrawSizeOriginal = originalHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Fat)
            {
                baseBodyDrawSize = submodExtension.drawSizeFat;
                baseHeadDrawSize = submodExtension.headdrawSizeFat;

                submodExtension.drawSize = submodExtension.drawSizeFat;
                submodExtension.headdrawSize = submodExtension.headdrawSizeFat;

                submodExtension.drawSizeOriginal = originalBodyDrawSize;
                submodExtension.headdrawSizeOriginal = originalHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Hulk)
            {
                baseBodyDrawSize = submodExtension.drawSizeHulk;
                baseHeadDrawSize = submodExtension.headdrawSizeHulk;

                submodExtension.drawSize = submodExtension.drawSizeHulk;
                submodExtension.headdrawSize = submodExtension.headdrawSizeHulk;

                submodExtension.drawSizeOriginal = originalBodyDrawSize;
                submodExtension.headdrawSizeOriginal = originalHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Thin)
            {
                baseBodyDrawSize = submodExtension.drawSizeThin;
                baseHeadDrawSize = submodExtension.headdrawSizeThin;

                submodExtension.drawSize = submodExtension.drawSizeThin;
                submodExtension.headdrawSize = submodExtension.headdrawSizeThin;

                submodExtension.drawSizeOriginal = originalBodyDrawSize;
                submodExtension.headdrawSizeOriginal = originalHeadDrawSize;
            }
        }
        private bool GenderValidator(Gender? gender, Pawn pawn)
        {
            if (gender == null || gender == Gender.None)
            {
                return true;
            }
            if (pawn.gender != gender)
            {
                return false;
            }
            return true;
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            if (originalBodyTypeDef == null || originalBodyTypeDef == pawn.story.bodyType)
            {
                return;
            }
            pawn.story.bodyType = originalBodyTypeDef;
            ChangeBodyTypeExtension submodExtension = Utility_GetModExtension.GetChangeBodyTypeExtension(pawn);
            if (submodExtension == null)
            {
                return;
            }
            if (submodExtension != null)
            {
                if (Props.targetBodyTypeDef != null)
                {
                    RecoverBaseSize(Props.targetBodyTypeDef, submodExtension);
                }
                if (Props.genderBodyTypeSets != null)
                {
                    foreach (var genderSet in Props.genderBodyTypeSets)
                    {
                        if (pawn.gender == genderSet.gender)
                        {
                            RecoverBaseSize(genderSet.def, submodExtension);
                        }
                    }
                }
            }
        }
        private void RecoverBaseSize(BodyTypeDef bodytypeDef, ChangeBodyTypeExtension submodExtension)
        {
            if (bodytypeDef == BodyTypeDefOf.Male)
            {
                submodExtension.drawSizeMale = baseBodyDrawSize;
                submodExtension.headdrawSizeMale = baseHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Female)
            {
                submodExtension.drawSizeFemale = baseBodyDrawSize;
                submodExtension.headdrawSizeFemale = baseHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Fat)
            {
                submodExtension.drawSizeFat = baseBodyDrawSize;
                submodExtension.headdrawSizeFat = baseHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Hulk)
            {
                submodExtension.drawSizeHulk = baseBodyDrawSize;
                submodExtension.headdrawSizeHulk = baseHeadDrawSize;
            }
            if (bodytypeDef == BodyTypeDefOf.Thin)
            {
                submodExtension.drawSizeThin = baseBodyDrawSize;
                submodExtension.headdrawSizeThin = baseHeadDrawSize;
            }
            baseBodyDrawSize = originalBodyDrawSize;
            baseHeadDrawSize = originalHeadDrawSize;
        }
    }
}