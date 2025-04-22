using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimWorld

{
    public class MayRequireNCCoreAttribute : MayRequireAttribute
    {
        public MayRequireNCCoreAttribute()
          : base("emitbreaker.MIM.WH40k.NC.Core")
        {
        }
    }
}
