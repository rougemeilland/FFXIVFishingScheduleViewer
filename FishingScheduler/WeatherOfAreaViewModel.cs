using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
{
    class WeatherOfAreaViewModel
        : ViewModel
    {
        private const int _maxWeatherColumnsCount = 22;
        private bool _isDisposed;
        private Area _area;
        private ISettingProvider _settingProvider;

        public WeatherOfAreaViewModel(Area area, IEnumerable<WeatherListCellViewModel> weathers, bool isLastArea, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _area = area;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            AreaName =
                new WeatherListCellViewModel(
                    () => _area.Name,
                    _settingProvider)
                {
                    CellPositionType = isLastArea ? "BottomLeft" : "Left",
                };
            Weathers = weathers.Concat(Enumerable.Repeat(new WeatherListCellViewModel(), _maxWeatherColumnsCount)).Take(_maxWeatherColumnsCount).ToArray();
        }

        public WeatherListCellViewModel AreaName { get; }
        public WeatherListCellViewModel[] Weathers { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var item in Weathers)
                    item.Dispose();
                AreaName.Dispose();
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, System.EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(AreaName));
        }
    }
}
