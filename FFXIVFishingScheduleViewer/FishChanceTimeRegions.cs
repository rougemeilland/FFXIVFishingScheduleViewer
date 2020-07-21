using System.Linq;

namespace FFXIVFishingScheduleViewer
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
    }
}
