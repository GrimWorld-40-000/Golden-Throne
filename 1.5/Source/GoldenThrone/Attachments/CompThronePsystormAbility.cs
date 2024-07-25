using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace GoldenThrone.Attachments
{
    public class CompProperties_ThronePsystormAbility  : CompProperties_AbilityEffect
    {
        public int goodwillImpactForKill;
        public int goodwillImpactForCast;
        public int worldRangeTiles;
        public float attackRadius;

        public IntRange hitsRange;
        public FloatRange damageRange;

        public CompProperties_ThronePsystormAbility() => compClass = typeof(CompThronePsystormAbilityEffect);
    }

    public class CompThronePsystormAbilityEffect : CompAbilityEffect
    {
        private CompProperties_ThronePsystormAbility Props => (CompProperties_ThronePsystormAbility)props;


        private Dictionary<Faction, Pair<bool, Pawn>> _affectedFactions;
        private readonly List<Pawn> _targetsToAttack = new List<Pawn>();
        private static readonly List<IntVec3> CachedRadiusCells = new List<IntVec3>();
        private static IntVec3? _cachedRadiusCellsTarget = new IntVec3?();
        private static readonly Map CachedRadiusCellsMap = null;


        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (_affectedFactions == null)
              _affectedFactions = new Dictionary<Faction, Pair<bool, Pawn>>();
            else
              _affectedFactions.Clear();
            _targetsToAttack.Clear();
            foreach (Pawn pawn in parent.pawn.Map.mapPawns.AllPawnsSpawned)
            {
              if (!CanApplyEffects(pawn) || pawn.Fogged()) continue;
              bool flag = !pawn.Spawned ||
                          pawn.Position.InHorDistOf(parent.pawn.Position, parent.def.EffectRadius) ||
                          !pawn.Position.InHorDistOf(parent.pawn.Position, Props.attackRadius);
              AffectGoodwill(pawn.HomeFaction, !flag, pawn);
              if (!flag)
                _targetsToAttack.Add(pawn);
              else
                GivePsystormThought(pawn);
            }

            foreach (var allPawn in Find.Maps.Where(map => map != parent.pawn.Map &&
                                                           Find.WorldGrid.TraversalDistanceBetween(map.Tile,
                                                             parent.pawn.Map.Tile,
                                                             maxDist: Props.worldRangeTiles + 1) <= Props.worldRangeTiles)
                       .SelectMany(map => map.mapPawns.AllPawns.Where(CanApplyEffects)))
            {
              GivePsystormThought(allPawn);
            }

            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                if (Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, parent.pawn.Map.Tile,
                      maxDist: Props.worldRangeTiles + 1) > Props.worldRangeTiles) continue;
                foreach (Pawn pawn in caravan.pawns)
                {
                    if (CanApplyEffects(pawn))
                      GivePsystormThought(pawn);
                }
            }
            
            _targetsToAttack.Do(p => InjurePawn(p.GetStatValue(StatDefOf.PsychicSensitivity), p));

            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
              if (!allFaction.IsPlayer && !allFaction.defeated && !allFaction.HostileTo(Faction.OfPlayer))
                AffectGoodwill(allFaction, false);
            }

            if (parent.pawn.Faction == Faction.OfPlayer)
            {
              foreach (KeyValuePair<Faction, Pair<bool, Pawn>> affectedFaction in _affectedFactions)
              {
                Faction key = affectedFaction.Key;
                Pair<bool, Pawn> pair = affectedFaction.Value;
                int num = pair.First ? 1 : 0;
                int goodwillChange =
                  num != 0 ? Props.goodwillImpactForKill : Props.goodwillImpactForCast;
                Faction.OfPlayer.TryAffectGoodwillWith(key, goodwillChange, reason: HistoryEventDefOf.UsedHarmfulAbility);
              }
            }

            base.Apply(target, dest);
            _affectedFactions.Clear();
            _targetsToAttack.Clear();
        }

        private void AffectGoodwill(Faction faction, bool attackedSomeone, Pawn p = null)
        {
          if (faction == null || faction.IsPlayer || faction.HostileTo(Faction.OfPlayer) ||
              p is { IsSlaveOfColony: true } || _affectedFactions.TryGetValue(faction, out var pair) &&
              !(!pair.First & attackedSomeone)) return;
          _affectedFactions[faction] = new Pair<bool, Pawn>(attackedSomeone, p);
        }

        private static void GivePsystormThought(Pawn p)
        {
          p.needs?.mood?.thoughts.memories.TryGainMemory(GWGT_DefsOf.GWGT_PsystormEcho);
        }

        private static bool CanApplyEffects(Pawn p)
        {
          return !p.kindDef.isBoss && !p.Dead && !p.Suspended && p.RaceProps.intelligence >= Intelligence.Humanlike;
        }

        private void InjurePawn(float sensitivityDamageBonus, Pawn pawn)
        {
            //Random injury
            for (int i = 0; i < Props.hitsRange.RandomInRange; i++)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Psychic, Props.damageRange.RandomInRange * (1 + sensitivityDamageBonus), hitPart: pawn.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Psychic)));
            }
            
            //Hit the brain
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Psychic, brain.def.GetMaxHealth(pawn) * sensitivityDamageBonus, hitPart: brain));

            
            RestUtility.WakeUp(pawn);
        }

        public override void OnGizmoUpdate()
        {
            if (!_cachedRadiusCellsTarget.HasValue ||
                _cachedRadiusCellsTarget.Value == parent.pawn.Position ||
                CachedRadiusCellsMap != parent.pawn.Map)
            {
                CachedRadiusCells.Clear();
                foreach (IntVec3 allCell in parent.pawn.Map.AllCells)
                {
                    if (allCell.InHorDistOf(parent.pawn.Position, Props.attackRadius))
                      CachedRadiusCells.Add(allCell);
                }

                _cachedRadiusCellsTarget = parent.pawn.Position;
            }

            GenDraw.DrawFieldEdges(CachedRadiusCells);
        }
    }
}