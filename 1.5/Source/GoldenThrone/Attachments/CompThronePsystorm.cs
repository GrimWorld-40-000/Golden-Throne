using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GoldenThrone.Attachments
{
    public class CompProperties_ThronePsystorm: CompProperties
    {
        public int ticksCooldown;
        
        public CompProperties_ThronePsystorm()
        {
            compClass = typeof(CompThronePsystorm);
        }   
    }


    public class CompThronePsystorm: GoldenThroneAttachmentComp
    {
        
        public CompProperties_ThronePsystorm Props => (CompProperties_ThronePsystorm)props;

        public static int LastCastTick;

        private bool CanCast => GenTicks.TicksGame - LastCastTick > Props.ticksCooldown && Active;

        public override IEnumerable<Gizmo> GetModuleGizmos()
        {
            if (ThroneDisabled) yield break;
            if (!IsThroneOccupied(out Pawn pawn)) yield break;
            Command_Ability psystorm = new Command_Ability(AbilityUtility.MakeAbility(GWGT_DefsOf.GWGT_Psystorm, pawn), pawn);
            if (!CanCast) psystorm.Disabled = true;

            yield return psystorm;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref LastCastTick, "lastCastTick");
        }
    }
}