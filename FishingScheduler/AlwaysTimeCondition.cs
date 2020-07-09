namespace FishingScheduler
{
    class AlwaysTimeCondition
        : ITimeCondition
    {
        public double DifficultyValue => 1.0;

        public string Description => null;

        public EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange)
        {
            return wholeRange;
        }
    }
}
