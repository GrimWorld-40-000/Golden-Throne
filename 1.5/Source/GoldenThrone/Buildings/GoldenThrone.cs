using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoldenThrone.Attachments;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GoldenThrone.Buildings
{
    public class Building_GoldenThrone: Building
    {
        public CompAffectedByGoldenThroneFacilities AffectedByFacilities =>
            _cachedAffectedByFacilities ??= GetComp<CompAffectedByGoldenThroneFacilities>();
        private CompAffectedByGoldenThroneFacilities _cachedAffectedByFacilities;
        
        
        public CompRoomRequirement RoomRequirement =>
            _cachedRequirement ??= GetComp<CompRoomRequirement>();
        private CompRoomRequirement _cachedRequirement;
        
        
        public static int MaxModuleCapacity = 10;

        public bool IsEnabled;

        public ThroneDisabledReason ThroneDisabledReason;


        public int TotalCapacityUsed => ActiveAttachments.Sum(attachment => attachment.CapacityCost);

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
            Scribe_Values.Look(ref IsEnabled, "GWGT_isEnabled");
        }

        public string ModuleCapacityReport => ActiveAttachments.Any() ? "GWGT.AttachedModules".Translate(TotalCapacityUsed, MaxModuleCapacity) : "GWGT.HasNoModules".Translate();

        private HashSet<CompGoldenThroneAttachment> GetActiveAttachments()
        {
            _tmpAttachmentsSet.Clear();
            
            _tmpAttachmentsSet.AddRange(Attachments.Where(attachment => _activeAttachmentThingIds.Contains(attachment.parent.thingIDNumber)));
            
            return _tmpAttachmentsSet;
        }

        private void ClearAttachmentCache()
        {
            _cachedAttachments = null;
            _cachedActiveAttachments = null;
        }

        
        public override void TickRare()
        {
            base.TickRare();

            Room room = this.GetRoom();

            if (room.OutdoorsForWork)
            {
                ThroneDisabledReason = ThroneDisabledReason.Outside;
                IsEnabled = false;
            }
            else if (!RoomRequirement.IsSatisfied)
            {
                ThroneDisabledReason = ThroneDisabledReason.BadRoom;
                IsEnabled = false;
            }
            else
            {
                IsEnabled = true;
            }
        }

        public void TryRemoveModule(Thing module)
        {
            _activeAttachmentThingIds.Remove(module.thingIDNumber);
            ClearAttachmentCache();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            Attachments.Do(attachment => attachment.Disconnect());
        }

        public void TryAddNewModule(Thing module)
        {
            //If it's a golden throne attachment, add it
            if (module.TryGetComp(out CompGoldenThroneAttachment attachment))
            {
                //If we have room for it, add it
                if (attachment.CapacityCost + TotalCapacityUsed <= MaxModuleCapacity)
                {
                    //Attach this to the module
                    attachment.AttachedThrone = this;
                    _activeAttachmentThingIds.Add(module.thingIDNumber);
                }
            }
            
            ClearAttachmentCache();
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new GoldenThroneModuleGizmo(this);
            
            foreach (var moduleGizmo in ActiveAttachments.SelectMany(compGoldenThroneAttachment => compGoldenThroneAttachment.GetModuleGizmos()))
            {
                yield return moduleGizmo;
            }
            
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder builder = new StringBuilder(base.GetInspectString());
            if (!IsEnabled) builder.AppendLineIfNotEmpty().Append("GWGT.ThroneroomNotAdequate".Translate().Colorize(Color.red));
            return builder.ToString();
        }
    }

    public enum ThroneDisabledReason
    {
        NoPower,
        Outside,
        BadRoom
    }
}