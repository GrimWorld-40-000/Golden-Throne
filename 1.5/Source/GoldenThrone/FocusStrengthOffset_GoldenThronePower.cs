using System.Linq;
using GoldenThrone.Attachments;
using GoldenThrone.Buildings;
using RimWorld;
using Verse;

namespace GoldenThrone
{
    public class FocusStrengthOffset_GoldenThronePower: FocusStrengthOffset
    {
        //public CompGoldenThrone attachedThrone
        
        //public Buildings.GoldenThrone Throne => _cachedThrone ??= c as Buildings.GoldenThrone;
        //private Buildings.GoldenThrone _cachedThrone;

        public override string GetExplanation(Thing parent)
        {
            return "GWGT.PsydrainCoffinStatExplanationAbstract".Translate() + ": " +
                   GetOffset(parent).ToStringWithSign("0%");
        }


        public override string GetExplanationAbstract(ThingDef def = null)
        {
            return "GWGT.PsydrainCoffinStatExplanationAbstract".Translate() + ": " + offset.ToStringWithSign("0%");
        }
        

        public override float GetOffset(Thing parent, Pawn user = null)
        {
            return (parent as Building_GoldenThrone).ActiveAttachments.Sum(attachment => attachment.parent.GetComp<CompPsydrainCoffin>()?.PsyfocusBonus ?? 0);
        }


        public override bool CanApply(Thing parent, Pawn user = null)
        {
            return parent.Spawned && parent is Building_GoldenThrone buildingThrone && buildingThrone.ActiveAttachments.Any(attachment => attachment.parent.HasComp<CompPsydrainCoffin>());
        }
    }
}