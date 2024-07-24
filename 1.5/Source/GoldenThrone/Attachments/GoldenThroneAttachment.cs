using System.Text;
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

        public Buildings.Building_GoldenThrone AttachedThrone;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref AttachedThrone, "GWGT_attachedThrone");
        }

        public override string CompInspectStringExtra()
        {
            return AttachedThrone == null ? new StringBuilder(base.CompInspectStringExtra()).AppendLineIfNotEmpty().Append("GWGT.NoThroneAttached".Translate().Colorize(Color.red)).ToString() : base.CompInspectStringExtra();
        }

        public void Disconnect()
        {
            AttachedThrone = null;
        }

        public override void PostDraw()
        {
            if (AttachedThrone == null) return;
            
            Vector3 a = AttachedThrone.TrueCenter();
            Vector3 b = parent.TrueCenter();
            a.y = b.y = AltitudeLayer.SmallWire.AltitudeFor();
            Matrix4x4 identity = Matrix4x4.identity;
            identity.SetTRS((a + b) / 2f, Quaternion.Euler(0.0f, a.AngleToFlat(b) + 90f, 0.0f), new Vector3(1f, 1f, (a - b).MagnitudeHorizontal()));
            Graphics.DrawMesh(MeshPool.plane10, identity, HoseMat, 0);
        }
    }
}