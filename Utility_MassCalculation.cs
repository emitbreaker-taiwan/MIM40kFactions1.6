using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Utility_MassCalculation
    {
        private static float massModifier => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().massModifier;

        public static float CarryMassCalculation(Pawn pawn, float __result, float drawSize)
        {
            if (__result < 1f)
            {
                __result = 1f;
            }

            if (drawSize > 1f)
            {
                drawSize = drawSize - 1f;
            }
            else
            {
                drawSize = 0.01f;
            }

            float multiplier = drawSize * (100 + massModifier);

            if (pawn.RaceProps.baseBodySize > 1f)
            {
                multiplier += (pawn.RaceProps.baseBodySize - 1f) * massModifier;
            }

            EMNC_Necron_ValidatiorExtension NecronmodExtension = pawn.def.GetModExtension<EMNC_Necron_ValidatiorExtension>();

            if (NecronmodExtension != null)
            {
                __result += multiplier * (massModifier * 5f);
            }
            else
            {
                __result *= multiplier;
            }
            return __result;
        }

        public static void ApplyMassCarriedExtensionModifiers(Pawn p, ref float mass)
        {
            if (p == null || p.Dead || p.IsDessicated())
                return;

            float baseMass = mass;
            float multiplier = 1f;
            float addedMass = 0f;

            // ✅ Flat bonus from RaceDef
            var raceExt = p.def.GetModExtension<MassCarriedExtension>();
            if (raceExt != null)
            {
                addedMass += p.RaceProps.Humanlike ? raceExt.massCarryBuff : raceExt.massCarryBuffAnimal;
            }

            // ✅ Flat bonus from KindDef
            if (p.kindDef != null)
            {
                var kindExt = p.kindDef.GetModExtension<MassCarriedExtension>();
                if (kindExt != null)
                {
                    addedMass += p.RaceProps.Humanlike ? kindExt.massCarryBuff : kindExt.massCarryBuffAnimal;
                }
            }

            // ✅ Multiplier from BodySnatcherExtension
            BodySnatcherExtension bodySnatcher = Utility_BodySnatcherManager.GetBodySnatcherExtension(p);
            if (bodySnatcher != null)
            {
                float drawFactor = 0f;

                if (p.RaceProps.Humanlike && bodySnatcher.drawSize != null)
                {
                    drawFactor = bodySnatcher.drawSize.x;
                }
                else if (p.RaceProps.packAnimal)
                {
                    drawFactor = bodySnatcher.animalCarryMassModifier;
                }

                if (drawFactor > 1f)
                {
                    float drawMod = drawFactor - 1f;
                    multiplier += drawMod * ((100f + massModifier) / 100f);
                }

                if (p.RaceProps.baseBodySize > 1f)
                {
                    multiplier += (p.RaceProps.baseBodySize - 1f) * (massModifier / 100f);
                }

                if (p.def.GetModExtension<EMNC_Necron_ValidatiorExtension>() != null)
                {
                    addedMass += multiplier * (massModifier * 5f);
                    multiplier = 1f;
                }
            }

            // ✅ Final mass calculation
            mass = (baseMass + addedMass) * multiplier;
        }
    }
}