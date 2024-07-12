using HarmonyLib;
using RimWorld;
using Verse;

namespace GoldenThrone
{
    [StaticConstructorOnStartup]
    public static class ApplyHarmonyPatches
    {
        static ApplyHarmonyPatches()
        {
            Harmony harmony = new Harmony("Grimmworld.GoldenThrone");

            //Calls the special implementation when a thing is linked to the Golden Throne
            harmony.Patch(
                AccessTools.Method(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.Notify_NewLink)),
                new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostFacilityLinkedToGoldenThrone)));

            //Calls the special implementation when a thing is unlinked from the Golden Throne
            harmony.Patch(
                AccessTools.Method(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.Notify_LinkRemoved)),
                new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostFacilityUnlinkedFromGoldenThrone)));
        }

        private static void PostFacilityLinkedToGoldenThrone(CompAffectedByFacilities __instance, Thing facility)
        {
            if (__instance is CompAffectedByGoldenThroneFacilities goldenThrone)
            {
                goldenThrone.OnNewModuleLinked(facility);
            }
        }
        private static void PostFacilityUnlinkedFromGoldenThrone(CompAffectedByFacilities __instance, Thing thing)
        {
            if (__instance is CompAffectedByGoldenThroneFacilities goldenThrone)
            {
                goldenThrone.OnModuleUnlinked(thing);
            }
        }
    }
}