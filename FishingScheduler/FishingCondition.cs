using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingScheduler
{
    class FishingCondition
    {
        private ITimeCondition _timeCondition;
        private IWeatherCondition _weatherCondition;

        public FishingCondition(FishingGround fishingGround)
            : this(fishingGround, new AlwaysTimeCondition(), new AnyWeatherCondition())
        {
        }

        public FishingCondition(FishingGround fishingGround, WeatherType weather)
            : this(fishingGround, new AlwaysTimeCondition(), new SimpleWeatherCondition(fishingGround.Area, weather))
        {
        }

        public FishingCondition(FishingGround fishingGround, WeatherType weatherBefore, WeatherType weatherAfter)
            : this(fishingGround, new AlwaysTimeCondition(), new ChangedWeatherCondition(fishingGround.Area, weatherBefore, weatherAfter))
        {
        }

        public FishingCondition(FishingGround fishingGround, int hourOfStart, int hourOfEnd)
            : this(fishingGround, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new AnyWeatherCondition())
        {
        }

        public FishingCondition(FishingGround fishingGround, int hourOfStart, int hourOfEnd, WeatherType weathers)
            : this(fishingGround, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new SimpleWeatherCondition(fishingGround.Area, weathers))
        {
        }

        public FishingCondition(FishingGround fishingGround, int hourOfStart, int hourOfEnd, WeatherType weatherBefore, WeatherType weatherAfter)
            : this(fishingGround, new SimpleTimeRegionCondition(hourOfStart, hourOfEnd), new ChangedWeatherCondition(fishingGround.Area, weatherBefore, weatherAfter))
        {
        }

        private FishingCondition(FishingGround fishingGround, ITimeCondition timeCondition, IWeatherCondition weatherCondition)
        {
            DifficultyValue = timeCondition.DifficultyValue * weatherCondition.DifficultyValue;
            FishingGround = fishingGround;
            _timeCondition = timeCondition;
            _weatherCondition = weatherCondition;
            Desctriptions = new[] { _timeCondition.Description, _weatherCondition.Description }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        public FishingGround FishingGround { get; }
        public IEnumerable<string> Desctriptions { get; }
        public double DifficultyValue { get; }

        public EorzeaDateTimeHourRegions GetFishingChance(IKeyValueCollection<string, AreaGroup> areaGroups, EorzeaDateTimeHourRegions wholeRange)
        {
            var region1 = _timeCondition.FindRegions(wholeRange);
            var region2 = _weatherCondition.FindRegions(wholeRange);
            var region = region1.Intersect(region2);
            return region;
        }
    }
}
