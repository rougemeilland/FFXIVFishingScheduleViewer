using System;
using System.Linq;

namespace FishingScheduler
{
    class Area
    {
        private static int _serialNumber = 0;
        private KeyValueCollection<string, FishingGround> _fishingGrounds;
        private WeatherPercentageOfArea[] _percentageOfWeather;

        public Area(AreaGroup areaGroup, string areaName, WeatherPercentageOfArea[] percentageOfWeather)
        {
            Order = _serialNumber++;
            AreaGroup = areaGroup;
            AreaName = areaName;
            _fishingGrounds = new KeyValueCollection<string, FishingGround>();
            _percentageOfWeather = percentageOfWeather.ToArray();
            if (_percentageOfWeather.Sum(item => item.Percentage) != 100)
                throw new Exception();
        }

        public int Order { get; }

        public AreaGroup AreaGroup { get; }

        public string AreaName { get; }

        public IKeyValueCollection<string, FishingGround> FishingGrounds => _fishingGrounds;

        public void AddFishingGroup(FishingGround fishingGround)
        {
            _fishingGrounds.Add(fishingGround.FishingGroundName, fishingGround);
        }

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

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return AreaName.Equals(((Area)o).AreaName);
        }

        public override int GetHashCode()
        {
            return AreaName.GetHashCode();
        }
    }
}
