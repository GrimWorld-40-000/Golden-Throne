using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace GoldenThrone.Attachments
{
    public class CompProperties_ThroneFarskipReturnAbility : CompProperties_AbilityEffect
    {
        public IntRange stunTicks;
        public int ageCostYears;

        public CompProperties_ThroneFarskipReturnAbility() => compClass = typeof(CompThroneFarskipReturnAbilityEffect);
    }

    public class CompThroneFarskipReturnAbilityEffect : CompAbilityEffect
    {
        private CompProperties_ThroneFarskipReturnAbility Props => (CompProperties_ThroneFarskipReturnAbility)props;

        public override void Apply(GlobalTargetInfo target)
        {
            //Age up user
            parent.pawn.ageTracker.AgeTickMothballed(3600000 * Props.ageCostYears);
            
            Map targetMap = (target.WorldObject as MapParent)?.Map;
            IntVec3 targetCell = parent.pawn.Position;
            List<Pawn> list = AlliedPawnsOnMap(targetMap).ToList();
            Map homeMap = parent.pawn.Map;
            

            foreach (Pawn pawn in list)
            {
                if (pawn.Spawned)
                {
                    pawn.teleporting = true;
                    pawn.ExitMap(false, Rot4.Invalid);
                    AbilityUtility.DoClamor(pawn.Position, Props.clamorRadius, parent.pawn,
                        Props.clamorType);
                    pawn.teleporting = false;
                }

                CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, homeMap, out var result,
                    extraValidator: cell =>
                        cell != targetCell && cell.GetRoom(homeMap) == targetCell.GetRoom(homeMap));
                GenSpawn.Spawn(pawn, result, homeMap);
                if (pawn.drafter != null && pawn.IsColonistPlayerControlled && !pawn.Downed)
                    pawn.drafter.Drafted = true;
                pawn.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, false);
                pawn.Notify_Teleported();
                CompAbilityEffect_Teleport.SendSkipUsedSignal((LocalTargetInfo)(Thing)pawn,
                    parent.pawn);
                if (pawn.IsPrisoner)
                    pawn.guest.WaitInsteadOfEscapingForDefaultTicks();
                parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(pawn, pawn.Map),
                    pawn.Position, 60, targetMap);
                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(result, pawn.Map));
                if ((pawn.IsColonist || pawn.RaceProps.packAnimal || pawn.IsColonyMech) && pawn.Map.IsPlayerHome)
                    pawn.inventory.UnloadEverything = true;
            }
        }


        private bool IsAlliedPawnOnMap(Map targetMap)
        {
            return targetMap.mapPawns.AllPawnsSpawned.Any(p =>
                !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer);
        }

        private IEnumerable<Pawn> AlliedPawnsOnMap(Map targetMap)
        {
            return targetMap.mapPawns.AllPawnsSpawned.Where(p =>
                !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer);
        }

        private bool ShouldEnterMap(GlobalTargetInfo target)
        {
            if (target.WorldObject is Caravan worldObject1 && worldObject1.Faction == parent.pawn.Faction ||
                target.WorldObject is not MapParent { HasMap: true } worldObject2)
                return false;
            return IsAlliedPawnOnMap(worldObject2.Map) || worldObject2.Map == parent.pawn.Map;
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            return ShouldEnterMap(target) && base.Valid(target, throwMessages);
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return (target.WorldObject is not MapParent worldObject || worldObject.Map == null ||
                    IsAlliedPawnOnMap(worldObject.Map)) && base.CanApplyOn(target) && target.WorldObject is not Caravan;
        }

        public override string WorldMapExtraLabel(GlobalTargetInfo target)
        {
            return !Valid(target) ? "GWGT.AbilityMustSkipAllies".Translate() : "GWGT.AbilitySkipAllies".Translate();
        }
    }
}