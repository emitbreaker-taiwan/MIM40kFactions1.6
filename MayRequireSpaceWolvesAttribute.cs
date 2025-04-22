using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimWorld
{
    public class MayRequireSpaceWolvesAttribute : MayRequireAttribute
    {
        public MayRequireSpaceWolvesAttribute()
          : base("emitbreaker.MIM.WH40k.AA.SW")
        {
        }
    }
}
