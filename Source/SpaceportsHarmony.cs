﻿using HarmonyLib;
using RimWorld;
using Spaceports.LordToils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceports
{
    [StaticConstructorOnStartup]
    public static class SpaceportsHarmony
    {

        static SpaceportsHarmony()
        {
            Log.Message("[Spaceports] Okay, showtime!");
            Harmony har = new Harmony("Spaceports_Base");
            har.PatchAll(Assembly.GetExecutingAssembly());
            TryModPatches();
        }

        public static void TryModPatches()
        {

            if (Verse.ModLister.HasActiveModWithName("Hospitality")) //conditional patch to Hospitality
            {
                Harmony harmony = new Harmony("Spaceports_Plus_Hospitality");
                Log.Message("[Spaceports] Hospitality FOUND, attempting to patch...");
                var mOriginal = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:CreateLord");
                var mPostfix = typeof(SpaceportsHarmony).GetMethod("CreateLordPostfix");

                if (mOriginal != null)
                {
                    var patch = new HarmonyMethod(mPostfix);
                    Log.Message("[Spaceports] Attempting to postfix Hospitality.IncidentWorker_VisitorGroup.CreateLord...");
                    harmony.Patch(mOriginal, postfix: patch);
                }

                if (Verse.ModLister.HasActiveModWithName("Save Our Ship 2")) //Conditional integration patches with SOS2
                {
                    Log.Message("[Spaceports] Hospitality and SOS2 FOUND, attempting integration patch...");
                    var mOriginalA = AccessTools.Method("Hospitality.ItemUtility:PocketHeadgear");
                    var mPrefixA = typeof(SpaceportsHarmony).GetMethod("PocketHeadgearPrefix");
                    var mOriginalB = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:FactionCanBeGroupSource");
                    var mPostfixB = typeof(SpaceportsHarmony).GetMethod("FactionCanBeGroupSourcePostfix");
                    var mOriginalC = AccessTools.Method("Hospitality.IncidentWorker_VisitorGroup:CheckCanCome");
                    var mPostfixC = typeof(SpaceportsHarmony).GetMethod("CheckCanComePostfix");

                    if (mOriginalA != null)
                    {
                        var patch = new HarmonyMethod(mPrefixA);
                        Log.Message("[Spaceports] Attempting to prefix Hospitality.ItemUtility.PocketHeadgear...");
                        harmony.Patch(mOriginalA, prefix: patch);
                    }
                    if (mOriginalB != null)
                    {
                        var patch = new HarmonyMethod(mPostfixB);
                        Log.Message("[Spaceports] Attempting to postfix Hospitality.IncidentWorker_VisitorGroup.FactionCanBeGroupSource...");
                        harmony.Patch(mOriginalB, postfix: patch);
                    }
                    if (mOriginalC != null)
                    {
                        var patch = new HarmonyMethod(mPostfixC);
                        Log.Message("[Spaceports] Attempting to postfix Hospitality.IncidentWorker_VisitorGroup.CheckCanCome...");
                        harmony.Patch(mOriginalC, postfix: patch);
                    }
                }

            }
            else
            {
                Log.Message("[Spaceports] Hospitality not found, patches bypassed.");
            }

            //conditional patch to Trader Ships/Themis Traders (they use the same assembly under the hood LMFAOOO)
            if (Verse.ModLister.HasActiveModWithName("Trader ships") || Verse.ModLister.HasActiveModWithName("Rim-Effect: Themis Traders")) 
            {
                Harmony harmony = new Harmony("Spaceports_Plus_TraderShips");
                Log.Message("[Spaceports] Trader Ships/Themis Traders FOUND, attempting to patch...");
                var mOriginal = AccessTools.Method("TraderShips.IncidentWorkerTraderShip:FindCloseLandingSpot");
                var mPostfix = typeof(SpaceportsHarmony).GetMethod("FindCloseLandingSpotPostfix");

                if (mOriginal != null)
                {
                    var patch = new HarmonyMethod(mPostfix);
                    Log.Message("[Spaceports] Attempting to postfix TraderShips.IncidentWorkerTraderShip.FindCloseLandingSpot...");
                    harmony.Patch(mOriginal, postfix: patch);
                }
            }
            else
            {
                Log.Message("[Spaceports] Trader Ships not found, patches bypassed.");
            }
        }

        [HarmonyPatch(typeof(DropCellFinder), "GetBestShuttleLandingSpot", new Type[] { typeof(Map), typeof(Faction) })] //Royalty shuttle patch
        public static class Harmony_DropCellFinder_GetBestShuttleLandingSpot
        {
            static void Postfix(Map map, Faction factionForFindingSpot, ref IntVec3 __result)
            {
                if (!Utils.CheckIfClearForLanding(map, 0))
                {
                    return;
                }
                else
                {
                    __result = Utils.FindValidSpaceportPad(map, factionForFindingSpot, 0);
                }
                return;
            }
        }

        [HarmonyPostfix] //Hospitality patch
        public static void CreateLordPostfix(Faction faction, List<Pawn> pawns, Map map)
        {
            //Conditional check
            //IF rand.chance of configured chance checks out (or if this is a space map)
            //AND Hospitality integration is enabled
            //AND we are clear for landing
            //AND the faction is not neolithic
            //AND Kessler Syndrome is not in effect
            if ((Rand.Chance(LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().hospitalityChance) || Utils.IsMapInSpace(map)) && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().hospitalityEnabled && Utils.CheckIfClearForLanding(map, 3) && faction.def.techLevel != TechLevel.Neolithic && !map.gameConditionManager.ConditionIsActive(SpaceportsDefOf.Spaceports_KesslerSyndrome))
            {
                if (pawns != null)
                {
                    IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 3); //Find valid landing pad or touchdown spot
                    TransportShip shuttle = Utils.GenerateInboundShuttle(pawns, pad, map); //Initialize shuttle

                    StateGraph graphExit = new LordJobs.LordJob_DepartSpaceport(shuttle.shipThing).CreateGraph(); //Intialize patched subgraph

                    Lord lord = pawns[0].GetLord(); //Get Lord
                    List<LordToil> lordToils = lord.Graph.lordToils.ToList(); //Get and copy Lord lordToils to list

                    LordToil_TryShuttleWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TryShuttleWoundedGuest(shuttle.shipThing, LocomotionUrgency.Sprint, canDig: false, interruptCurrentJob: true); //Initialize patched guest rescue
                    lordToil_TakeWoundedGuest.lord = lord;
                    lordToils.Add(lordToil_TakeWoundedGuest);

                    LordToil patchedToilExit = lord.Graph.AttachSubgraph(graphExit).StartingToil; //Attach patched subgraph
                    LordToil patchedToilExit2 = graphExit.lordToils[1]; //Get second toil from subgraph
                    patchedToilExit.lord = lord; //Ensure lord is assigned correctly - stops "curLordToil lord is null (forgot to add toil to graph?)" bullshit
                    patchedToilExit2.lord = lord; //ditto
                    lordToils.Add(patchedToilExit); //Attach first patched toil
                    lordToils.Add(patchedToilExit2); //Attach second patched toil
                    lord.Graph.lordToils = lordToils; //Ensure lord graph is updated (might be redundant)


                    List<Transition> transitions = lord.Graph.transitions.ToList(); //Get and copy transitions to list
                    foreach (Transition transition in transitions)
                    {
                        foreach (Trigger trigger in transition.triggers) //foreach trigger in each transition
                        {
                            //determine which transition we're dealing with by referencing its triggers
                            //might replace with string matching since this is the equivalent of fucking triangulation
                            if (trigger is Trigger_PawnExperiencingDangerousTemperatures || trigger is Trigger_BecamePlayerEnemy || (transition.preActions.Any(x => x is TransitionAction_Message) && !(trigger is Trigger_WoundedGuestPresent)))
                            {
                                transition.target = patchedToilExit;
                            }
                            //Edge case to patch wounded guest trigger
                            else if (trigger is Trigger_WoundedGuestPresent)
                            {
                                transition.target = lordToil_TakeWoundedGuest;
                            }
                            //Remove all occurences of a custom Hospitality preaction that throws NRE (I think because it assumes it is interacting w/ a LordToil_Travel but gets a LordToil_Wait)
                            foreach (TransitionAction preAction in transition.preActions.ToList())
                            {
                                if (preAction.ToString().Equals("Hospitality.TransitionAction_EnsureHaveNearbyExitDestination"))
                                {
                                    transition.preActions.Remove(preAction);
                                }
                            }
                        }
                    }
                    lord.Graph.transitions = transitions; //Ensure lord graph is updated (might be redundant)
                }

                return;
            }

            else //Patched behavior not called
            {
                return;
            }

        }

        [HarmonyPrefix] //Hospitality/SOS2 bridge patch, stops pawns from removing their EVA helmets
        public static bool PocketHeadgearPrefix(Pawn pawn)
        {
            foreach(Apparel ap in pawn.apparel.WornApparel)
            {
                if(ap.def.defName == "Apparel_SpaceSuitHelmet")
                {
                    return false; //keep that EVA helmet on you fucking idiot
                }
            }
            return true;
        }

        [HarmonyPostfix] //Hospitality/SOS2 bridge patch, disqualifies neolithic factions from visiting space maps
        public static void FactionCanBeGroupSourcePostfix(Faction f, Map map, ref bool __result)
        {
            if(f.def.techLevel == TechLevel.Neolithic && Utils.IsMapInSpace(map))
            {
                __result = false;
            }
        }

        [HarmonyPostfix] //Hospitality/SOS2 bridge patch, stops temp concerns when trying to visit a space map
        public static void CheckCanComePostfix(Map map, Faction faction, ref TaggedString reasons, ref bool __result)
        {
            if (__result || reasons == null)
            {
                return;
            }

            String TranslatedTemp = "- " + "Temperature".Translate();
            if (reasons.ToString().Contains(TranslatedTemp))
            {
                TaggedString newstr = reasons.ToString().Replace(TranslatedTemp, "");
                reasons = newstr;
            }

            if (!reasons.ToString().Contains("-"))
            {
                __result = true;
            }
        }

        [HarmonyPostfix] //Trader Ships/Themis Traders patch
        public static void FindCloseLandingSpotPostfix(Map map, Faction faction, ref IntVec3 spot)
        {
            if(!Utils.CheckIfClearForLanding(map, 2))
            {
                return;
            }
            else
            {
                spot = Utils.FindValidSpaceportPad(map, faction, 2);
            }
            return;
        }

    }

}
