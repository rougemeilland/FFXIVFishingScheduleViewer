﻿namespace FishingScheduler
{
    interface IFishingConditionElement
    {
        double DifficultyValue { get; }
        string Description { get; }
        EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange);
    }
}