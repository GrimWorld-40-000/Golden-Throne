using RimWorld;
using UnityEngine;
using Verse;

namespace GoldenThrone.Attachments
{
    public class CompProperties_GoldenThroneAttachment: CompProperties_Facility
    {
        public int capacityCost;
        
        public CompProperties_GoldenThroneAttachment()
        {
            compClass = typeof(CompGoldenThroneAttachment);
        }
    }

    /// <summary>
    /// Base class for all attachments
    /// </summary>
    public class CompGoldenThroneAttachment : CompFacility
    {
        [Unsaved]
        private Material _cachedHoseMat;
        
        private Material HoseMat
        {
            get
            {
                if (_cachedHoseMat == null)
                    _cachedHoseMat = MaterialPool.MatFrom("Other/DeathrestBuildingConnection");
                return _cachedHoseMat;
            }
        }
        
        public CompProperties_GoldenThroneAttachment Props => (CompProperties_GoldenThroneAttachment) props;

        public int CapacityCost => Props.capacityCost;

        public Buildings.Building_GoldenThrone attachedThrone;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref attachedThrone, "GWGT_attachedThrone");
        }

        public override void PostDraw()
        {
            if (attachedThrone == null) return;
            
            Vector3 a = attachedThrone.TrueCenter();
            Vector3 b = parent.TrueCenter();
            a.y = b.y = AltitudeLayer.SmallWire.AltitudeFor();
            Matrix4x4 identity = Matrix4x4.identity;
            identity.SetTRS((a + b) / 2f, Quaternion.Euler(0.0f, a.AngleToFlat(b) + 90f, 0.0f), new Vector3(1f, 1f, (a - b).MagnitudeHorizontal()));
            Graphics.DrawMesh(MeshPool.plane10, identity, HoseMat, 0);
        }
    }
}