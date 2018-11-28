﻿using System;
using System.Reflection;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Prioritize
{
    #region UnsafePatches

    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Global")]
    public class Patch_GenClosest1
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {
            if (!MainMod.UseUnsafePatches) return;
            var p = priorityGetter;
            if (p == null) p = delegate (Thing t)
            {
                return 0f;
            };
            priorityGetter = delegate (Thing t)
            {
                float pr = p(t) + (MainMod.save.TryGetThingPriority(t, out int pri) ? pri : 0);
                if (t.Map != null && t.Position.InBounds(t.Map))
                {
                    pr += MainMod.save.GetOrCreatePriorityMapData(t.Map).GetPriorityAt(t.Position);
                }
                return pr;
            };
        }
    }

    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Global_Reachable")]
    public class Patch_GenClosest2
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {
            if (!MainMod.UseUnsafePatches) return;
            var p = priorityGetter;
            if (p == null) p = delegate (Thing t)
            {
                return 0f;
            };
            priorityGetter = delegate (Thing t)
            {
                float pr = p(t) + (MainMod.save.TryGetThingPriority(t, out int pri) ? pri : 0);
                if (t.Map != null && t.Position.InBounds(t.Map))
                {
                    pr += MainMod.save.GetOrCreatePriorityMapData(t.Map).GetPriorityAt(t.Position);
                }
                return pr;
            };
        }
    }

    [HarmonyPatch(typeof(GenClosest), "RegionwiseBFSWorker")]
    public class Patch_GenClosest3
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {
            if (!MainMod.UseUnsafePatches) return;
            var p = priorityGetter;
            if (p == null) p = delegate (Thing t)
            {
                return 0f;
            };
            priorityGetter = delegate (Thing t)
            {
                float pr = p(t) + (MainMod.save.TryGetThingPriority(t, out int pri) ? pri : 0);
                if (t.Map != null && t.Position.InBounds(t.Map))
                {
                    pr += MainMod.save.GetOrCreatePriorityMapData(t.Map).GetPriorityAt(t.Position);
                }
                return pr;
            };
        }
    }
    #endregion

    #region SafePatches

    [HarmonyPatch(typeof(WorkGiver_Scanner))]
    [HarmonyPatch("Prioritized", MethodType.Getter)]
    public class Patch_Prioritized
    {
        public static bool Prefix(WorkGiver_Scanner __instance, ref bool __result)
        {
            __result = true;
            return false;
        }
    }


    [HarmonyPatch(typeof(WorkGiver_Scanner), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) })]
    public class Patch_GetPriority
    {
        public static void Postfix(Pawn pawn, TargetInfo t, ref float __result)
        {
            if (__result < 0) return;
            if (pawn.Faction != null && !pawn.Faction.IsPlayer) return;

            Map m = pawn.Map; if (m == null) m = t.Map;
            if (t.HasThing && !MainMod.UseUnsafePatches)
            {
                __result += MainMod.save.TryGetThingPriority(t.Thing, out int pri) ? pri + 0.1f : 0;
            }
            __result += MainMod.save.GetOrCreatePriorityMapData(m).GetPriorityAt(t.Cell);

        }
    }

    #endregion

    #region UtilityPatches

    [HarmonyPatch(typeof(Blueprint), "TryReplaceWithSolidThing")]
    public class Patch_Blueprint
    {
        public static void Postfix(Blueprint __instance, Thing createdThing)
        {
            if (MainMod.save.TryGetThingPriority(__instance, out int pri))
                MainMod.save.SetThingPriority(createdThing, pri);
        }
    }

    [HarmonyPatch(typeof(Frame), "FailConstruction")]
    public class Patch_Frame
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int patchphase = 0;
            foreach (var inst in instructions)
            {
                yield return inst;
                if (patchphase == 1 && inst.opcode == OpCodes.Pop)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, typeof(Patch_Frame).GetMethod("FixPriority"));
                    patchphase = 2;
                }
                if (inst.operand == typeof(GenSpawn).GetMethod("Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) }))
                {
                    patchphase = 1;
                }
                
            }
        }

        public static void FixPriority(Thing fromFrame, Thing toBlueprint)
        {
            if (MainMod.save.TryGetThingPriority(fromFrame, out int pri))
                MainMod.save.SetThingPriority(toBlueprint, pri);

        }
    }

    #endregion
}