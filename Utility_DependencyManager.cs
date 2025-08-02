using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public static class Utility_DependencyManager
    {
        public static bool IsRimDarkActive()
        {
            return ModsConfig.IsActive("Phonicmas.RimDark.MankindsFinest") || ModsConfig.IsActive("Phonicmas.40kGenes");
        }

        public static bool IsVFEActive()
        {
            return ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core");
        }

        public static bool IsVFEPiratesActive()
        {
            return ModsConfig.IsActive("OskarPotocki.VFE.Pirates");
        }

        public static bool IsVPEActive()
        {
            return ModsConfig.IsActive("VanillaExpanded.VPsycastsE");
        }

        public static bool IsFacialAnimationActive()
        {
            return ModsConfig.IsActive("Nals.FacialAnimation");
        }

        public static bool IsVehicleFrameworkActive()
        {
            return ModsConfig.IsActive("SmashPhil.VehicleFramework");
        }

        public static bool IsCombatExtendedActive()
        {
            return ModsConfig.IsActive("CETeam.CombatExtended");
        }

        public static bool IsFuckingHARActive()
        {
            return ModsConfig.IsActive("erdelf.HumanoidAlienRaces") || ModsConfig.IsActive("erdelf.HumanoidAlienRaces.dev");
        }

        // My Mods
        public static bool IsAACoreActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.Core");
        }

        public static bool IsSpaceWolvesActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.AA.SW");
        }

        public static bool IsGCCoreActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.GC.Core");
        }

        public static bool IsNCCoreActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.NC.Core");
        }

        public static bool IsOKCoreActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core");
        }

        public static bool IsChaosDaemonsActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.CH.CD");
        }

        public static bool IsThousandSonsActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS");
        }

        public static bool IsDeathGuardActive()
        {
            return ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.DG");
        }
    }
}
