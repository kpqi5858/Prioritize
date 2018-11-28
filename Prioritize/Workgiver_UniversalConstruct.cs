using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Prioritize
{
    public class Workgiver_UniversalConstruct : WorkGiver_ConstructDeliverResources
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Construction);

        private static List<Func<Pawn, Thing, bool, Job>> CheckList = new List<Func<Pawn, Thing, bool, Job>>();

        public Workgiver_UniversalConstruct()
        {
            //ConstructFinishFrames
            CheckList.Add(delegate (Pawn pawn, Thing t, bool forced)
            {
                if (t.Faction != pawn.Faction)
                {
                    return null;
                }
                Frame frame = t as Frame;
                if (frame == null)
                {
                    return null;
                }
                if (frame.MaterialsNeeded().Count > 0)
                {
                    return null;
                }
                if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
                {
                    return GenConstruct.HandleBlockingThingJob(frame, pawn, forced);
                }
                Frame t2 = frame;
                if (!GenConstruct.CanConstruct(t2, pawn, true, forced))
                {
                    return null;
                }
                return new Job(JobDefOf.FinishFrame, frame);
            });

            //ConstructDeliverResourcesToFrames
            CheckList.Add(delegate (Pawn pawn, Thing t, bool forced)
            {
                if (t.Faction != pawn.Faction)
                {
                    return null;
                }
                Frame frame = t as Frame;
                if (frame == null)
                {
                    return null;
                }
                if (GenConstruct.FirstBlockingThing(frame, pawn) != null)
                {
                    return GenConstruct.HandleBlockingThingJob(frame, pawn, forced);
                }
                bool checkConstructionSkill = this.def.workType == WorkTypeDefOf.Construction;
                if (!GenConstruct.CanConstruct(frame, pawn, checkConstructionSkill, forced))
                {
                    return null;
                }
                return base.ResourceDeliverJobFor(pawn, frame, true);
            });

            //ConstructDeliverResourcesToBlueprints
            CheckList.Add(delegate (Pawn pawn, Thing t, bool forced)
            {
                if (t.Faction != pawn.Faction)
                {
                    return null;
                }
                Blueprint blueprint = t as Blueprint;
                if (blueprint == null)
                {
                    return null;
                }
                if (GenConstruct.FirstBlockingThing(blueprint, pawn) != null)
                {
                    return GenConstruct.HandleBlockingThingJob(blueprint, pawn, forced);
                }
                bool flag = this.def.workType == WorkTypeDefOf.Construction;
                if (!GenConstruct.CanConstruct(blueprint, pawn, flag, forced))
                {
                    return null;
                }
                if (!flag && WorkGiver_ConstructDeliverResources.ShouldRemoveExistingFloorFirst(pawn, blueprint))
                {
                    return null;
                }
                Job job = base.RemoveExistingFloorJob(pawn, blueprint);
                if (job != null)
                {
                    return job;
                }
                Job job2 = base.ResourceDeliverJobFor(pawn, blueprint, true);
                if (job2 != null)
                {
                    return job2;
                }
                if (this.def.workType != WorkTypeDefOf.Hauling)
                {
                    Job job3 = this.NoCostFrameMakeJobFor(pawn, blueprint);
                    if (job3 != null)
                    {
                        return job3;
                    }
                }
                return null;
            });
        }

        private Job NoCostFrameMakeJobFor(Pawn pawn, IConstructible c)
        {
            if (c is Blueprint_Install)
            {
                return null;
            }
            if (c is Blueprint && c.MaterialsNeeded().Count == 0)
            {
                return new Job(JobDefOf.PlaceNoCostFrame)
                {
                    targetA = (Thing)c
                };
            }
            return null;
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            foreach (var func in CheckList)
            {
                var res = func(pawn, t, forced);
                if (res != null) return res;
            }
            return null;
        }
    }
}
