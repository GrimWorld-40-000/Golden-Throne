using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GoldenThrone.Attachments
{
    public class CompProperties_LifeSupport: CompProperties
    {
        public List<NeedDef> needsProvidedFor;
        
        public CompProperties_LifeSupport()
        {
            compClass = typeof(CompLifeSupport);
        }   
    }


    public class CompLifeSupport: GoldenThroneAttachmentComp
    {
        public CompProperties_LifeSupport Props => (CompProperties_LifeSupport)props;
        
        
        
        
        private CompPowerTrader PowerTrader => _compPowerTrader ??= _compPowerTrader = parent.GetComp<CompPowerTrader>();
        private CompPowerTrader _compPowerTrader;
        private bool PowerOn => PowerTrader.PowerOn;
        
        
        
        public override void CompTick()
        {
            base.CompTick();
            if (!parent.IsHashIntervalTick(250)) return;
            if (!PowerOn) return;
            if (!IsThroneOccupied(out Pawn user)) return;
            foreach (var need in Props.needsProvidedFor.Select(needDef => user.needs.TryGetNeed(needDef)).Where(need => need != null))
            {
                need.CurLevel += need.MaxLevel / 30;
            }
        }
    }
}