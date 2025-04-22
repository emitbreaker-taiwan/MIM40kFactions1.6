using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Verb_ShootRadialBeam : Verb_ShootLasBeam
    {
        protected override void HitCell(IntVec3 cell, IntVec3 sourceCell, float damageFactor = 1f)
        {
            base.HitCell(cell, sourceCell, damageFactor);
            DoRadialArcBlast(cell, verbProps.beamTotalDamage / pathCells.Count * damageFactor);
        }

        private void DoRadialArcBlast(IntVec3 center, float baseDamage)
        {
            float radius = 3f;
            float falloff = 0.5f;

            foreach (var c in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!c.InBounds(caster.Map)) continue;
                if (Rand.Chance(falloff))
                {
                    Thing t = c.GetFirstPawn(caster.Map) ?? (Thing)c.GetFirstBuilding(caster.Map);
                    if (t != null && t != caster)
                    {
                        DamageInfo dinfo = new DamageInfo(verbProps.beamDamageDef, baseDamage * Rand.Range(0.7f, 1.1f), -1f, (c - caster.Position).AngleFlat, caster);
                        t.TakeDamage(dinfo);
                    }
                }
            }
        }
    }
}