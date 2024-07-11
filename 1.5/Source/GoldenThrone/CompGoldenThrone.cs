using RimWorld;

namespace GoldenThrone
{
    public class CompProperties_GoldenThrone: CompProperties_AssignableToPawn
    {
        public CompProperties_GoldenThrone()
        {
            compClass = typeof(CompGoldenThrone);
        }
    }


    public class CompGoldenThrone : CompAssignableToPawn
    {
        
    }
}