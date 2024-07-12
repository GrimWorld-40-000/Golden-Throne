﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GoldenThrone.Attachments
{
	public class JobDriver_EnterPsydrainCoffin : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, ignoreOtherReservations: errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => !job.targetA.Thing.TryGetComp<CompPsydrainCoffin>().CanAccept(GetActor()));
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return PrepareToEnterToil(TargetIndex.A);
			Toil enter = new Toil();
			enter.initAction = delegate
			{
				Pawn actor = enter.actor;
				actor.CurJob.targetA.Thing.TryGetComp<CompPsydrainCoffin>()?.TryAcceptPawn(actor);
			};
			enter.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return enter;
		}

		public static Toil PrepareToEnterToil(TargetIndex podIndex)
		{
			Toil toil = Toils_General.Wait(70);
			toil.FailOnCannotTouch(podIndex, PathEndMode.InteractionCell);
			toil.WithProgressBarToilDelay(podIndex);
			return toil;
		}
	}


}