using System.Linq;

namespace FishingScheduler
{
    class FishChanceTimeRegions
    {
        public FishChanceTimeRegions(Fish fish, FishingCondition condition, EorzeaDateTimeHourRegions regions, EorzeaDateTime now)
        {
            Fish = fish;
            FishingCondition = condition;
            Regions = regions;
        }

        public Fish Fish { get; }
        public FishingCondition FishingCondition { get; }
        public EorzeaDateTimeHourRegions Regions { get; }

        /*
        public FishChanceTimeRegions UnionRegion(EorzeaDateTimeHourRegions otherRegions)
        {
            return new FishChanceTimeRegions(Fish, FishingGround, Regions.Union(otherRegions));
        }

        public FishChanceTimeRegions IntersectRegion(EorzeaDateTimeHourRegions otherRegions)
        {
            return new FishChanceTimeRegions(Fish, FishingGround, Regions.Intersect(otherRegions));
        }

        public FishChanceTimeRegions ExceptRegion(EorzeaDateTimeHourRegions otherRegions)
        {
            return new FishChanceTimeRegions(Fish, FishingGround, Regions.Except(otherRegions));
        }
        */
    }
}
