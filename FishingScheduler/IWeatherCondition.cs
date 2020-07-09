namespace FishingScheduler
{
    interface IWeatherCondition
    {
        double DifficultyValue { get; }
        string Description { get; }
        EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange);
    }
}
