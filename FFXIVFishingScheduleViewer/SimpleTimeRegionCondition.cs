using System;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    class SimpleTimeRegionCondition
        : ITimeFishingConditionElement
    {
        private int _hourOfStart;
        private int _hours;

        public SimpleTimeRegionCondition(int hourOfStart, int hourOfEnd)
        {
            if (hourOfStart < 0 || hourOfStart >= 24)
                throw new ArgumentException();
            if (hourOfEnd < 0 || hourOfEnd >= 24)
                throw new ArgumentException();
            _hourOfStart = hourOfStart;
            _hours = hourOfEnd - hourOfStart;
            if (_hours < 0)
                _hours += 24;
            DifficultyValue = 24.0 / _hours;
        }

        public double DifficultyValue { get; }

        public string Description =>
            string.Format(
                "{0} {1:D02}:00 - {2:D02}:59",
                Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ET.Short")],
                _hourOfStart,
                (_hourOfStart + _hours - 1) % 24);

        public EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange)
        {
            var timeOfStart = EorzeaDateTime.FromEpochHours((wholeRange.Begin.EpochDays - 1) * 24 + _hourOfStart);
            var timeOfEnd = wholeRange.End;
            var interval = EorzeaTimeSpan.FromHours(24);
            var span = EorzeaTimeSpan.FromHours(_hours);
            var regions = new List<EorzeaDateTimeRegion>();
            for (var time = timeOfStart; time < timeOfEnd; time += interval)
                regions.Add(new EorzeaDateTimeRegion(time, span));
            return new EorzeaDateTimeHourRegions(regions).Intersect(wholeRange);
        }
    }
}
