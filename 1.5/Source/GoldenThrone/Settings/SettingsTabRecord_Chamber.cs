using System;
using GoldenThrone.Buildings;
using GW_Frame.Settings;
using UnityEngine;
using Verse;

namespace GoldenThrone.Settings
{
    public class SettingsTabRecord_GoldenThrone: SettingsTabRecord
    {
        private SettingsRecord_GoldenThrone settingsRecord;
        public SettingsRecord_GoldenThrone SettingsRecord
        {
            get
            {
                if (settingsRecord == null)
                {
                    GW_Frame.Settings.Settings.Instance.TryGetModSettings(typeof(SettingsRecord_GoldenThrone), out SettingsRecord settingsRecord);
                    this.settingsRecord = settingsRecord as SettingsRecord_GoldenThrone;
                }
                return settingsRecord;
            }
        }
        
        
        public SettingsTabRecord_GoldenThrone(SettingsTabDef def, string label, Action clickedAction, Func<bool> selected) : base(def, label, clickedAction, selected)
        {
            
        }


        public override void OnGUI(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect.ContractedBy(20f));
            listingStandard.Gap();

            int moduleCapacity = Building_GoldenThrone.MaxModuleCapacity;
            
            
            listingStandard.Label("GWGT.ModuleCapacitySetting".Translate(moduleCapacity));
            Building_GoldenThrone.MaxModuleCapacity = (int)listingStandard.Slider(moduleCapacity, 1, 100);
            
            
            listingStandard.End();
        }
    }
}