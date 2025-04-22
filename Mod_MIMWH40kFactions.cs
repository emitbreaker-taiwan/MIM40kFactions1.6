using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using System.Reflection;

namespace MIM40kFactions
{
    public class Mod_MIMWH40kFactions : Mod
    {
        ModSettings_MIMWH40kFactions settings;

        private bool cachedEnableThoughtBlocking;
        private bool cachedEnableThoughtFilter;

        public Mod_MIMWH40kFactions(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSettings_MIMWH40kFactions>();
            cachedEnableThoughtBlocking = settings.enableHarmonyThoughtBlocking;
            cachedEnableThoughtFilter = settings.enableHarmonyThoughtFiltering;
        }

        private static Vector2 scrollPosition = Vector2.zero;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outerRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect innerRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height * 2f); // Adjust height as needed

            // Begin the scroll view
            scrollPosition = GUI.BeginScrollView(outerRect, scrollPosition, innerRect);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(innerRect);
            listingStandard.Label("ModSetting_MIMWH40kFactionsModExplanation".Translate());
            listingStandard.Gap(10f); // Optional spacing
            listingStandard.CheckboxLabeled("ModSetting_EnableDebug".Translate(), ref GetSettings<ModSettings_MIMWH40kFactions>().debugMode);
            listingStandard.Gap(10f); // Optional spacing
            listingStandard.CheckboxLabeled("ModSetting_TurnOnWipeMemory".Translate(), ref GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtBlocking);
            if (GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtBlocking != cachedEnableThoughtBlocking)
            {
                listingStandard.Label("ModSetting_TurnOnWipeMemoryRestartWarning".Translate());
            }
            listingStandard.Gap(10f); // Optional spacing
            listingStandard.CheckboxLabeled("ModSetting_ThoughtFilter".Translate(), ref GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtFiltering);
            if (GetSettings<ModSettings_MIMWH40kFactions>().enableHarmonyThoughtFiltering != cachedEnableThoughtFilter)
            {
                listingStandard.Label("ModSetting_TurnOnWipeMemoryRestartWarning".Translate());
            }
            listingStandard.Gap(10f); // Optional spacing
            if (listingStandard.ButtonText("ModSetting_GlobalThoughtDefsButton".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ThoughtDefSelector(GetSettings<ModSettings_MIMWH40kFactions>().globalBlockedThoughtDefs));
            }
            listingStandard.Gap(10f); // Optional spacing
            settings.accuracyModifier = listingStandard.SliderLabeled("ModSetting_RangedAccuracyModifier".Translate() + ": " + (settings.accuracyModifier / 100f).ToStringPercent(), settings.accuracyModifier, 0f, 100f, 0.5f, "ModSetting_RangedAccuracyModifierExplanation".Translate());
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.OK.Core"))
            {
                listingStandard.Gap(20f);
                listingStandard.Label("ModSetting_OrkMaxCount".Translate() + ": " + (settings.maxOrkCount).ToString());
                listingStandard.SubLabel("ModSetting_OrkMaxCountExplanation".Translate(), innerRect.width);
                settings.maxOrkCount = (int)listingStandard.Slider(settings.maxOrkCount, 10, 300);
                listingStandard.Gap(20f);
                listingStandard.Label("ModSetting_OrkoidMaxCount".Translate() + ": " + (settings.maxOrkoidCount).ToString());
                listingStandard.SubLabel("ModSetting_OrkoidMaxCountExplanation".Translate(), innerRect.width);
                settings.maxOrkoidCount = (int)listingStandard.Slider(settings.maxOrkoidCount, 10, 300);
                listingStandard.Gap(20f);
                listingStandard.CheckboxLabeled("ModSetting_CountSporeOnMap".Translate() + ": ", ref settings.countSpore);
                listingStandard.SubLabel("ModSetting_CountSporeOnMapExplanation".Translate(), innerRect.width);
                if (settings.countSpore)
                {
                    listingStandard.Gap(20f);
                    listingStandard.Label("ModSetting_OrkoidSporeMaxCount".Translate() + ": " + (settings.maxSporeCount).ToString());
                    listingStandard.SubLabel("ModSetting_OrkoidSporeMaxCountExplanation".Translate(), innerRect.width);
                    settings.maxSporeCount = (int)listingStandard.Slider(settings.maxSporeCount, 10, 300);
                }
            }
            listingStandard.Gap(20f);
            listingStandard.Label("ModSetting_MassCapacityModifier".Translate() + ": " + (settings.massModifier / 100f).ToStringPercent());
            listingStandard.SubLabel("ModSetting_MassCapacityModifierExplanation".Translate(), innerRect.width);
            settings.massModifier = listingStandard.Slider(settings.massModifier, 1f, 100f);
            //settings.massModifier = listingStandard.SliderLabeled("ModSetting_MassCapacityModifier".Translate() + ": " + (settings.massModifier / 100f).ToStringPercent(), settings.massModifier, 1f, 100f, 0.5f, "ModSetting_MassCapacityModifierExplanation".Translate());
            listingStandard.End();
            // End the scroll view
            GUI.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ModSetting_MIMWH40kFactionsModName".Translate();
        }
    }
}