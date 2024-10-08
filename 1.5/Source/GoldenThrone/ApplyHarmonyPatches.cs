using System.Collections.Generic;
using System.Linq;
using GoldenThrone.Buildings;
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
            Harmony harmony = new Harmony("Grimworld.GoldenThrone");

            //Calls the special implementation when a thing is linked to the Golden Throne
            harmony.Patch(
                AccessTools.Method(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.Notify_NewLink)),
                new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostFacilityLinkedToGoldenThrone)));

            //Calls the special implementation when a thing is unlinked from the Golden Throne
            harmony.Patch(
                AccessTools.Method(typeof(CompAffectedByFacilities), nameof(CompAffectedByFacilities.Notify_LinkRemoved)),
                new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostFacilityUnlinkedFromGoldenThrone)));
            
            //Pawns prefer the Golden Throne over everything
            harmony.Patch(
                AccessTools.Method(typeof(MeditationUtility), nameof(MeditationUtility.AllMeditationSpotCandidates)),
                prefix: new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PreGetMeditationUtility)));
            
            //Meditate at spot
            harmony.Patch(
                AccessTools.Method(typeof(MeditationUtility), nameof(MeditationUtility.FindMeditationSpot)),
                postfix: new HarmonyMethod(typeof(ApplyHarmonyPatches), nameof(PostFindMeditationSpot)));
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
        private static bool PreGetMeditationUtility(ref IEnumerable<LocalTargetInfo> __result, Pawn pawn, bool allowFallbackSpots = true)
        {
            if (!TryGetGoldenThroneSpot(pawn, out LocalTargetInfo targetInfo)) return true;
            if (targetInfo == null) return true;
            __result = new[] { targetInfo };
            return false;

        }

        private static void PostFindMeditationSpot(ref MeditationSpotAndFocus __result, Pawn pawn)
        {
            if (__result.focus.Thing?.def != GWGT_DefsOf.GWGT_GoldenThrone) return;
            __result.spot = __result.focus.Thing?.InteractionCell ?? __result.focus.Cell;
        }

        private static bool TryGetGoldenThroneSpot(Pawn pawn, out LocalTargetInfo targetInfo)
        {
            
            
            foreach (Building building in pawn.Map.listerBuildings.AllBuildingsColonistOfDef(GWGT_DefsOf.GWGT_GoldenThrone).Where(building => building.GetComp<CompGoldenThroneOwnership>().AssignedPawns?.Contains(pawn) ?? false))
            {
                if (!MeditationUtility.IsValidMeditationBuildingForPawn(building, pawn)) continue;
                targetInfo = new LocalTargetInfo(building);
                return true;
            }

            targetInfo = null;
            return false;
        }
    }
}