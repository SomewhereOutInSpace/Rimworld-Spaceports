﻿using RimWorld;
using SharpUtils;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using static Spaceports.Utils;

namespace Spaceports
{
    [DefOf]
    public static class SpaceportsDefOf
    {
        public static ThingDef Synthread;
        public static ThingDef Spaceports_ShuttleLandingPad;
        public static ThingDef ShuttleA_Crashing;
        public static ThingDef Spaceports_RoyaltyShuttle;
        public static ThingDef Spaceports_Shrapnel;

        public static TransportShipDef Spaceports_RoyaltyShuttleTS;
        public static TransportShipDef Spaceports_ShuttleA;
        public static TransportShipDef Spaceports_ShuttleInert;
        public static TransportShipDef Spaceports_SurpriseShuttle;

        public static IncidentDef Spaceports_VisitorShuttleArrival;
        public static IncidentDef Spaceports_TraderShuttleArrival;
        public static GameConditionDef Spaceports_KesslerSyndrome;
        public static IncidentDef Spaceports_MedevacReward;

        public static DutyDef Spaceports_TryShuttleWoundedGuest;
        public static JobDef Spaceports_Kidnap;

        public static ThoughtDef Spaceports_PsychicCharge;
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsMisc //Misc complex constants 
    {
        public static List<AccessControlState> AccessStates = new List<AccessControlState>();
        static SpaceportsMisc()
        {
            AccessStates.Add(new AccessControlState("Spaceports_None", -1));
            AccessStates.Add(new AccessControlState("Spaceports_AllTypes", 0));
            AccessStates.Add(new AccessControlState("Spaceports_JustVisitors", 1));
            AccessStates.Add(new AccessControlState("Spaceports_JustTraders", 2));
            if (Verse.ModLister.HasActiveModWithName("Hospitality"))
            {
                AccessStates.Add(new AccessControlState("Spaceports_JustGuests", 3));
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsMats //Material constants used in animations/visual changes. Spun up at runtime.
    {
        public static readonly SharpAnim.FrameStack ChillSpot = SharpAnim.ConstructFrameStack("Buildings/SpaceportChillSpot/ChillSpotOverlay");

        public static readonly SharpAnim.Frame HoldingPatternGraphic = SharpAnim.ConstructFrame("Animations/HoldingPattern", ShaderDatabase.TransparentPostLight);
        public static readonly SharpAnim.Frame BlockedPatternGraphic = SharpAnim.ConstructFrame("Animations/BlockedPattern", ShaderDatabase.TransparentPostLight);
        public static readonly SharpAnim.FrameStack LandingPadRimLights = SharpAnim.ConstructFrameStack("Animations/LandingPad/RimLights", ShaderDatabase.TransparentPostLight);
        public static readonly SharpAnim.FrameStack LandingPadTouchdownLights = SharpAnim.ConstructFrameStack("Animations/LandingPad/TouchdownLights", ShaderDatabase.TransparentPostLight);

        public static readonly SharpAnim.Frame RadarDish = SharpAnim.ConstructFrame("Animations/Beacon/SpaceportBeaconDish");
        public static readonly SharpAnim.FrameStack BeaconLights = SharpAnim.ConstructFrameStack("Animations/Beacon/SpaceportBeaconLights", ShaderDatabase.TransparentPostLight);
    }

    [StaticConstructorOnStartup]
    public static class SpaceportsShuttleVariants //Compiles list of available shuttle variants at runtime.
    {
        public static List<TransportShipDef> AllShuttleVariants = new List<TransportShipDef>();
        static SpaceportsShuttleVariants()
        {
            if (ModsConfig.RoyaltyActive)
            {
                AllShuttleVariants.Add(SpaceportsDefOf.Spaceports_RoyaltyShuttleTS);
            }
            AllShuttleVariants.Add(SpaceportsDefOf.Spaceports_ShuttleA);
        }
    }
}