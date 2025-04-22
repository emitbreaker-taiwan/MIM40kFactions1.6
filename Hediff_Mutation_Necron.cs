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
    public class Hediff_Mutation_Necron : HediffWithComps
    {
        public override void Tick()
        {
            if ((double)Severity == 1.0)
                return;

            ageTicks++;

            if (ModsConfig.BiotechActive && ModsConfig.IsActive("emitbreaker.MIM.WH40k.NC.Core") && pawn.genes.Xenotype != Utility_XenotypeManagement.Named("EMNC_Necrons"))
                pawn.genes.SetXenotype(Utility_XenotypeManagement.Named("EMNC_Necrons"));
            Severity = 1;
        }
        private static void DoCosmetic(Pawn EMNC_Necron)
        {
            EMNC_Necron.story.hairDef = HairDefOf.Bald;
            EMNC_Necron.style.beardDef = BeardDefOf.NoBeard;
            EMNC_Necron.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            EMNC_Necron.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
        }
        private static void DoChangeBody(Pawn EMNC_Necron, BodySnatcherExtension modExtension)
        {
            try
            {
                if (modExtension.bodyTypeDef != null)
                    EMNC_Necron.story.bodyType = modExtension.bodyTypeDef;
                if (!modExtension.pathBody.NullOrEmpty() && PathBodyValidator(modExtension))
                {
                    EMNC_Necron.story.bodyType.bodyNakedGraphicPath = modExtension.pathBody;
                    EMNC_Necron.story.bodyType.bodyDessicatedGraphicPath = modExtension.pathBody;
                }
            }
            catch (NullReferenceException)
            {
                Log.Error("Change bodyNakedGraphicPath failed");
                return;
            }            
        }
        private static bool PathBodyValidator(BodySnatcherExtension modExtension)
        {
            if (modExtension.bodyTypeDef == null)
                return true;
            if (modExtension.bodyTypeDef != null && modExtension.usepathBodywithBodyDef)
                return true;
            return false;
        }
        private static void DoChangeHead(Pawn EMNC_Necron, BodySnatcherExtension modExtension)
        {
            try
            {
                if (modExtension.invisibleHead)
                {
                    EMNC_Necron.story.headType = Utility_HeadTypeDefManagement.Named("EMWH_Invisible");
                    return;
                }
                if (!modExtension.invisibleHead && modExtension.headTypeDef != null)
                    EMNC_Necron.story.headType = modExtension.headTypeDef;
            }
            catch (NullReferenceException)
            {
                Log.Error("Change headtype failed");
                return;
            }
        }
        private static void DoChangeBodyScale(Pawn EMNC_Necron, BodySnatcherExtension modExtension)
        {
            try
            {
                EMNC_Necron.story.bodyType.bodyGraphicScale.x *= modExtension.drawSize.x;
                EMNC_Necron.story.bodyType.bodyGraphicScale.y *= modExtension.drawSize.y;
            }
            catch (NullReferenceException)
            {
                Log.Error("Change bodyscale failed");
                return;
            }
        }
        private static void DoPostAction(Pawn EMNC_Necron)
        {
            try
            {
                EMNC_Necron.Drawer.renderer.SetAllGraphicsDirty();
            }
            catch (NullReferenceException)
            {
                Log.Error("Graphic refresh failed");
                return;
            }            
        }
    }
}
