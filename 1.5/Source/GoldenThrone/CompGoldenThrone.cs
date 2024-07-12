using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GoldenThrone
{
    public class CompProperties_GoldenThrone: CompProperties_AssignableToPawn
    {
        public float minimumSensitivityToUse;
        
        public CompProperties_GoldenThrone()
        {
            compClass = typeof(CompGoldenThrone);
        }
    }


    public class CompGoldenThrone : CompAssignableToPawn
    {
        public CompProperties_GoldenThrone Props => (CompProperties_GoldenThrone)props;

        public override IEnumerable<Pawn> AssigningCandidates => !parent.Spawned
            ? Enumerable.Empty<Pawn>()
            : parent.Map.mapPawns.FreeColonists.Where(pawn =>
                pawn.GetStatValue(StatDefOf.PsychicSensitivity) >= Props.minimumSensitivityToUse);
    }
}