using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace GoldenThrone.Attachments
{
    public class CompProperties_PsydrainCoffin: CompProperties
    {
        public float daysToKill;
        public float basePsyfocusGain;
        
        public CompProperties_PsydrainCoffin()
        {
            compClass = typeof(CompPsydrainCoffin);
        }
    }


    public class CompPsydrainCoffin: GoldenThroneAttachmentComp, ISuspendableThingHolder
    {
        public CompProperties_PsydrainCoffin Props => (CompProperties_PsydrainCoffin)props;
     
        //Coffin updates every 5000 ticks
        //A day is 60,000 ticks
        //this is the number of updates to kill
        private float ShockSeverityPerCoffinTick => 1 / (Props.daysToKill * 12);
        
        public CompPsydrainCoffin()
        {
            InnerContainer = new ThingOwner<Thing>(this);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.IsHashIntervalTick(5000)) return;
            Progress1LongTick();
        }

        private void Progress1LongTick()
        {
            if (Occupant == null) return;
            //Give psydrain shock
            Hediff shock = Occupant.health.GetOrAddHediff(GWGT_DefsOf.GWGT_PsydrainShock);

            if (shock.Severity + ShockSeverityPerCoffinTick > 1)
            {
                EjectContents();
            }
            shock.Severity += ShockSeverityPerCoffinTick;
        }

        private CompPowerTrader PowerTrader => _compPowerTrader ??= _compPowerTrader = parent.GetComp<CompPowerTrader>();
        private CompPowerTrader _compPowerTrader;

        private bool PowerOn => PowerTrader.PowerOn;

        private ThingOwner InnerContainer;

        public bool IsContentsSuspended => true;
        private Pawn Occupant => InnerContainer.OfType<Pawn>().FirstOrDefault();

        public float PsyfocusBonus =>
            (Occupant?.GetStatValue(StatDefOf.PsychicSensitivity) ?? 0) * Props.basePsyfocusGain;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref InnerContainer, "innerContainer", this);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return InnerContainer;
        }


        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            EjectContents(previousMap);
            InnerContainer.ClearAndDestroyContents();
            base.PostDestroy(mode, previousMap);
        }

        protected virtual void EjectContents(Map destMap = null)
        {
            //OnEmpty();
            InnerContainer.TryDropAll(parent.InteractionCell, destMap ?? parent.Map, ThingPlaceMode.Near);
        }
        
        private int TicksUntilDeath
        {
            get
            {
                if (Occupant.health.hediffSet.TryGetHediff(GWGT_DefsOf.GWGT_PsydrainShock, out Hediff shock)) return (int)(60000 * Props.daysToKill * (1 - shock.Severity));
                return (int)Props.daysToKill * 60000;
            }
        }


        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //No more base stuff
            //stringBuilder.AppendLineIfNotEmpty().Append(base.CompInspectStringExtra()).AppendLineIfNotEmpty();
            if (!parent.Spawned || Occupant == null) return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
            stringBuilder.AppendLineIfNotEmpty().Append("Contains".Translate()).Append(": ")
                .Append(Occupant.NameShortColored.Resolve()).AppendLineIfNotEmpty().Append("GWGT.PawnLifespanRemaining".Translate(GenDate.ToStringTicksToPeriod(TicksUntilDeath))).AppendLineIfNotEmpty().Append("GWGT.PawnPsyfocusGain".Translate(PsyfocusBonus.ToStringWithSign("0%"), Occupant.Named("PAWN")));
            return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
        }
        
        //Possible future implementation
        public bool CanAccept(Pawn pawn) => true;

        public void TryAcceptPawn(Pawn pawn)
        {
            if (!CanAccept(pawn)) return;
            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }
            if (pawn.holdingOwner != null)
            {
                pawn.holdingOwner.TryTransferToContainer(pawn, InnerContainer);
            }
            else
            {
                InnerContainer.TryAdd(pawn);
            }
        }
        
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (parent.GetComp<CompGoldenThroneAttachment>().AttachedThrone == null)
            {
                yield return new FloatMenuOption("GWGT.NoThroneAttached".Translate(), null);
                yield break;
            }
            if (selPawn.IsQuestLodger())
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null);
                yield break;
            }
            if (!selPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield break;
            }
            if (!PowerOn)
            {
                yield return new FloatMenuOption("CannotUseNoPower".Translate(), null);
                yield break;
            }
            if (selPawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)
            {
                yield return new FloatMenuOption("GWGT.PsychicallyDeaf".Translate(), null);
                yield break;
            }
            if (Occupant != null)
            {
                yield return new FloatMenuOption("CannotUseReason".Translate("GWGT.CoffinOccupied".Translate()), null);
                yield break;
            }
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GWGT.EnterPsydrainCoffin".Translate(), delegate
            {
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(GWGT_DefsOf.GWGT_EnterPsydrainCoffin, parent), JobTag.Misc);
            }), selPawn, parent);
        }
        
        
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            /*if (InnerContainer.Any)
            {
                foreach (Thing item in InnerContainer)
                {
                    yield return Building.SelectContainedItemGizmo(parent, item);
                }
            }*/
            if (Occupant != null && Prefs.DevMode)
            { 
                
                yield return new Command_Action
                {
                    icon = BaseContent.BadTex,
                    action = delegate
                    {
                        EjectContents();
                    },
                    defaultLabel = "GWGT.EjectPawn".Translate(),
                    defaultDesc = "GWGT.EjectPawnDesc".Translate(Occupant.Named("PAWN"))
                };

                yield return new Command_Action
                {
                    icon = BaseContent.BadTex,
                    action = delegate
                    {
                        //Progress by 30 long ticks
                        for (int i = 0; i < 30; i++)
                        {
                            Progress1LongTick();
                        }
                    },
                    defaultLabel = "GWGT.ProgressDrain".Translate(),
                    defaultDesc = ""
                };
            }
        }
    }
}