using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompMechPowerCellBuilding : ThingComp
    {
        private int powerTicksLeft;

        public bool depleted;

        private MechPowerCellBuildingGizmo gizmo;

        public CompProperties_MechPowerCellBuilding Props => (CompProperties_MechPowerCellBuilding)props;

        public float PercentFull => (float)powerTicksLeft / (float)Props.totalPowerTicks;

        public int PowerTicksLeft => powerTicksLeft;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            //if (!ModLister.CheckBiotechOrAnomaly("Mech power cell"))
            //{
            //    parent.Destroy();
            //    return;
            //}

            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                powerTicksLeft = Props.totalPowerTicks;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            if (Find.Selector.SingleSelectedThing == parent)
            {
                if (gizmo == null)
                {
                    gizmo = new MechPowerCellBuildingGizmo(this);
                }

                gizmo.Order = -100f;
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Power left 0%";
                command_Action.action = delegate
                {
                    powerTicksLeft = 0;
                };
                yield return command_Action;
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "DEV: Power left 100%";
                command_Action2.action = delegate
                {
                    powerTicksLeft = Props.totalPowerTicks;
                };
                yield return command_Action2;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (depleted)
            {
                return;
            }

            powerTicksLeft--;
            if (powerTicksLeft <= 0)
            {
                if (Props.killWhenDepleted)
                {
                    KillPowerProcessor();
                    return;
                }

                powerTicksLeft = 0;
                depleted = true;
            }
        }

        private void KillPowerProcessor()
        {
            Building building = (Building)parent;
            if (building != null)
            {
                building.Destroy();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref powerTicksLeft, "powerTicksLeft", 0);
            Scribe_Values.Look(ref depleted, "depleted", defaultValue: false);
        }
    }
}