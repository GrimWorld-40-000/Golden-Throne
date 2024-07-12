using RimWorld;
using Verse;

namespace GoldenThrone
{
    public class CompProperties_AffectedByGoldenThroneFacilities : CompProperties_AffectedByFacilities
    {
        public CompProperties_AffectedByGoldenThroneFacilities()
        {
            compClass = typeof(CompAffectedByGoldenThroneFacilities);
        }
    }
    
    public class CompAffectedByGoldenThroneFacilities: CompAffectedByFacilities
    {
        public CompProperties_AffectedByGoldenThroneFacilities Props =>
            (CompProperties_AffectedByGoldenThroneFacilities)props;

        public void OnNewModuleLinked(Thing module)
        {
            if (parent is Buildings.Building_GoldenThrone throne)
            {
                throne.TryAddNewModule(module);
            }
        }

        public void OnModuleUnlinked(Thing module)
        {
            if (parent is Buildings.Building_GoldenThrone throne)
            {
                throne.TryRemoveModule(module);
            }
        }
    }
}