using System;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompCauseAbilities_Apparel : ThingComp
    {
        private CompProperties_CauseAbilities_Apparel Props
        {
            get => (CompProperties_CauseAbilities_Apparel)props;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            if (Props.abilityDef != null)
                pawn.abilities.GainAbility(Props.abilityDef);
            if (Props.abilityDefs.NullOrEmpty<AbilityDef>())
                return;
            for (int index = 0; index < Props.abilityDefs.Count; ++index)
                pawn.abilities.GainAbility(Props.abilityDefs[index]);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (Props.abilityDef != null)
                pawn.abilities.RemoveAbility(Props.abilityDef);
            if (Props.abilityDefs.NullOrEmpty<AbilityDef>())
                return;
            for (int index = 0; index < Props.abilityDefs.Count; ++index)
                pawn.abilities.RemoveAbility(Props.abilityDefs[index]);
        }
    }
}