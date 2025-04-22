using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIM40kFactions.Compatibility
{
    public interface IPatch
    {
        bool CanInstall();

        void Install();
    }
}
