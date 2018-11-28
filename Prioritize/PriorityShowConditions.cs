using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Prioritize
{
    [StaticConstructorOnStartup]
    public static class PriorityShowConditions
    {

        private static List<PriorityShowCondition> Conditions = new List<PriorityShowCondition>();

        private static List<FloatMenuOption> CachedOptions = new List<FloatMenuOption>();

        public static PriorityShowCondition DefaultCondition;

        static PriorityShowConditions() 
        {
            var defcond = new PriorityShowCondition(delegate (Thing t)
            {
                Map map = t.Map;
                var res = t is Blueprint || t is Frame || map.designationManager.DesignationOn(t) != null || map.listerHaulables.ThingsPotentiallyNeedingHauling().Contains(t);
                if (t.Position.IsValid) res = res || map.designationManager.AllDesignationsAt(t.Position).Count() > 0;
                return res;
            }, "Auto");
            DefaultCondition = defcond;

            Conditions.Add(defcond);

            Conditions.Add(new PriorityShowCondition(delegate (Thing t) 
            {
                return t is Blueprint || t is Frame;
            }, "Blueprint"));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Blueprint || t is Frame || t.Map.designationManager.DesignationOn(t) != null;
            }, "Designations"));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Building || t is Hive;
            }, "Building"));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Pawn;
            }, "Pawn"));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t.def.EverHaulable;
            }, "Items"));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Plant;
            }, "Plant"));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is ThingWithComps;
            }, "All"));
            CacheMenuOptions();
        }

        private static void CacheMenuOptions()
        {
            foreach (var v in Conditions)
            {
                CachedOptions.Add(new FloatMenuOption(v.label, delegate ()
                {
                    MainMod.ThingShowCond = v.Cond;
                }));
            }
        }

        public static void ShowConditionsMenuBox()
        {
            Find.WindowStack.Add(new FloatMenu(CachedOptions));
        }
    }
}
