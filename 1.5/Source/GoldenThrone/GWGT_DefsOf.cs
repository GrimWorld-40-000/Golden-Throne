using RimWorld;
using Verse;

namespace GoldenThrone
{
    [DefOf]
    public class GWGT_DefsOf
    {
        public static HediffDef GWGT_PsydrainShock;
        public static JobDef GWGT_EnterPsydrainCoffin;
        public static ThingDef GWGT_GoldenThrone;
        public static AbilityDef GWGT_ThroneFarskip;
        public static AbilityDef GWGT_ThroneFarskipReturn;
        public static AbilityDef GWGT_Psystorm;
        public static ThoughtDef GWGT_PsystormEcho;
        
        

        static GWGT_DefsOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(GWGT_DefsOf));
        }
    }
}