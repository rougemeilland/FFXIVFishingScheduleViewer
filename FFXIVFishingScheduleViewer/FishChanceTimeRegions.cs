namespace FFXIVFishingScheduleViewer
{
    class FishChanceTimeRegions
    {
        public FishChanceTimeRegions(FishingCondition condition, EorzeaDateTimeHourRegions regions)
        {
            FishingCondition = condition;
            Regions = regions;
        }

        public FishingCondition FishingCondition { get; }
        public EorzeaDateTimeHourRegions Regions { get; }
    }
}
