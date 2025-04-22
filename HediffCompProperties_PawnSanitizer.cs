using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_PawnSanitizer : HediffCompProperties, ISanitizerSource
    {
        [NoTranslate]
        public List<string> needsToRemove;
        [NoTranslate]
        public List<string> hediffsToRemove;
        [NoTranslate]
        public List<string> thoughtsToRemove;
        [NoTranslate]
        public List<string> blockedInspirationDefs;
        [NoTranslate]
        public List<string> blockedMentalStateDefs;

        public bool wipeAllMemories = false;
        public bool preventNewThoughts = false;
        public bool removeInspiration = false;
        public bool removeMentalState = false;

        public bool blockAllInspiration = false;
        public bool blockAllMentalStates = false;

        public bool removeAllAddictions = false;

        public bool onlyForHumanlike = false;

        public HediffCompProperties_PawnSanitizer()
        {
            this.compClass = typeof(HediffComp_PawnSanitizer);
        }

        // ✅ Explicit interface property implementations:
        List<string> ISanitizerSource.needsToRemove => needsToRemove;
        List<string> ISanitizerSource.hediffsToRemove => hediffsToRemove;
        List<string> ISanitizerSource.thoughtsToRemove => thoughtsToRemove;
        List<string> ISanitizerSource.blockedInspirationDefs => blockedInspirationDefs;
        List<string> ISanitizerSource.blockedMentalStateDefs => blockedMentalStateDefs;
        bool ISanitizerSource.wipeAllMemories => wipeAllMemories;
        bool ISanitizerSource.preventNewThoughts => preventNewThoughts;
        bool ISanitizerSource.removeInspiration => removeInspiration;
        bool ISanitizerSource.removeMentalState => removeMentalState;
        bool ISanitizerSource.blockAllInspiration => blockAllInspiration;
        bool ISanitizerSource.blockAllMentalStates => blockAllMentalStates;
        bool ISanitizerSource.removeAllAddictions => removeAllAddictions;
    }
}
