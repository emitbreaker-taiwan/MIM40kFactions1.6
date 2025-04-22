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
    public class CompMagazinePouch : ThingComp
    {
        public CompProperties_MagazinePouch Props => (CompProperties_MagazinePouch)props;

        protected Pawn PawnOwner
        {
            get
            {
                Apparel apparel;
                if ((apparel = parent as Apparel) != null)
                {
                    return apparel.Wearer;
                }

                Pawn result;
                if ((result = parent as Pawn) != null)
                {
                    return result;
                }

                return null;
            }
        }

        public bool IsApparel => parent is Apparel;
        private bool IsBuiltIn => !IsApparel;

        private bool WeaponValidator()
        {
            if (PawnOwner.equipment.AllEquipmentListForReading.NullOrEmpty())
            {
                return false;
            }
            if (PawnOwner.equipment.Primary.def == null)
            {
                return false;
            }
            return Props.applicableWeapons.Contains(PawnOwner.equipment.Primary.def);
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetWornGizmosExtra())
            {
                yield return item;
            }

            if (IsApparel)
            {
                foreach (Gizmo gizmo in GetGizmos())
                {
                    yield return gizmo;
                }
            }

            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (IsBuiltIn)
            {
                foreach (var gizmo in GetGizmos())
                    yield return gizmo;
            }
        }

        private IEnumerable<Gizmo> GetGizmos()
        {
            if (!WeaponValidator() || PawnOwner?.Faction != Faction.OfPlayer || !PawnOwner.Drafted)
                yield break;

            for (int i = 0; i < Props.changeableProjectiles.Count; i++)
            {
                var option = Props.changeableProjectiles[i];
                if (option.researchProjectRequired == null || option.researchProjectRequired.IsFinished)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "EMWH_ChangableProjectile".Translate() + " " + option.changeableProjectile.label,
                        defaultDesc = option.changeableProjectile.description,
                        icon = TexCommand.Attack,
                        action = () =>
                        {
                            Props.selectedProjectile = option.changeableProjectile;
                            Props.requireLineofSight = option.requiredLineofSight;
                            Props.label = option.changeableProjectile.label;
                            if (option.range > 0f)
                                Props.range = option.range;
                            if (option.burstShotCount > 0)
                                Props.burstShotCount = option.burstShotCount;
                            Props.additionalRange = option.additionalRange;
                            Props.negativeRange = option.negativeRange;

                            // 📢 Display floating message over pawn
                            if (PawnOwner != null && PawnOwner.Spawned)
                            {
                                MoteMaker.ThrowText(PawnOwner.DrawPos, PawnOwner.Map, Props.label, Color.green);
                            }
                        }
                    };
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            sb.AppendLine("EMWH_ChangableProjectileWeapons".Translate());
            sb.AppendLine(string.Join(", ", Props.applicableWeapons.Select(w => w.label)));
            return sb.ToString();
        }

        public override string GetDescriptionPart()
        {
            var sb = new StringBuilder(base.GetDescriptionPart());
            sb.AppendLine("EMWH_ChangableProjectileWeapons".Translate());
            sb.AppendLine(string.Join(", ", Props.applicableWeapons.Select(w => w.label)));
            return sb.ToString();
        }

        public void LogCurrentOverrides()
        {
            if (!DebugSettings.ShowDevGizmos) return;
            Log.Message($"[CompMagazinePouch] Projectile: {Props.selectedProjectile}, Range: {Props.range}, Burst: {Props.burstShotCount}, LoS: {Props.requireLineofSight}, Label: {Props.label}");
        }
    }
}