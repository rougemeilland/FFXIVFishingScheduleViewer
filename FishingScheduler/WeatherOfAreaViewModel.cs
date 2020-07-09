using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class WeatherOfAreaViewModel
        : ViewModel
    {
        public WeatherOfAreaViewModel(string areaName, IEnumerable<WeatherViewModel> weathers)
        {
            AreaName = areaName;
            Weathers = weathers.ToArray();
        }

        public string AreaName { get; }
        public WeatherViewModel[] Weathers { get; }
    }
}
