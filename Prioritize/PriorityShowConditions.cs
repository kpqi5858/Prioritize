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
                if (t.Faction?.IsPlayer == false) return false;
                Map map = t.Map;
                var res = t is Blueprint || t is Frame || map.designationManager.DesignationOn(t) != null || map.listerHaulables.ThingsPotentiallyNeedingHauling().Contains(t);
                if (t.Position.IsValid) res = res || map.designationManager.AllDesignationsAt(t.Position).Count() > 0;

                Building bui = t as Building;
                if (bui != null && t.Position.IsValid)
                {
                    if (bui.def.building.repairable && t.def.useHitPoints && t.HitPoints != t.MaxHitPoints && t.Map?.areaManager.Home[t.Position] == true) res = true;
                    if (bui.def == ThingDefOf.DeepDrill) res = true;
                }
                
                return res;
            }, "P_Auto".Translate());
            DefaultCondition = defcond;

            Conditions.Add(defcond);

            Conditions.Add(new PriorityShowCondition(delegate (Thing t) 
            {
                return t is Blueprint || t is Frame;
            }, "BlueprintLabelExtra".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Blueprint || t is Frame || t.Map.designationManager.DesignationOn(t) != null;
            }, "P_Designations".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Building || t is Hive;
            }, "P_Building".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t is Pawn;
            }, "PawnsTabShort".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return t.def.EverHaulable;
            }, "P_Items".Translate()));

            Conditions.Add(new PriorityShowCondition(delegate (Thing t)
            {
                return true;
            }, "ShowAll".Translate()));
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
