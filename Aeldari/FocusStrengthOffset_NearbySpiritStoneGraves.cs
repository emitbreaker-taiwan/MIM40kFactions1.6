using RimWorld;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class FocusStrengthOffset_NearbySpiritStoneGraves : FocusStrengthOffset_BuildingDefs
    {
        public float focusPerFullGrave;

        protected override float OffsetForBuilding(Thing b)
        {
            float num = OffsetFor(b.def);

            if (b is Building_Grave grave)
            {
                foreach (var inner in grave.GetDirectlyHeldThings())
                {
                    var comp = inner.TryGetComp<CompSpiritStone>();
                    if (comp != null)
                    {
                        var soul = comp.GetOriginalPawn();
                        if (soul != null && soul.RaceProps != null && soul.RaceProps.Humanlike)
                        {
                            num += focusPerFullGrave;
                            break;
                        }
                    }
                }
            }

            return num;
        }

        public override float MaxOffset(Thing parent = null)
        {
            return (float)maxBuildings * (focusPerFullGrave + base.MaxOffsetPerBuilding);
        }

        public override string GetExplanation(Thing parent)
        {
            if (!parent.Spawned)
            {
                return GetExplanationAbstract(parent.def);
            }

            int num = BuildingCount(parent);
            return explanationKey.Translate(
                num,
                maxBuildings,
                base.MaxOffsetPerBuilding.ToString("0%"),
                (base.MaxOffsetPerBuilding + focusPerFullGrave).ToString("0%")
            ) + ": " + GetOffset(parent).ToStringWithSign("0%");
        }

        public override string GetExplanationAbstract(ThingDef def = null)
        {
            return explanationKeyAbstract.Translate(
                maxBuildings,
                base.MaxOffsetPerBuilding.ToString("0%"),
                (base.MaxOffsetPerBuilding + focusPerFullGrave).ToString("0%")
            ) + ": +0-" + MaxOffset().ToString("0%");
        }
    }
}
