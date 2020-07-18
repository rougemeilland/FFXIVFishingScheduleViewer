using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FishingScheduler
{
    class WeatherListViewModel
        : ViewModel
    {
        private const int _maxWeatherColumnsCount = 22;
        private bool _isDisposed;
        private ISettingProvider _settingProvider;

        public WeatherListViewModel(AreaGroup areaGroup, IEnumerable<WeatherListCellViewModel> columnHeaders, IEnumerable<WeatherOfAreaViewModel> weathers, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            AreaGroup = areaGroup;
            ColumnHeaders = columnHeaders.Concat(Enumerable.Repeat(new WeatherListCellViewModel(), _maxWeatherColumnsCount)).Take(_maxWeatherColumnsCount).ToArray();
            WeatherList = new ObservableCollection<WeatherOfAreaViewModel>(weathers);
            GUIText = GUITextTranslate.Instance;
            _settingProvider = settingProvider;
            _settingProvider.AreaGroupOnForecastWeatherExpanded += _settingProvider_AreaGroupOnForecastWeatherExpanded;
            _settingProvider.AreaGroupOnForecastWeatherContracted += _settingProvider_AreaGroupOnForecastWeatherContracted;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
        }

        public AreaGroup AreaGroup { get; }
        public string AreaGroupName => AreaGroup.Name;

        public bool IsExpanded
        {
            get => _settingProvider.GetIsExpandedAreaGroupOnForecastWeather(AreaGroup);
            set => _settingProvider.SetIsExpandedAreaGroupOnForecastWeather(AreaGroup, value);
        }

        public WeatherListCellViewModel[] ColumnHeaders { get; }
        public ObservableCollection<WeatherOfAreaViewModel> WeatherList { get; }
        public GUITextTranslate GUIText { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var item in WeatherList)
                    item.Dispose();
                foreach (var item in ColumnHeaders)
                    item.Dispose();
                _settingProvider.AreaGroupOnForecastWeatherExpanded -= _settingProvider_AreaGroupOnForecastWeatherExpanded;
                _settingProvider.AreaGroupOnForecastWeatherContracted -= _settingProvider_AreaGroupOnForecastWeatherContracted;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_AreaGroupOnForecastWeatherExpanded(object sender, AreaGroup e)
        {
            if (e == AreaGroup)
                RaisePropertyChangedEvent(nameof(IsExpanded));
        }

        private void _settingProvider_AreaGroupOnForecastWeatherContracted(object sender, AreaGroup e)
        {
            if (e == AreaGroup)
                RaisePropertyChangedEvent(nameof(IsExpanded));
        }

        private void _settingProvider_UserLanguageChanged(object sender, System.EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(AreaGroupName));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}
