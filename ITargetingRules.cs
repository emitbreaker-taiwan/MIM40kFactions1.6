using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIM40kFactions
{
    public interface ITargetingRules
    {
        bool TargetHostilesOnly { get; }
        bool TargetNeutralBuildings { get; }
    }
}
