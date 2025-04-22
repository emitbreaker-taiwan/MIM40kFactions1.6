using MIM40kFactions.Compatibility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilitybyWeapon : ThingComp
    {
        public CompProperties_AbilitybyWeapon Props => (CompProperties_AbilitybyWeapon)this.props;

        private Pawn lastHolder;

        // Sync method to ensure abilities are synchronized in multiplayer
        [Multiplayer.SyncMethod]
        public void SyncGainAbilities(Pawn pawn)
        {
            if (ModsConfig.BiotechActive && pawn.abilities != null)
            {
                foreach (var abilityDef in Props.abilities)
                {
                    if (!pawn.abilities.abilities.Any(ab => ab.def == abilityDef))
                    {
                        pawn.abilities.GainAbility(abilityDef);
                    }
                }
            }
        }

        // Sync method for unequipping abilities
        [Multiplayer.SyncMethod]
        public void SyncRemoveAbilities(Pawn pawn)
        {
            if (ModsConfig.BiotechActive && pawn.abilities != null)
            {
                foreach (var abilityDef in Props.abilities)
                {
                    Ability ability = pawn.abilities.abilities.FirstOrDefault(ab => ab.def == abilityDef);
                    if (ability != null)
                    {
                        pawn.abilities.RemoveAbility(abilityDef);
                    }
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            lastHolder = pawn;

            if (ModsConfig.BiotechActive && pawn.abilities != null)
            {
                foreach (var abilityDef in Props.abilities)
                {
                    if (!pawn.abilities.abilities.Any(ab => ab.def == abilityDef))
                    {
                        pawn.abilities.GainAbility(abilityDef);
                    }
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (ModsConfig.BiotechActive && pawn.abilities != null)
            {
                foreach (var abilityDef in Props.abilities)
                {
                    Ability ability = pawn.abilities.abilities.FirstOrDefault(ab => ab.def == abilityDef);
                    if (ability != null)
                    {
                        pawn.abilities.RemoveAbility(abilityDef);
                    }
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (lastHolder != null && lastHolder.Spawned && lastHolder.abilities != null)
            {
                foreach (var abilityDef in Props.abilities)
                {
                    lastHolder.abilities.RemoveAbility(abilityDef);
                }
            }
        }
    }
}
