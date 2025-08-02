using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class HediffComp_SpiritStone : HediffComp
    {
        public HediffCompProperties_SpiritStone Props => (HediffCompProperties_SpiritStone)props;
        private Map mapToSpawn => parent.pawn.MapHeld;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            IntVec3 positionToSpawn = parent.pawn.Position;

            if (mapToSpawn == null)
            {
                Log.Warning($"[MIM Aeldari] Cannot spawn {parent.LabelCap}'s spirit stone because the pawn's map is null.");
                return;
            }

            if (parent.pawn.Dead && parent.pawn.Corpse != null)
            {
                if (Props.debugMode)
                {
                    Log.Message($"[MIM Aeldari] Aeldari Pawn {Pawn.LabelCap} died. Attempting to spawn Spirit Stone.");
                }

                if (parent.pawn.apparel != null)
                {
                    List<Apparel> wornApparel = parent.pawn.apparel.WornApparel.ToList(); // ToList to avoid modifying collection while iterating
                    foreach (Apparel ap in wornApparel)
                    {
                        parent.pawn.apparel.TryDrop(ap); // Remove from pawn's apparel list
                        if (Props.debugMode) Log.Message($"[MIM Aeldari] Dropped apparel: {ap.LabelCap}");
                    }
                }

                // 2. Drop all equipped weapons
                if (parent.pawn.equipment != null && parent.pawn.equipment.Primary != null)
                {
                    parent.pawn.equipment.DropAllEquipment(positionToSpawn);
                    if (Props.debugMode) Log.Message($"[MIM Aeldari] Dropped weapon: {parent.pawn.equipment.Primary.LabelCap}");
                }

                Thing spiritStone = ThingMaker.MakeThing(Props.spiritStoneDef);

                CompSpiritStone compSpiritStone = spiritStone.TryGetComp<CompSpiritStone>();
                if (compSpiritStone != null)
                {
                    compSpiritStone.StorePawnData(parent.pawn);
                    if (Props.debugMode)
                    {
                        Log.Message($"[MIM Aeldari] Stored data for {parent.pawn.LabelCap} in Spirit Stone.");
                    }
                }
                else
                {
                    Log.Warning($"[MIM Aeldari] Spirit Stone ThingDef {Props.spiritStoneDef.defName} is missing CompAeldariSoulData. Soul data not stored.");
                }

                GenSpawn.Spawn(spiritStone, positionToSpawn, mapToSpawn);

                parent.pawn.Corpse.Destroy();
                if (Props.debugMode)
                {
                    Log.Message($"[MIM Aeldari] Corpse of {parent.pawn.LabelCap} destroyed.");
                }
            }
        }
    }
}
