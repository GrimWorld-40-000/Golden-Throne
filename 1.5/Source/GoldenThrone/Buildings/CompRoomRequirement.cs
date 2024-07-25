using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GoldenThrone.Buildings
{
    public class CompProperties_RoomRequirement : CompProperties
    {
        public List<RoomRequirement> throneRoomRequirements;

        public CompProperties_RoomRequirement()
        {
            compClass = typeof(CompRoomRequirement);
        }
    }
    
    public class CompRoomRequirement: ThingComp
    {
        public bool IsSatisfied
        {
            get
            {
                Room room = parent.GetRoom();
                return ((CompProperties_RoomRequirement)props).throneRoomRequirements.All(requirement =>
                    requirement.Met(room));
            }
        }
    }
}