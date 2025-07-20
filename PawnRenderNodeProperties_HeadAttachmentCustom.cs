using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class PawnRenderNodeProperties_HeadAttachmentCustom : PawnRenderNodeProperties
    {
        public PawnRenderNodeProperties_HeadAttachmentCustom()
        {
            visibleFacing = new List<Rot4>
        {
            Rot4.East,
            Rot4.South,
            Rot4.West
        };
            workerClass = typeof(PawnRenderNodeWorker_HeadAttachmentCustom);
            nodeClass = typeof(PawnRenderNode_AttachmentHead);
        }

        public override void ResolveReferences()
        {
            skipFlag = RenderSkipFlagDefOf.Eyes;
        }
    }
}