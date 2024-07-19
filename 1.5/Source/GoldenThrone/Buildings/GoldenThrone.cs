using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoldenThrone.Attachments;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GoldenThrone.Buildings
{
    public class Building_GoldenThrone: Building
    {
        public CompAffectedByGoldenThroneFacilities AffectedByFacilities =>
            _cachedAffectedByFacilities ??= GetComp<CompAffectedByGoldenThroneFacilities>();
        private CompAffectedByGoldenThroneFacilities _cachedAffectedByFacilities;
        
        public static int MaxModuleCapacity = 10;

        public int totalCapacityUsed => ActiveAttachments.Sum(attachment => attachment.CapacityCost);

        public List<CompGoldenThroneAttachment> ActiveAttachments => _cachedActiveAttachments ??= GetActiveAttachments().ToList();
        private List<CompGoldenThroneAttachment> _cachedActiveAttachments;

        private HashSet<int> _activeAttachmentThingIds = new HashSet<int>();
        
        public List<CompGoldenThroneAttachment> Attachments => _cachedAttachments ??= GetAttachments();
        private List<CompGoldenThroneAttachment> _cachedAttachments;

        private readonly List<CompGoldenThroneAttachment> _tmpAttachments = new List<CompGoldenThroneAttachment>();
        private readonly HashSet<CompGoldenThroneAttachment> _tmpAttachmentsSet = new HashSet<CompGoldenThroneAttachment>();
        private List<CompGoldenThroneAttachment> GetAttachments()
        {
            _tmpAttachments.Clear();
            foreach (Thing attachedBuilding in AffectedByFacilities.LinkedFacilitiesListForReading)
            {
                if (attachedBuilding.TryGetComp(out CompGoldenThroneAttachment attachment))
                {
                    _tmpAttachments.Add(attachment);
                }
            }
            return _tmpAttachments.ToList();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _activeAttachmentThingIds, "GWGT_attachedModules");
        }

        public string ModuleCapacityReport => ActiveAttachments.Any() ? "GWGT.AttachedModules".Translate(totalCapacityUsed, MaxModuleCapacity) : "GWGT.HasNoModules".Translate();

        private HashSet<CompGoldenThroneAttachment> GetActiveAttachments()
        {
            _tmpAttachmentsSet.Clear();
            
            _tmpAttachmentsSet.AddRange(Attachments.Where(attachment => _activeAttachmentThingIds.Contains(attachment.parent.thingIDNumber)));
            
            return _tmpAttachmentsSet;
        }

        public void ClearAttachmentCache()
        {
            _cachedAttachments = null;
            _cachedActiveAttachments = null;
        }

        public void TryRemoveModule(Thing module)
        {
            _activeAttachmentThingIds.Remove(module.thingIDNumber);
            ClearAttachmentCache();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            Attachments.Do(attachment => attachment.attachedThrone = null);
        }

        public void TryAddNewModule(Thing module)
        {
            //If it's a golden throne attachment, add it
            if (module.TryGetComp(out CompGoldenThroneAttachment attachment))
            {
                //If we have room for it, add it
                if (attachment.CapacityCost + totalCapacityUsed <= MaxModuleCapacity)
                {
                    //Attach this to the module
                    attachment.attachedThrone = this;
                    _activeAttachmentThingIds.Add(module.thingIDNumber);
                }
            }
            ClearAttachmentCache();
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new GoldenThroneModuleGizmo(this);
            
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }
    }
}