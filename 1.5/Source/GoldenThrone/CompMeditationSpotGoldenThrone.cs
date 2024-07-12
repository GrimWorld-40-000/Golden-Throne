using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GoldenThrone
{
    public class CompProperties_MeditationFocusGoldenThrone: CompProperties_MeditationFocus
    {
        public CompProperties_MeditationFocusGoldenThrone()
        {
            compClass = typeof(CompMeditationFocusGoldenThrone);
        }
    }


    public class CompMeditationFocusGoldenThrone : CompMeditationFocus
    {
        public CompProperties_MeditationFocusGoldenThrone Props => (CompProperties_MeditationFocusGoldenThrone)props;
    }
}