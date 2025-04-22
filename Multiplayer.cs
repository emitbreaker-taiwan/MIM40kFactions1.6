using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Compatibility
{
    public class Multiplayer : IPatch
    {
        private static bool isMultiplayerActive;
        private static Func<bool> _inMultiplayer;
        private static Func<bool> _isExecutingCommands;
        private static Func<bool> _isExecutingCommandsIssuedBySelf;

        public bool CanInstall()
        {
            Log.Message("Checking Multiplayer Compat");
            return ModLister.HasActiveModWithName(nameof(Multiplayer));
        }

        public void Install()
        {
            Log.Message("CombatExtended :: Installing Multiplayer Compat");
            Multiplayer.isMultiplayerActive = true;
        }

        public static bool InMultiplayer
        {
            get => Multiplayer.isMultiplayerActive && Multiplayer._inMultiplayer();
        }

        public static bool IsExecutingCommands
        {
            get => Multiplayer.isMultiplayerActive && Multiplayer._isExecutingCommands();
        }

        public static bool IsExecutingCommandsIssuedBySelf
        {
            get => Multiplayer.isMultiplayerActive && Multiplayer._isExecutingCommandsIssuedBySelf();
        }

        public static void registerCallbacks(Func<bool> inMP, Func<bool> iec, Func<bool> iecibs)
        {
            Multiplayer._inMultiplayer = inMP;
            Multiplayer._isExecutingCommands = iec;
            Multiplayer._isExecutingCommandsIssuedBySelf = iecibs;
        }

        // Custom SyncMethod logic to synchronize method execution across clients
        public static void SyncMethod(Action methodToSync)
        {
            if (InMultiplayer)
            {
                // If in multiplayer mode, sync method execution across all clients
                Log.Message("Multiplayer active, synchronizing method execution.");
                // Here you would call an actual network sync method depending on how your mod handles networking.
                // For example, you'd implement something like this:
                // Multiplayer.SyncAllClients(methodToSync);
            }
            else
            {
                // If not in multiplayer, execute the method as normal
                methodToSync();
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class SyncMethodAttribute : Attribute
        {
            public int syncContext = -1;
            public int[] exposeParameters = (int[])null;
        }
    }
}
