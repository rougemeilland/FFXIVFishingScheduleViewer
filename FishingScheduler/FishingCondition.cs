using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class FishingCondition
    {
        private ITimeFishingConditionElement _timeCondition;
        private IWeatherFishingConditionElement _weatherCondition;

        public FishingCondition(FishingSpot fishingSpot)
            : this(fishingSpot, new AlwaysTimeCondition(), new AnyWeatherCondition())
        {
        }

        public FishingCondition(FishingSpot fishingSpot, WeatherType weather)
            : this(fishingSpot, new AlwaysTimeCondition(), new SimpleWeatherCondition(fishingSpot.Area, weather))
        {
        }

        public FishingCondition(FishingSpot fishingSpot, WeatherType weatherBefore, WeatherType weatherAfter)
            : this(fishingSpot, new AlwaysTimeCondition(), new ChangedWeatherCondition(fishingSpot.Area, weatherBefore, weatherAfter))
        {
        }

        public FishingCondition(FishingSpot fishingSpot, int hourOfStart, int hourOfEnd)
            : this(fishingSpot, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new AnyWeatherCondition())
        {
        }

        public FishingCondition(FishingSpot fishingSpot, int hourOfStart, int hourOfEnd, WeatherType weathers)
            : this(fishingSpot, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new SimpleWeatherCondition(fishingSpot.Area, weathers))
        {
        }

        public FishingCondition(FishingSpot fishingSpot, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter)
            : this(fishingSpot, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new ChangedWeatherCondition(fishingSpot.Area, weatherBefore, weatherAfter))
        {
        }

        private FishingCondition(FishingSpot fishingSpot, ITimeFishingConditionElement timeCondition, IWeatherFishingConditionElement weatherCondition)
        {
            DifficultyValue = timeCondition.DifficultyValue * weatherCondition.DifficultyValue;
            FishingSpot = fishingSpot;
            _timeCondition = timeCondition;
            _weatherCondition = weatherCondition;
            ConditionElements = new[]
            {
                (IFishingConditionElement)_timeCondition,
                (IFishingConditionElement)_weatherCondition,
            }
            .ToArray();
        }

        public FishingSpot FishingSpot { get; }
        public IEnumerable<IFishingConditionElement> ConditionElements { get; }
        public double DifficultyValue { get; }

        public EorzeaDateTimeHourRegions GetFishingChance(EorzeaDateTimeHourRegions wholeRange)
        {
            var region1 = _timeCondition.FindRegions(wholeRange);
            var region2 = _weatherCondition.FindRegions(wholeRange);
            var region = region1.Intersect(region2);
            return region;
        }
    }
}
