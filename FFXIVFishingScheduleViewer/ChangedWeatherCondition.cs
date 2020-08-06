using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    class ChangedWeatherCondition
        : IWeatherFishingConditionElement
    {
        private Area _area;
        private WeatherType _before;
        private WeatherType _after;

        public ChangedWeatherCondition(Area area, WeatherType before, WeatherType after)
        {
            _area = area;
            _before = before;
            _after = after;
            DifficultyValue = 100.0 / _area.GetWeatherPercentage(_before) * 100.0 / _area.GetWeatherPercentage(_after);
#if DEBUG
            // 与えられた条件の天候がそのエリアで発生しうるかどうか検証する
            if (Enum.GetValues(typeof(WeatherType))
                .Cast<WeatherType>()
                .Where(w => (w & (_before | after)) != WeatherType.None)
                .Any(w => !area.ContainsWeather(w)))
            {
                throw new Exception();
            }
#endif
        }

        public double DifficultyValue { get; }

        public string Description => string.Format("{0}⇒{1}", _before.GetText(), _after.GetText());

        public EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange)
        {
            var countOfStart = wholeRange.Begin.EpochHours / 8;
            var countOfEnd = (wholeRange.End.EpochHours + 7) / 8;
            var interval = EorzeaTimeSpan.FromHours(8);
            var weathers = new Dictionary<long, WeatherType>();
            var regions = new List<EorzeaDateTimeRegion>();
            for (var count = countOfStart - 1; count < countOfEnd; ++count)
                weathers.Add(count, _area.ForecastWeather(EorzeaDateTime.FromEpochHours(count * 8)));
            for (var count = countOfStart; count < countOfEnd; ++count)
            {
                if ((weathers[count - 1] & _before) != WeatherType.None && (weathers[count] & _after) != WeatherType.None)
                    regions.Add(new EorzeaDateTimeRegion(EorzeaDateTime.FromEpochHours(count * 8), interval));
            }
            return new EorzeaDateTimeHourRegions(regions).Intersect(wholeRange);
        }
    }
}
