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
    public class PawnRenderNodeWorker_HeadAttachmentCustom : PawnRenderNodeWorker_FlipWhenCrawling
    {        
        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 result = base.OffsetFor(node, parms, out pivot);
            if (TryGetWoundAnchor(node.Props.anchorTag, parms, out var anchor))
            {
                PawnDrawUtility.CalcAnchorData(parms.pawn, anchor, parms.facing, out var anchorOffset, out var _);
                result += anchorOffset;
            }

            return result;
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            Vector3 vector = Vector3.one;
            vector.x *= node.Props.drawSize.x * node.debugScale;
            vector.z *= node.Props.drawSize.y * node.debugScale;
            if (node.AnimationWorker != null && node.AnimationWorker.Enabled() && !parms.flags.FlagSet(PawnRenderFlags.Portrait))
            {
                vector = vector.MultipliedBy(node.AnimationWorker.ScaleAtTick(node.tree.AnimationTick, parms));
            }

            if (node.Props.drawData != null)
            {
                vector *= node.Props.drawData.ScaleFor(parms.pawn);
            }

            BodySnatcherExtension modExtension = Utility_BodySnatcherManager.GetBodySnatcherExtension(parms.pawn);

            if (modExtension != null)
            {
                if (modExtension.headdrawSize.x > 0)
                {
                    vector.x *= modExtension.headdrawSize.x;
                    vector.z *= modExtension.headdrawSize.y;
                }
                else if (modExtension.drawSize.x > 0)
                {
                    vector.x *= modExtension.drawSize.x;
                    vector.z *= modExtension.drawSize.y;
                }
            }

            if (node.Props.anchorTag == "RightEye" ||  node.Props.anchorTag == "LeftEye")
            {
                vector *= parms.pawn.ageTracker.CurLifeStage.eyeSizeFactor.GetValueOrDefault(1f);
            }
            
            return vector;
        }

        protected bool TryGetWoundAnchor(string anchorTag, PawnDrawParms parms, out BodyTypeDef.WoundAnchor anchor)
        {
            anchor = null;
            if (anchorTag.NullOrEmpty())
            {
                return false;
            }

            List<BodyTypeDef.WoundAnchor> woundAnchors = parms.pawn.story.bodyType.woundAnchors;
            for (int i = 0; i < woundAnchors.Count; i++)
            {
                BodyTypeDef.WoundAnchor woundAnchor = woundAnchors[i];
                if (woundAnchor.tag == anchorTag)
                {
                    Rot4? rotation = woundAnchor.rotation;
                    Rot4 facing = parms.facing;
                    if (rotation.HasValue && (!rotation.HasValue || rotation.GetValueOrDefault() == facing) && (parms.facing == Rot4.South || woundAnchor.narrowCrown.GetValueOrDefault() == parms.pawn.story.headType.narrow))
                    {
                        anchor = woundAnchor;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
