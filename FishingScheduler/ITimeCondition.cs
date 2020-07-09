namespace FishingScheduler
{
    interface ITimeCondition
    {
        double DifficultyValue { get; }
        string Description { get; }
        EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange);
    }
}
