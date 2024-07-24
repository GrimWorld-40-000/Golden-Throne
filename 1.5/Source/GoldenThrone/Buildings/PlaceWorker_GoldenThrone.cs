using Verse;

namespace GoldenThrone.Buildings
{
    public class PlaceWorker_GoldenThrone: PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null,
            Thing thing = null)
        {
            return map.listerBuildings.AllBuildingsColonistOfDef(GWGT_DefsOf.GWGT_GoldenThrone).Any() ? "GWGT.ThroneAlreadyExists".Translate() : base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
        }
        
    }
}