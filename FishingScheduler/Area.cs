using System;
using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class Area
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private TranslationTextId _nameId;
        private WeatherPercentageOfArea[] _percentageOfWeather;

        public Area(AreaGroup areaGroup, string areaId, WeatherPercentageOfArea[] percentageOfWeather)
        {
            Order = _serialNumber++;
            AreaGroup = areaGroup;
            Id = new GameDataObjectId(GameDataObjectCategory.Area, areaId);
            _nameId = new TranslationTextId(TranslationCategory.Area, areaId);
            FishingSpots = new FishingSpotCollection();
            _percentageOfWeather = percentageOfWeather.ToArray();
            if (_percentageOfWeather.Sum(item => item.Percentage) != 100)
                throw new Exception();
        }

        public int Order { get; }

        public AreaGroup AreaGroup { get; }

        public GameDataObjectId Id { get; }
        public string Name => Translate.Instance[_nameId];
        public FishingSpotCollection FishingSpots { get; }

        public WeatherType ForecastWeather(DateTime time)
        {
            return ForecastWeather(time.ToEorzeaDateTime());
        }

        public WeatherType ForecastWeather(EorzeaDateTime time)
        {
            if (time.EpochSeconds < 0)
                return WeatherType.None;
            var zone = ((time.EpochHours / 8) + 1) * 8 % 24;
            var temp0 = (time.EpochDays + 304512) * 100 + zone;
            if (temp0 < uint.MinValue || temp0 > uint.MaxValue)
                throw new Exception();
            var temp1 = (uint)temp0;
            var temp2 = (temp1 << 11) ^ temp1;
            var temp3 = (temp2 >> 8) ^ temp2;
            var index = (int)(temp3 % 100);
            if (index < 0 || index >= 100)
                throw new Exception();
            foreach (var item in _percentageOfWeather)
            {
                if (index < item.Percentage)
                    return item.Weather;
                index -= item.Percentage;
            }
            throw new Exception();
        }

        public int GetWeatherPercentage(WeatherType weathers)
        {
            return _percentageOfWeather.Where(item => (item.Weather & weathers) != WeatherType.None).Sum(item => item.Percentage);
        }

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(_nameId);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return Id.Equals(((Area)o).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
