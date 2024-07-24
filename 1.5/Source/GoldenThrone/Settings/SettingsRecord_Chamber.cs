using GoldenThrone.Buildings;
using GW_Frame.Settings;
using Verse;

namespace GoldenThrone.Settings
{
    public class SettingsRecord_GoldenThrone: SettingsRecord
    {
        public override void CastChanges() { }

        public override void Reset()
        {
            Building_GoldenThrone.MaxModuleCapacity = 10;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Building_GoldenThrone.MaxModuleCapacity, "throneModuleCapacity",10,true);
        }
    }
}