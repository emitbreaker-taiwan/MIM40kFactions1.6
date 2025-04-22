using MIM40kFactions.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Verb_ShootChainBeam : Verb_ShootLasBeam
    {
        protected override void HitCell(IntVec3 cell, IntVec3 sourceCell, float damageFactor = 1f)
        {
            base.HitCell(cell, sourceCell, damageFactor);
            Pawn hitPawn = cell.GetFirstPawn(caster.Map);
            if (hitPawn != null)
            {
                float baseDamage = verbProps.beamTotalDamage / pathCells.Count * damageFactor;
                DoChainBounce(hitPawn, baseDamage);
            }
        }

        private void DoChainBounce(Pawn origin, float baseDamage)
        {
            int maxBounces = 3;
            float falloff = 0.75f;
            float bounceRange = 5f;

            Pawn last = origin;
            float damage = baseDamage;

            for (int i = 0; i < maxBounces; i++)
            {
                Pawn next = GenRadial.RadialCellsAround(last.Position, bounceRange, true)
                    .Select(c => c.GetFirstPawn(caster.Map))
                    .Where(p => p != null && p.Faction != caster.Faction && p != last)
                    .FirstOrDefault();

                if (next == null) break;

                damage *= falloff;
                DamageInfo dinfo = new DamageInfo(verbProps.beamDamageDef, damage, -1f, (next.Position - caster.Position).AngleFlat, caster);
                next.TakeDamage(dinfo);
                last = next;
            }
        }
    }
}
