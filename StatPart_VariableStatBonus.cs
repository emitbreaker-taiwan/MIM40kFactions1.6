using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    /// <summary>
    /// StatPart that applies per‐stat offsets/factors from any
    /// HediffComp_VariableStatBonus on the pawn’s hediffs.
    /// </summary>
    public class StatPart_VariableStatBonus : StatPart
    {
        // The StatDef this part was injected into
        public StatDef stat;

        public override void TransformValue(StatRequest req, ref float val)
        {
            // Only care about pawns
            if (!req.HasThing) return;
            var pawn = req.Thing as Pawn;
            if (pawn == null) return;

            // For each hediff‐comp, ask it how much offset/factor to apply to 'stat'
            foreach (var hd in pawn.health.hediffSet.hediffs)
            {
                var comp = hd.TryGetComp<HediffComp_VariableStatBonus>();
                if (comp == null) continue;

                float offset = comp.GetOffset(stat);
                float factor = comp.GetFactor(stat);

                if (!Mathf.Approximately(offset, 0f))
                    val += offset;
                if (!Mathf.Approximately(factor, 1f))
                    val *= factor;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing) return null;
            var pawn = req.Thing as Pawn;
            if (pawn == null) return null;

            var sb = new StringBuilder();
            foreach (var hd in pawn.health.hediffSet.hediffs)
            {
                var comp = hd.TryGetComp<HediffComp_VariableStatBonus>();
                if (comp == null) continue;

                float offset = comp.GetOffset(stat);
                float factor = comp.GetFactor(stat);

                if (!Mathf.Approximately(offset, 0f))
                    sb.AppendLine($"{hd.LabelCap}: +{offset:F1}");
                if (!Mathf.Approximately(factor, 1f))
                    sb.AppendLine($"{hd.LabelCap}: ×{factor:F2}");
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd('\r', '\n') : null;
        }
    }
}