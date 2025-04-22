using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIM40kFactions
{
    public interface ISanitizerSource
    {
        List<string> needsToRemove { get; }
        List<string> hediffsToRemove { get; }
        List<string> thoughtsToRemove { get; }
        List<string> blockedInspirationDefs { get; }
        List<string> blockedMentalStateDefs { get; }

        bool wipeAllMemories { get; }
        bool preventNewThoughts { get; }
        bool removeInspiration { get; }
        bool removeMentalState { get; }
        bool blockAllInspiration { get; }
        bool blockAllMentalStates { get; }
        bool removeAllAddictions { get; }
    }
}