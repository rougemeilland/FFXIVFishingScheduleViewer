using System;
using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class SimpleWeatherCondition
        : IWeatherFishingConditionElement
    {
        private Area _area;
        private WeatherType _weather;

        public SimpleWeatherCondition(Area area, WeatherType weather)
        {
            _area = area;
            _weather = weather;
            DifficultyValue = 100.0 / _area.GetWeatherPercentage(weather);

            // 与えられた条件の天候がそのエリアで発生しうるかどうか検証する
            if (Enum.GetValues(typeof(WeatherType))
                .Cast<WeatherType>()
                .Where(w => (w & _weather) != WeatherType.None)
                .Any(w => !area.ContainsWeather(w)))
                throw new Exception();
        }

        public double DifficultyValue { get; }

        public string Description => string.Format("{0}", _weather.GetText());

        public EorzeaDateTimeHourRegions FindRegions(EorzeaDateTimeHourRegions wholeRange)
        {
            var countOfStart = wholeRange.Begin.EpochHours / 8;
            var countOfEnd = (wholeRange.End.EpochHours + 7) / 8;
            var interval = EorzeaTimeSpan.FromHours(8);
            var regions = new List<EorzeaDateTimeRegion>();
            for (var count = countOfStart - 1; count < countOfEnd; ++count)
            {
                var currentWeather = _area.ForecastWeather(EorzeaDateTime.FromEpochHours(count * 8));
                if ((currentWeather & _weather) != WeatherType.None)
                    regions.Add(new EorzeaDateTimeRegion(EorzeaDateTime.FromEpochHours(count * 8), interval));
            }
            return new EorzeaDateTimeHourRegions(regions).Intersect(wholeRange);
        }
    }
}
