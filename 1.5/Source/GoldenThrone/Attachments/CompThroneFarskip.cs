using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GoldenThrone.Attachments
{
    public class CompProperties_ThroneFarskip: CompProperties
    {
        public CompProperties_ThroneFarskip()
        {
            compClass = typeof(CompThroneFarskip);
        }   
    }


    public class CompThroneFarskip: GoldenThroneAttachmentComp
    {
        
        public CompProperties_ThroneFarskip Props => (CompProperties_ThroneFarskip)props;
        

        public override IEnumerable<Gizmo> GetModuleGizmos()
        {
            if (!IsThroneOccupied(out Pawn pawn)) yield break;
            if (ThroneDisabled) yield break;
            Command_Ability farskip = new Command_Ability(AbilityUtility.MakeAbility(GWGT_DefsOf.GWGT_ThroneFarskip, pawn), pawn);
            if (!Active) farskip.Disabled = true;

            yield return farskip;
        }
    }
}