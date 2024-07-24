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
        
        
        private CompPowerTrader PowerTrader => _compPowerTrader ??= _compPowerTrader = parent.GetComp<CompPowerTrader>();
        private CompPowerTrader _compPowerTrader;
        private bool PowerOn => PowerTrader.PowerOn;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!IsThroneOccupied(out Pawn pawn)) yield break;
            Command_Ability farskip = new Command_Ability(AbilityUtility.MakeAbility(GWGT_DefsOf.GWGT_ThroneFarskip, pawn), pawn);
            if (!PowerOn) farskip.Disabled = true;

            yield return farskip;
                
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
        }
    }
}