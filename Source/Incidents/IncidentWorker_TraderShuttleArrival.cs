﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;


namespace Spaceports.Incidents
{
	public class IncidentWorker_TraderShuttleArrival : IncidentWorker_TraderCaravanArrival
	{
		protected override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Trader;

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryResolveParms(parms))
			{
				return false;
			}
			if (parms.faction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			List<Pawn> pawns = SpawnPawns(parms);
			if (pawns.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].needs != null && pawns[i].needs.food != null)
				{
					pawns[i].needs.food.CurLevel = pawns[i].needs.food.MaxLevel;
				}
			}
			TraderKindDef traderKind = null;
			for (int j = 0; j < pawns.Count; j++)
			{
				Pawn pawn = pawns[j];
				if (pawn.TraderKind != null)
				{
					traderKind = pawn.TraderKind;
					break;
				}
			}
			SendLetter(parms, pawns, traderKind);
			RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0].Position, pawns[0].MapHeld, pawns[0], out var result, delegate (IntVec3 c)
			{
				for (int k = 0; k < pawns.Count; k++)
				{
					if (!pawns[k].CanReach(c, PathEndMode.OnCell, Danger.Deadly))
					{
						return false;
					}
				}
				return true;
			});
			TransportShip shuttle = Utils.GenerateInboundShuttle(pawns, parms);
			LordJobs.LordJob_ShuttleTradeWithColony lordJob = new LordJobs.LordJob_ShuttleTradeWithColony(parms.faction, result, shuttle.shipThing);
			LordMaker.MakeNewLord(parms.faction, lordJob, map, pawns);
			return true;
		}

		protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
		{
			TaggedString letterLabel = "LetterLabelTraderCaravanArrival".Translate(parms.faction.Name, traderKind.label).CapitalizeFirst();
			TaggedString letterText = "LetterTraderCaravanArrival".Translate(parms.faction.NameColored, traderKind.label).CapitalizeFirst();
			letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
		}

	}
}
