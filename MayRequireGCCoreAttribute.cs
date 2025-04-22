using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimWorld

{
    public class MayRequireGCCoreAttribute : MayRequireAttribute
    {
        public MayRequireGCCoreAttribute()
          : base("emitbreaker.MIM.WH40k.GC.Core")
        {
        }
    }
}
