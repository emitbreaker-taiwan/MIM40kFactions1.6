using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class ITab_ContentsSpiritStone : ITab_ContentsBase
    {
        private List<Thing> listInt = new List<Thing>();

        public override IList<Thing> container
        {
            get
            {
                if (SelThing is Building_SpiritStoneVault vault)
                {
                    listInt.Clear();

                    foreach (Thing item in vault.GetInnerContainer())
                    {
                        if (item.TryGetComp<CompSpiritStone>() != null)
                        {
                            listInt.Add(item);
                        }
                    }

                    return listInt;
                }

                return listInt;
            }
        }

        public ITab_ContentsSpiritStone()
        {
            labelKey = "TabCasketContents";
            containedItemsKey = "ContainedItems";
            canRemoveThings = true;
        }
    }
}
