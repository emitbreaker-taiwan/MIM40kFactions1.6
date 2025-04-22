using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimWorld

{
    public class MayRequireThousandSonsAttribute : MayRequireAttribute
    {
        public MayRequireThousandSonsAttribute()
          : base("emitbreaker.MIM.WH40k.CSM.TS")
        {
        }
    }
}
