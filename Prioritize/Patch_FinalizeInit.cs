﻿using Harmony;
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Prioritize
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public class Patch_FinalizeInit
    {
        public static void Prefix()
        {
            if (MainMod.save == null)
            {
                //Log.Error("FinalizeInit called but no mod save loaded,");
                return;
            }
            MainMod.save.ClearUnusedThingPriority();
        }
    }


    [HarmonyPatch(typeof(Thing), "Destroy")]
    public class Patch_ThingDestroy
    {
        public static void Prefix(Thing __instance)
        {
            if (MainMod.save == null) return;
            MainMod.DestroyedThingId.Add(__instance.thingIDNumber);
        }
    }

    [HarmonyPatch(typeof(MapDeiniter), "NotifyEverythingWhichUsesMapReference")]
    public class Patch_MapDeinitier
    {
        public static void Prefix(Map map)
        {
            if (MainMod.save == null) return;
            int mapindex = Find.Maps.IndexOf(map);
            MainMod.save.PriorityMapDataDict.Remove(mapindex);
            for (int i = mapindex; i < Find.Maps.Count; i++)
            {
                if (i == mapindex) continue;
                var mapdata = MainMod.save.PriorityMapDataDict[i];
                if (mapdata == null)
                {
                    Log.Error("Cannot get PriorityMapData while deiniting map.");
                    continue;
                }
                mapdata.mapId -= 1;
                MainMod.save.PriorityMapDataDict.Remove(i);
                MainMod.save.PriorityMapDataDict.Add(i - 1, mapdata);
            }
        }
    }
}
