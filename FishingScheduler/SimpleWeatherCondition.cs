using System.Collections.Generic;

namespace FishingScheduler
{
    class SimpleWeatherCondition
        : IWeatherCondition
    {
        private Area _area;
        private WeatherType _weather;

        public SimpleWeatherCondition(Area area, WeatherType weather)
        {
            _area = area;
            _weather = weather;
            DifficultyValue = 100.0 / _area.GetWeatherPercentage(weather);
            Description = string.Format("{0}", _weather.GetText());
        }

        public double DifficultyValue { get; }

        public string Description { get; }

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
