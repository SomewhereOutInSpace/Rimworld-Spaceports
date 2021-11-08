﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Spaceports.Buildings
{
    class Building_ShuttleSpot : Building
    {
        private int AccessState = 0;
        private Utils.DrawOver AllAllowed;
        private Utils.DrawOver NoneAllowed;
        private Utils.DrawOver VisitorsAllowed;
        private Utils.DrawOver TradersAllowed;

        public override void PostMake()
        {
            AllAllowed = new Utils.DrawOver(SpaceportsFrames.ChillSpot_All, 30, this, 1f, 1f);
            NoneAllowed = new Utils.DrawOver(SpaceportsFrames.ChillSpot_None, 30, this, 1f, 1f);
            VisitorsAllowed = new Utils.DrawOver(SpaceportsFrames.ChillSpot_Visitors, 30, this, 1f, 1f);
            TradersAllowed = new Utils.DrawOver(SpaceportsFrames.ChillSpot_Traders, 30, this, 1f, 1f);
            base.PostMake();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AccessState, "accessState", 0);
        }

        public override void Draw()
        {
            base.Draw();
            if (AccessState == -1) 
            {
                NoneAllowed.FrameStep();
            }
            if (AccessState == 0) 
            {
                AllAllowed.FrameStep();
            }
            if (AccessState == 1)
            {
                VisitorsAllowed.FrameStep();
            }
            if (AccessState == 2) 
            {
                TradersAllowed.FrameStep();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {

                defaultLabel = "AccessControlButton".Translate(),
                defaultDesc = "AccessControlDesc".Translate(),
                icon = getAccessIcon(),
                order = -100,
                action = delegate ()
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (Utils.AccessControlState state in SpaceportsMisc.AccessStates)
                    {
                        string label = state.GetLabel();
                        FloatMenuOption option = new FloatMenuOption(label, delegate ()
                        {
                            SetAccessState(state.getValue());
                        });
                        options.Add(option);
                    }

                    if (options.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            };

        }

        private void SetAccessState(int val)
        {
            AccessState = val;
        }

        public bool CheckAccessGranted(int val)
        {
            if (AccessState == -1)
            {
                return false;
            }
            if (AccessState == 0 || val == 0)
            {
                return true;
            }
            else return AccessState == val;
        }

        private Texture2D getAccessIcon()
        {
            if (AccessState == -1)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpot_none", true);
            }
            else if (AccessState == 0)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpot_all", true);
            }
            else if (AccessState == 1)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpot_visitors", true);
            }
            else if (AccessState == 2)
            {
                return ContentFinder<Texture2D>.Get("Buildings/SpaceportChillSpot/ChillSpot_traders", true);
            }
            return null;
        }
    }
}