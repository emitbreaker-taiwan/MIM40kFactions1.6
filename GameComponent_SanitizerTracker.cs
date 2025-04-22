using MIM40kFactions.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class GameComponent_SanitizerTracker : GameComponent
    {
        public GameComponent_SanitizerTracker(Game game) { }

        [Multiplayer.SyncMethod]
        public override void ExposeData()
        {
            base.ExposeData();
            SanitizerTrackerRegistry.Instance.ExposeData();
        }
    }
}
