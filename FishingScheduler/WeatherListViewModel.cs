using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class WeatherListViewModel
        : ViewModel
    {
        private ISettingProvider _areaGroupSetting;

        public WeatherListViewModel(string areaGroupName, IEnumerable<string> columnHeaders, IEnumerable<WeatherOfAreaViewModel> weathers, ISettingProvider areaGroupSetting)
        {
            AreaGroupName = areaGroupName;
            ColumnHeaders = columnHeaders.Concat(Enumerable.Repeat("", 22)).Take(22).ToArray();
            WeatherList = new ObservableCollection<WeatherOfAreaViewModel>(weathers);
            _areaGroupSetting = areaGroupSetting;
        }

        public string AreaGroupName { get; }

        public bool IsExpanded
        {
            get
            {
                return _areaGroupSetting.GetIsExpandedAreaGroupOnForecastWeather(AreaGroupName);
            }

            set
            {
                if (_areaGroupSetting.SetIsExpandedAreaGroupOnForecastWeather(AreaGroupName, value))
                    RaisePropertyChangedEvent(nameof(IsExpanded));
            }
        }

        public string[] ColumnHeaders { get; }
        public ObservableCollection<WeatherOfAreaViewModel> WeatherList { get; }
    }
}
