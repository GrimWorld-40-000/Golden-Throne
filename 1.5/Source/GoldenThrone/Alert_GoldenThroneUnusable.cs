using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoldenThrone.Buildings;
using RimWorld;
using Verse;

namespace GoldenThrone
{
    public class Alert_GoldenThroneUnusable: Alert
    {
        public Building_GoldenThrone Throne
        {
            get
            {
                List<Map> maps = Find.Maps;

                return (from map in maps select map.listerBuildings.AllBuildingsColonistOfDef(GWGT_DefsOf.GWGT_GoldenThrone) into thrones where thrones.Any(throne => !((Building_GoldenThrone)throne).IsEnabled) select (Building_GoldenThrone)thrones.First()).FirstOrDefault();
            }
        }

        public override AlertReport GetReport() => AlertReport.CulpritIs(Throne);

        public override TaggedString GetExplanation()
        {
            StringBuilder explanation = new StringBuilder();
            switch (Throne?.ThroneDisabledReason)
            {
                case ThroneDisabledReason.NoPower:
                    explanation.AppendLineIfNotEmpty().Append("GWGT.Alert_NoPower".Translate());
                    break;
                case ThroneDisabledReason.Outside:
                    explanation.AppendLineIfNotEmpty().Append("GWGT.Alert_Outside".Translate());
                    break;
                case ThroneDisabledReason.BadRoom:
                    explanation.AppendLineIfNotEmpty().Append("GWGT.Alert_BadRoom".Translate());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return "GWGT.Alert_ThroneUnusableDescription".Translate(explanation);
        }

        public override string GetLabel() => "GWGT.Alert_ThroneUnusable".Translate();
    }
}