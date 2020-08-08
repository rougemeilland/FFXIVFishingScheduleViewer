using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVFishingScheduleViewer.Strings;

namespace FFXIVFishingScheduleViewer.Models
{
    class Area
        : IGameDataObject
    {
        private static int _serialNumber = 0;
        private string _rawId;
        private WeatherPercentageOfArea[] _percentageOfWeather;

        public Area(AreaGroup areaGroup, string areaId, WeatherPercentageOfArea[] percentageOfWeather)
        {
            Order = _serialNumber++;
            _rawId = areaId;
            Id = new GameDataObjectId(GameDataObjectCategory.Area, areaId);
            NameId = new TranslationTextId(TranslationCategory.Area, areaId);
            AreaGroup = areaGroup;
            FishingSpots = new FishingSpotCollection();
            _percentageOfWeather = percentageOfWeather.ToArray();
            if (_percentageOfWeather.Sum(item => item.Percentage) != 100)
                throw new Exception();
        }

        string IGameDataObject.InternalId => _rawId;
        public int Order { get; }
        public GameDataObjectId Id { get; }
        public TranslationTextId NameId { get; }
        public string Name => Translate.Instance[NameId];
        public AreaGroup AreaGroup { get; }
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
#if DEBUG
            if (temp0 < uint.MinValue || temp0 > uint.MaxValue)
                throw new Exception();
#endif
            var temp1 = (uint)temp0;
            var temp2 = (temp1 << 11) ^ temp1;
            var temp3 = (temp2 >> 8) ^ temp2;
            var index = (int)(temp3 % 100);
#if DEBUG
            if (index < 0 || index >= 100)
                throw new Exception();
#endif
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

        public bool ContainsWeather(WeatherType weather)
        {
            return _percentageOfWeather.Any(item => (item.Weather & weather) != WeatherType.None);
        }

        public IEnumerable<string> CheckTranslation()
        {
            return Translate.Instance.CheckTranslation(NameId);
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
