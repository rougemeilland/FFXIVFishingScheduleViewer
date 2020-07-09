namespace FishingScheduler
{
    class AnyWeatherCondition
        : IWeatherCondition
    {
        public double DifficultyValue => 1.0;

        public string Description => null;

        public EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange)
        {
            return wholeRange;
        }
    }
}
