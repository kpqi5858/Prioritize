using HugsLib.Utils;
using Verse;
using System.Collections.Generic;

namespace Prioritize
{

    public class PSaveData : UtilityWorldObject
    {
        public Dictionary<int, PriorityMapData> PriorityMapDataDict = new Dictionary<int, PriorityMapData>();
        public Dictionary<int, int> ThingPriority = new Dictionary<int, int>();

        public PSaveData()
        {
            
        }

        public int GetOrCreateThingPriority(Thing t)
        {
            if (ThingPriority.TryGetValue(t.thingIDNumber, out int val)) return val;
            ThingPriority.Add(t.thingIDNumber, 0); return 0;
        }

        public bool TryGetThingPriority(Thing t, out int pri)
        {
            return ThingPriority.TryGetValue(t.thingIDNumber, out pri);
        }
        public void SetThingPriority(Thing t, int p)
        {
            if (ThingPriority.ContainsKey(t.thingIDNumber))
            {
                if (p == 0)
                {
                    ThingPriority.Remove(t.thingIDNumber);
                    return;
                }
                ThingPriority[t.thingIDNumber] = p;
            }
            else if (p == 0) return;
            else ThingPriority.Add(t.thingIDNumber, p);
        }
        public PriorityMapData GetOrCreatePriorityMapData(Map m)
        {
            if (PriorityMapDataDict.TryGetValue(Find.Maps.IndexOf(m), out PriorityMapData grid)) return grid;
            PriorityMapData pg = new PriorityMapData(m);
            PriorityMapDataDict.Add(Find.Maps.IndexOf(m), pg); return pg;
        }

        public void ClearUnusedThingPriority()
        {
            var newThingPri = new Dictionary<int, int>();
            foreach(Map map in Find.Maps)
            {
                var things = map.spawnedThings;
                for (int i = 0; i < things.Count; i++)
                {
                    var t = things[i];
                    if (ThingPriority.TryGetValue(t.thingIDNumber, out int v)) newThingPri.Add(t.thingIDNumber, v);
                }
            }
            ThingPriority = newThingPri;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<int, PriorityMapData>(ref PriorityMapDataDict, "prioritymapdata", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<int, int>(ref ThingPriority, "thingPriority", LookMode.Value, LookMode.Value);
        }

        public void ResolvePriorityGridMaps(int mapid)
        {
            var grid = GetOrCreatePriorityMapData(Find.Maps[mapid]);
            //if (grid == null) throw new Exception("wtf");
            grid.map = Find.Maps[grid.mapId];
        }
    }
}
