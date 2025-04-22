using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimWorld

{
    public class MayRequireDeathGuardAttribute : MayRequireAttribute
    {
        public MayRequireDeathGuardAttribute()
          : base("emitbreaker.MIM.WH40k.CSM.DG")
        {
        }
    }
}
