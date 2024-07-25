using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace GoldenThrone.Attachments
{
    public class CompProperties_ThroneFarskipAbility : CompProperties_AbilityEffect
    {
        public IntRange stunTicks;
        public int ageCostYears;

        public CompProperties_ThroneFarskipAbility() => compClass = typeof(CompThroneFarskipAbilityEffect);
    }

    public class CompThroneFarskipAbilityEffect : CompAbilityEffect
    {
        private CompProperties_ThroneFarskipAbility Props => (CompProperties_ThroneFarskipAbility)props;

        public override void Apply(GlobalTargetInfo target)
        {
            //Age up user
            parent.pawn.ageTracker.AgeTickMothballed(3600000 * Props.ageCostYears);
            
            Map targetMap = (target.WorldObject as MapParent)?.Map;
            IntVec3 targetCell = IntVec3.Invalid;
            List<Pawn> list = PawnsToSkip().ToList();
            
            foreach (Pawn target1 in list)
                parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(target1, target1.Map),
                    target1.Position, 60);
            SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
            

            if (ShouldEnterMap(target))
            {
                Pawn pawn = AlliedPawnOnMap(targetMap);
                targetCell = pawn?.Position ?? parent.pawn.Position;
            }

            if (targetCell.IsValid)
            {
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

                    CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result,
                        extraValidator: cell =>
                            cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
                    GenSpawn.Spawn(pawn, result, targetMap);
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
            else if ((target.WorldObject is not Caravan worldObject ? 0 : worldObject.Faction == parent.pawn.Faction ? 1 : 0) != 0)
            {
                foreach (Pawn p in list)
                {
                    worldObject = target.WorldObject as Caravan;
                    worldObject?.AddPawn(p, true);
                    p.ExitMap(false, Rot4.Invalid);
                    AbilityUtility.DoClamor(p.Position, Props.clamorRadius, parent.pawn, Props.clamorType);
                }
            }
            
            CaravanMaker.MakeCaravan(list, parent.pawn.Faction, target.Tile, false);
            foreach (Pawn pawn in list)
                pawn.ExitMap(false, Rot4.Invalid);
            
        }

        private IEnumerable<Pawn> PawnsToSkip()
        {
            bool homeMap = parent.pawn.Map.IsPlayerHome;
            bool sentLodgerMessage = false;
            foreach (Thing thing in GenRadial.RadialDistinctThingsAround(parent.pawn.Position,
                         parent.pawn.Map, parent.def.EffectRadius, true))
            {
                if (thing is not Pawn p) continue;
                //Does not skip the caster
                if (p == parent.pawn) continue;
                if (p.IsQuestLodger())
                {
                    if (!sentLodgerMessage)
                        Messages.Message("MessageLodgersCantFarskip".Translate(), (Thing)p,
                            MessageTypeDefOf.NegativeEvent, false);
                    sentLodgerMessage = true;
                }
                else if (!p.Dead && (p.IsColonist || p.IsPrisonerOfColony ||
                                     !homeMap && p.IsNonMutantAnimal && p.Faction is { IsPlayer: true }))
                    yield return p;
            }
        }

        private Pawn AlliedPawnOnMap(Map targetMap)
        {
            return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault(p =>
                !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer &&
                !PawnsToSkip().Contains(p));
        }

        private bool ShouldEnterMap(GlobalTargetInfo target)
        {
            if (target.WorldObject is Caravan worldObject1 && worldObject1.Faction == parent.pawn.Faction ||
                target.WorldObject is not MapParent { HasMap: true } worldObject2)
                return false;
            return AlliedPawnOnMap(worldObject2.Map) != null || worldObject2.Map == parent.pawn.Map;
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            return (ShouldEnterMap(target)) && base.Valid(target, throwMessages);
        }

        public override bool CanApplyOn(GlobalTargetInfo target)
        {
            return (target.WorldObject is not MapParent worldObject || worldObject.Map == null ||
                    AlliedPawnOnMap(worldObject.Map) != null) && base.CanApplyOn(target) && target.WorldObject is not Caravan;
        }

        public override Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
        {
            Pawn pawn = PawnsToSkip().FirstOrDefault(p => p.IsQuestLodger());
            return pawn != null
                ? Dialog_MessageBox.CreateConfirmation(
                    "FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), confirmAction)
                : (Window)null;
        }

        public override string WorldMapExtraLabel(GlobalTargetInfo target)
        {
            return !Valid(target) ? "AbilityNeedAllyToSkip".Translate() : "AbilitySkipToRandomAlly".Translate();
        }
    }
}