using System.Collections.Generic;
using System.Linq;
using GoldenThrone.Buildings;
using RimWorld;
using Verse;

namespace GoldenThrone.Attachments
{
    public abstract class GoldenThroneAttachmentComp: ThingComp
    {
        public Building_GoldenThrone Throne => ThroneAttachment.AttachedThrone;
        
        public CompGoldenThroneAttachment ThroneAttachment =>
            _throneAttachment ??= parent.GetComp<CompGoldenThroneAttachment>();
        private CompGoldenThroneAttachment _throneAttachment;
       
        public CompGoldenThroneOwnership ThroneOwnership =>
            _throneOwnership ??= Throne?.GetComp<CompGoldenThroneOwnership>();
        private CompGoldenThroneOwnership _throneOwnership;
        
        
        
        
        private CompPowerTrader PowerTrader => _compPowerTrader ??= _compPowerTrader = parent.GetComp<CompPowerTrader>();
        private CompPowerTrader _compPowerTrader;
        private bool PowerOn => PowerTrader.PowerOn;

        protected bool Active => PowerOn && (Throne?.IsEnabled ?? false);
        
        protected bool ThroneDisabled => !(Throne?.IsEnabled ?? false);
        
        


        private Pawn _cachedUser;
        
        //Updates the cached user
        public override void CompTick()
        {
            base.CompTick();
            
            //Rare tick
            if (!parent.IsHashIntervalTick(250)) return;
            
            if (ThroneOwnership?.AssignedPawns?.FirstOrFallback()?.psychicEntropy?.IsCurrentlyMeditating ?? false)
            {
                _cachedUser = ThroneOwnership.AssignedPawns.FirstOrFallback();
            }
            else
            {
                _cachedUser = null;
            }
        }

        public bool IsThroneOccupied(out Pawn user)
        {
            user = _cachedUser;

            if (user == null) return false;
            if (user.Dead || !user.Spawned)
            {
                user = null;
                _cachedUser = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Yields all gizmos created by attached modules.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Gizmo> GetModuleGizmos()
        {
            yield break;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in GetModuleGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
        }
    }
}