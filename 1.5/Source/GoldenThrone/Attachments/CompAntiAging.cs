using RimWorld;
using Verse;

namespace GoldenThrone.Attachments
{
    public class CompProperties_AntiAging: CompProperties
    {
        public int agingMultiplier;
        
        public CompProperties_AntiAging()
        {
            compClass = typeof(CompAntiAging);
        }   
    }


    public class CompAntiAging: GoldenThroneAttachmentComp
    {
        public CompProperties_AntiAging Props => (CompProperties_AntiAging)props;
        
        
        
        public override void CompTick()
        {
            base.CompTick();
            if(!Active) return;
            if (parent.IsHashIntervalTick(Props.agingMultiplier)) return;
            if (IsThroneOccupied(out Pawn user))
            {
                user.ageTracker.AgeBiologicalTicks -= 1;
            }
        }
    }
}