﻿namespace FishingScheduler
{
    class AlwaysTimeCondition
        : ITimeFishingConditionElement
    {
        public double DifficultyValue => 1.0;

        public string Description => null;

        public EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange)
        {
            return wholeRange;
        }
    }
}
