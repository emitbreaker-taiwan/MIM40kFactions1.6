using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_BillConfigurator : CompProperties
    {
        [NoTranslate]
        public string titleLabel = "Configure";
        [NoTranslate]
        public string defaultDescription = "Description";
        [NoTranslate]
        public string configureApplied = "Applied";
        [NoTranslate]
        public string configureCancelled = "Cancelled";
        [NoTranslate]
        public string UIIconPath = "UI/Commands/CopySettings";

        [NoTranslate]
        public string searchPlaceholderLabel = "Search items...";
        [NoTranslate]
        public string availableItemsLabel = "Available items:";
        [NoTranslate]
        public string selectedItemsLabel = "Selected items:";
        [NoTranslate]
        public string availableRecipesLabel = "Available recipes:";
        [NoTranslate]
        public string researchRequiresLabel = "Research required:";
        [NoTranslate]
        public string buttonCancelLabel = "Cancel";
        [NoTranslate]
        public string buttonConfirmLabel = "Confirm";
        [NoTranslate]
        public string researchFilterLabel = "Filter by research required";        
        [NoTranslate]
        public string apparelLayerFilterLabel = "Filter by apparel layer";
        [NoTranslate]
        public string queueItemsLabel = "Queue";
        [NoTranslate]
        public string noResearchFound = "No Research Found";

        [NoTranslate]
        public string noItemSelected = "No item selected.";
        [NoTranslate]
        public string selectedLabel = "Selected:";
        [NoTranslate]
        public string noSuggestionLabel = "No suggestions.";

        [NoTranslate]
        public string ceAmmoCompClass = "CombatExtended.CompAmmoUser";

        // NEW FIELDS
        public Gender? previewPawnGender = null;
        public BodyTypeDef previewPawnBodyType = null;

        public CompProperties_BillConfigurator()
        {
            compClass = typeof(CompBillConfigurator);
        }
    }
}
