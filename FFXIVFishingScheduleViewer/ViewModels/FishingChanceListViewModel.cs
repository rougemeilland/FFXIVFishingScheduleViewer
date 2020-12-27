using FFXIVFishingScheduleViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class FishingChanceListViewModel
        : ViewModel
    {
        private class FishChanceTimeRegionsComparer
            : IEqualityComparer<FishChanceTimeRegions>
        {
            bool IEqualityComparer<FishChanceTimeRegions>.Equals(FishChanceTimeRegions x, FishChanceTimeRegions y)
            {
                return
                    x.FishingCondition.Fish.Equals(y.FishingCondition.Fish) &&
                    x.FishingCondition.FishingSpot.Equals(y.FishingCondition.FishingSpot);
            }

            int IEqualityComparer<FishChanceTimeRegions>.GetHashCode(FishChanceTimeRegions obj)
            {
                return obj.FishingCondition.Fish.GetHashCode() ^ obj.FishingCondition.FishingSpot.GetHashCode();
            }
        }

        private static TimeSpan _maxInterval = EorzeaTimeSpan.FromHours(8).ToEarthTimeSpan();
        private bool _isDisposed;
        private GameData _gameData;
        private Dispatcher _dispatcher;
        private DateTime _currentTime;
        private bool _isPendingFishFilterChandedEvent;
        private bool _useFishEye;
        private bool _excludeMooching;

        public FishingChanceListViewModel(GameData gameData, Dispatcher dispatcher)
        {
            _isDisposed = false;
            _gameData = gameData;
            _dispatcher = dispatcher;
            _currentTime = DateTime.MinValue;
            var source =
                _gameData.Fishes
                .SelectMany(fish => fish.FishingConditions)
                .Select(item => new { fish = item.Fish, fishingSpot = item.FishingSpot, area = item.FishingSpot.Area, areaGroup = item.FishingSpot.Area.AreaGroup });
            var areaList =
                source
                .GroupBy(item => item.area)
                .Select(g => new { area = g.Key, areaGroup = g.Key.AreaGroup, fishCount = g.Count() })
                .GroupBy(item => item.areaGroup)
                .Select(g => new
                {
                    areaGroup = g.Key,
                    areas = g
                        .Select(item => new { item.area, item.fishCount })
                        .OrderBy(item => item.area.Order)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.areaGroup, item => item.areas);
            var fishingSpotList =
                source
                .GroupBy(item => item.fishingSpot)
                .Select(g => new { fishingSpot = g.Key, fishCount = g.Count() })
                .GroupBy(item => item.fishingSpot.Area)
                .Select(g => new
                {
                    area = g.Key,
                    fishingSpots = g
                        .Select(item => new { item.fishingSpot, item.fishCount })
                        .OrderBy(item => item.fishingSpot.Order)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.area, item => item.fishingSpots);
            var fishListOfFishingSpot =
                source
                .GroupBy(item => item.fishingSpot)
                .Select(g => new
                {
                    fishingSpot = g.Key,
                    fishes = g
                        .Select(item => item.fish)
                        .OrderBy(item => item.DifficultyValue)
                        .ThenBy(item => item.Order)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.fishingSpot, item => item.fishes);
            FishingChanceList = new FishChanceTimeRegions[0];

            _gameData.SettingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            _gameData.SettingProvider.ForecastWeatherDaysChanged += _settingProvider_ForecastWeatherDaysChanged;
            _gameData.SettingProvider.FishMemoChanged += _settingProvider_FishMemoChanged;
            _gameData.SettingProvider.FishFilterChanded += _settingProvider_FishFilterChanded;
            _gameData.SettingProvider.FishingChanceListTextEffectChanged += _settingProvider_FishingChanceListTextEffectChanged;

            _isPendingFishFilterChandedEvent = false;
            _useFishEye = false;
            _excludeMooching = false;
        }

        public int ForecastWeatherDays => _gameData.SettingProvider.ForecastWeatherDays;
        public FishingChanceListTextEffectType FishingChanceListTextEffect => _gameData.SettingProvider.FishingChanceListTextEffect;

        public DateTime CurrentTime
        {
            get
            {
                return _currentTime;
            }

            set
            {
                if (value != _currentTime)
                {
                    var previousTime = _currentTime;
                    _currentTime = value;
                    if (previousTime == DateTime.MinValue || value - previousTime >= _maxInterval)
                    {
                        // 初回、または以前に呼び出されたときからETで8時間以上経過している場合
                        UpdateFishingChanceList(_currentTime);
                    }
                    else
                    {
                        var previousDate = previousTime.ToLocalTime().Date;
                        var currentDate = _currentTime.ToLocalTime().Date;
                        if (previousDate != currentDate)
                        {
                            // 以前に呼び出された時から、LTで日付が変わっている場合
                            UpdateFishingChanceList(_currentTime);
                        }
                        else
                        {
                            var previousEorzeaTime = previousTime.ToEorzeaDateTime();
                            var currentEorzeaTime = _currentTime.ToEorzeaDateTime();
                            if (previousEorzeaTime.GetStartOf8Hour() != currentEorzeaTime.GetStartOf8Hour())
                            {
                                // 以前に呼び出された時から、ETで0時/8時/16時の境界を越えている場合
                                UpdateFishingChanceList(_currentTime);
                            }
                        }
                    }
                    RaisePropertyChangedEvent(nameof(CurrentTime));
                }
            }
        }


        public IEnumerable<FishChanceTimeRegions> FishingChanceList { get; private set; }
        public IEnumerable<EorzeaDateTime> FishingChanceTimeList { get; private set; }

        public bool UseFishEye
        {
            get => _useFishEye;

            set
            {
                if (value != _useFishEye)
                {
                    _useFishEye = value;
                    UpdateFishingChanceList(DateTime.UtcNow);
                    RaisePropertyChangedEvent(nameof(UseFishEye));
                }
            }
        }

        public bool ExcludeMooching
        {
            get => _excludeMooching;

            set
            {
                if (value != _excludeMooching)
                {
                    _excludeMooching = value;
                    UpdateFishingChanceList(DateTime.UtcNow);
                    RaisePropertyChangedEvent(nameof(ExcludeMooching));
                }
            }
        }

        public void SetFishFilter(Fish fish, bool isEnabled)
        {
            _gameData.SettingProvider.SetIsEnabledFishFilter(fish, isEnabled);
        }

        public string GetFishMemo(Fish fish, FishingSpot fishingSpot)
        {
            return _gameData.SettingProvider.GetFishMemo(fish, fishingSpot);
        }

        public void SetFishMemo(Fish fish, FishingSpot fishingSpot, string memo)
        {
            _gameData.SettingProvider.SetFishMemo(fish, fishingSpot, memo);
        }

        public event EventHandler<FishMemoChangedEventArgs> FishMemoChanged;

        public FishDetailWindowViewModel GetDetailViewModel(Fish fish, FishingSpot fishingSpot)
        {
            return new FishDetailWindowViewModel(fish, fishingSpot, spot => GetFishMemo(fish, spot), _gameData.SettingProvider);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _gameData.SettingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _gameData.SettingProvider.ForecastWeatherDaysChanged -= _settingProvider_ForecastWeatherDaysChanged;
                _gameData.SettingProvider.FishMemoChanged -= _settingProvider_FishMemoChanged;
                _gameData.SettingProvider.FishFilterChanded -= _settingProvider_FishFilterChanded;
                _gameData.SettingProvider.FishingChanceListTextEffectChanged -= _settingProvider_FishingChanceListTextEffectChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishingChanceList));
            RaisePropertyChangedEvent(nameof(GUIText));
        }

        private void _settingProvider_ForecastWeatherDaysChanged(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            UpdateFishingChanceList(now);
            RaisePropertyChangedEvent(nameof(ForecastWeatherDays));
            RaisePropertyChangedEvent(nameof(FishingChanceTimeList));
            RaisePropertyChangedEvent(nameof(FishingChanceList));
        }

        private void _settingProvider_FishMemoChanged(object sender, FishMemoChangedEventArgs e)
        {
            var now = DateTime.UtcNow;
            UpdateFishingChanceList(now);
            RaisePropertyChangedEvent(nameof(FishingChanceList));
            FishMemoChanged(this, e);
        }

        private void _settingProvider_FishFilterChanded(object sender, Fish e)
        {
            _isPendingFishFilterChandedEvent = true;
            _dispatcher.InvokeAsync(() =>
            {
                if (_isPendingFishFilterChandedEvent)
                {
                    var now = DateTime.UtcNow;
                    UpdateFishingChanceList(now);
                    _isPendingFishFilterChandedEvent = false;
                }
            });
        }

        private void _settingProvider_FishingChanceListTextEffectChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishingChanceListTextEffect));
        }

        private void UpdateFishingChanceList(DateTime now)
        {
            var eorzeaNow = now.ToEorzeaDateTime().GetStartOf8Hour();
            // 開始時刻の8時間前から、開始時刻の[Properties.Settings.Default.DaysOfForecast]日後まで
            var wholePeriod = new EorzeaDateTimeHourRegions(new[]
            {
                new EorzeaDateTimeRegion(
                    eorzeaNow - EorzeaTimeSpan.FromHours(8),
                    eorzeaNow + EorzeaTimeSpan.FromDays(_gameData.SettingProvider.ForecastWeatherDays)
                )
            });
            var forecastPeriod = new EorzeaDateTimeHourRegions(new[]
            {
                new EorzeaDateTimeRegion(
                    eorzeaNow,
                    eorzeaNow + EorzeaTimeSpan.FromDays(_gameData.SettingProvider.ForecastWeatherDays)
                )
            });
            var founds =
                _gameData.Fishes
                .Where(fish => _gameData.SettingProvider.GetIsEnabledFishFilter(fish))
                .OrderByDescending(fish => fish.DifficultyValue)
                .SelectMany(fish => fish.GetFishingChance(wholePeriod, UseFishEye))
                .Where(result => result != null && !result.Regions.Intersect(forecastPeriod).IsEmpty)
                .Where(result => !UseFishEye || !ExcludeMooching || !result.FishingCondition.NeedMooching)
                .ToArray();
            var comparer = new FishChanceTimeRegionsComparer();
            FishingChanceTimeList =
                Enumerable.Range(0, 24 * _gameData.SettingProvider.ForecastWeatherDays + 8)
                .Select(index => eorzeaNow + EorzeaTimeSpan.FromHours(index - 8))
                .ToArray();
            FishingChanceList =
                FishingChanceTimeList
                .Select(time => founds
                    .Where(result => result.Regions.Contains(time))
                    .GroupBy(result => result.FishingCondition.Fish) // 魚毎にまとめる
                    .Select(g => g.OrderBy(item => item.FishingCondition.GetExpectedCountOfCasting()).First()) // 同じ魚が複数の場所に見つかった場合は釣るのにいちばん手間のかからない場所を選ぶ
                    .OrderByDescending(result => result.FishingCondition.Fish.DifficultyValue) // 時間帯がかぶっている魚がいる場合は発見しにくい魚を選ぶ
                    .ThenByDescending(result => result.FishingCondition.GetExpectedCountOfCasting()) // 発見のしにくさが同じ場合は釣りにかかる手間が大きい方を選ぶ
                    .ThenBy(result => result.FishingCondition.FishingSpot.Area.AreaGroup.Order) // 以降の並べ替えは特に重要な意味はない。(整列順序を一意に定めるだけ)
                    .ThenBy(result => result.FishingCondition.FishingSpot.Area.Order)
                    .ThenBy(result => result.FishingCondition.FishingSpot.Order)
                    .FirstOrDefault())
                .Where(result => result != null)
                .Distinct(comparer)
                .Select(result => new { result, firstRegion = result.Regions.Intersect(forecastPeriod).DateTimeRegions.First() })
                .OrderBy(item => item.firstRegion.Begin)
                .ThenBy(item => item.firstRegion.End)
                .Select(item => item.result)
                .ToArray();
            RaisePropertyChangedEvent(nameof(FishingChanceTimeList));
            RaisePropertyChangedEvent(nameof(FishingChanceList));
        }
    }
}
