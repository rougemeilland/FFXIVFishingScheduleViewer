using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer
{
    class MainViewModel
        : ViewModel
    {
        private class FishChanceTimeRegionsComparer
            : IEqualityComparer<FishChanceTimeRegions>
        {
            public bool Equals(FishChanceTimeRegions x, FishChanceTimeRegions y)
            {
                return
                    x.FishingCondition.Fish.Equals(y.FishingCondition.Fish) &&
                    x.FishingCondition.FishingSpot.Equals(y.FishingCondition.FishingSpot);
            }

            public int GetHashCode(FishChanceTimeRegions obj)
            {
                return obj.FishingCondition.Fish.GetHashCode() ^ obj.FishingCondition.FishingSpot.GetHashCode();
            }
        }

        private bool _isDisposed;
        private string _windowTitleText;
        private Dispatcher _dispatcher;
        private AreaGroupCollection _areaGroups;
        private FishCollection _fishes;
        private ISettingProvider _settingProvider;
        private DateTime _currentTime;
        private BrushConverter _brushConverter;
        private bool _isPendingFishFilterChandedEvent;

        public MainViewModel(string windowTitleText, Dispatcher dispatcher, AreaGroupCollection areaGroups, FishCollection fishes, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _windowTitleText = windowTitleText;
            _dispatcher = dispatcher;
            _areaGroups = areaGroups;
            _fishes = fishes;
            _settingProvider = settingProvider;
            _currentTime = DateTime.MinValue;
            _brushConverter = new BrushConverter();
            var source =
                _fishes
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
            WeatherList = new WeatherListViewModel[0];
            GUIText = GUITextTranslate.Instance;
            UpdatedDateTime = DateTime.UtcNow;
            FishingChanceList = new FishChanceTimeRegions[0];

            ShowDownloadPageCommand = null;
            OptionMenuCommand = null;
            ExitMenuCommand = null;
            ViewREADMEMenuCommand = null;
            AboutMenuCommand = null;

            _settingProvider = settingProvider;
            _settingProvider.SelectedMainViewTabIndexChanged += _settingProvider_SelectedMainViewTabIndexChanged;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            _settingProvider.ForecastWeatherDaysChanged += _settingProvider_ForecastWeatherDaysChanged;
            _settingProvider.FishMemoChanged += _settingProvider_FishMemoChanged;
            _settingProvider.FishFilterChanded += _settingProvider_FishFilterChanded;
            _settingProvider.NewVersionOfApplicationChanged += _settingProvider_NewVersionOfApplicationChanged;
            _settingProvider.FishingChanceListTextEffectChanged += _settingProvider_FishingChanceListTextEffectChanged;

            CurrentTime = DateTime.UtcNow;
            CurrentDateTimeText = "";
            NewVersionReleasedText = null;
            _isPendingFishFilterChandedEvent = false;
            _settingProvider.CheckNewVersionReleased();
        }

        public GUITextTranslate GUIText { get; }
        public string CurrentDateTimeText { get; private set; }
        public string NewVersionReleasedText { get; private set; }
        public string AboutMenuText => string.Format(GUITextTranslate.Instance["Menu.About"], _settingProvider.ProductName);

        public int SelectedMainViewTabIndex
        {
            get => _settingProvider.SelectedMainViewTabIndex;
            set => _settingProvider.SelectedMainViewTabIndex = value;
        }

        public FishingChanceListTextEffectType FishingChanceListTextEffect
        {
            get => _settingProvider.FishingChanceListTextEffect;
        }

        public IEnumerable<WeatherListViewModel> WeatherList { get; private set; }
        public ICommand ShowDownloadPageCommand { get; set; }
        public ICommand OptionMenuCommand { get; set; }
        public ICommand ExitMenuCommand { get; set; }
        public ICommand ViewREADMEMenuCommand { get; set; }
        public ICommand AboutMenuCommand { get; set; }

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
                    if (previousTime == DateTime.MinValue || value - previousTime >= TimeSpan.FromMinutes(70.0 / 3))
                    {
                        UpdateWeatherList(_currentTime);
                        UpdateFishingChanceList(_currentTime);
                    }
                    else
                    {
                        var previousEorzeaTime = previousTime.ToEorzeaDateTime();
                        var currentEorzeaTime = _currentTime.ToEorzeaDateTime();
                        if (previousEorzeaTime.EpochHours / 8 != currentEorzeaTime.EpochHours / 8)
                        {
                            UpdateWeatherList(_currentTime);
                            UpdateFishingChanceList(_currentTime);
                        }
                    }
                    UpdateCurrentDateTimeText(_currentTime);
                    RaisePropertyChangedEvent(nameof(CurrentTime));
                }
            }
        }

        public int ForecastWeatherDays => _settingProvider.ForecastWeatherDays;

        public IEnumerable<FishChanceTimeRegions> FishingChanceList { get; private set; }
        public IEnumerable<EorzeaDateTime> FishingChanceTimeList { get; private set; }
        public DateTime UpdatedDateTime { get; private set; }
        public string UrlOfDownloadPage => _settingProvider.UrlOfDownloadPage;
        public event EventHandler<FishMemoChangedEventArgs> FishMemoChanged;


        public void SetFishFilter(Fish fish, bool isEnabled)
        {
            _settingProvider.SetIsEnabledFishFilter(fish, isEnabled);
        }

        public string GetFishMemo(Fish fish, FishingSpot fishingSpot)
        {
            return _settingProvider.GetFishMemo(fish, fishingSpot);
        }

        public void SetFishMemo(Fish fish, FishingSpot fishingSpot, string memo)
        {
            _settingProvider.SetFishMemo(fish, fishingSpot, memo);
        }

        public FishDetailViewModel GetDetailViewModel(Fish fish, FishingSpot fishingSpot)
        {
            return new FishDetailViewModel(_windowTitleText, fish, fishingSpot, spot => GetFishMemo(fish, spot), _settingProvider);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _settingProvider.SelectedMainViewTabIndexChanged -= _settingProvider_SelectedMainViewTabIndexChanged;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _settingProvider.ForecastWeatherDaysChanged -= _settingProvider_ForecastWeatherDaysChanged;
                _settingProvider.FishMemoChanged -= _settingProvider_FishMemoChanged;
                _settingProvider.FishFilterChanded -= _settingProvider_FishFilterChanded;
                _settingProvider.NewVersionOfApplicationChanged -= _settingProvider_NewVersionOfApplicationChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            UpdateCurrentDateTimeText(DateTime.UtcNow);
            UpdateNewVersionReleasedText();
            RaisePropertyChangedEvent(nameof(FishingChanceList));
            RaisePropertyChangedEvent(nameof(AboutMenuText));
            RaisePropertyChangedEvent(nameof(GUIText));
        }

        private void _settingProvider_NewVersionOfApplicationChanged(object sender, EventArgs e)
        {
            _dispatcher.InvokeAsync(() =>
            {
                UpdateNewVersionReleasedText();
            });
        }

        private void _settingProvider_FishingChanceListTextEffectChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishingChanceListTextEffect));
        }

        private void UpdateCurrentDateTimeText(DateTime currentDateTime)
        {
            var localTimeNow = currentDateTime.ToLocalTime();
            var eorzeaTimeNow = currentDateTime.ToEorzeaDateTime();
            CurrentDateTimeText =
                string.Format(
                    "{0} {1:D02}:{2:D02} ( {3} {4:D02}:{5:D02}:{6:D02} )",
                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ET.Short")],
                    eorzeaTimeNow.Hour,
                    eorzeaTimeNow.Minute,
                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "LT.Short")],
                    localTimeNow.Hour,
                    localTimeNow.Minute,
                    localTimeNow.Second);
            RaisePropertyChangedEvent(nameof(CurrentDateTimeText));
        }

        private void UpdateNewVersionReleasedText()
        {
            var format = GUITextTranslate.Instance["Label.NewVersionReleased"];
            NewVersionReleasedText =
                _settingProvider.NewVersionOfApplication != null
                ? string.Format(
                    GUITextTranslate.Instance["Label.NewVersionReleased"],
                    _settingProvider.NewVersionOfApplication,
                    _settingProvider.CurrentVersionOfApplication)
                : null;
            RaisePropertyChangedEvent(nameof(NewVersionReleasedText));
        }

        private void UpdateWeatherList(DateTime now)
        {
            UpdatedDateTime = now;
            var eorzeaNow = UpdatedDateTime.ToEorzeaDateTime().GetStartOf8Hour();
            var timeList = Enumerable.Range(-1, _settingProvider.ForecastWeatherDays * 3 + 2)
                                .Select(index =>
                                {
                                    var startTime = eorzeaNow + EorzeaTimeSpan.FromHours(index * 8);
                                    var localStartTime = startTime.ToEarthDateTime().ToLocalTime();
                                    string headerText =
                                        string.Format(
                                            "{0} {1:D02}:{2:D02}:{3:D02} -\n( {4} {5:D02}:{6:D02} - )",
                                            Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "LT.Short")],
                                            localStartTime.Hour,
                                            localStartTime.Minute,
                                            localStartTime.Second,
                                            Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ET.Short")],
                                            startTime.Hour,
                                            startTime.Minute);
                                    var isLastTime = index == _settingProvider.ForecastWeatherDays * 3;
                                    return new { index, headerText, startTime, isLastTime };
                                })
                                .ToArray();
            foreach (var item in WeatherList)
                item?.Dispose();
            WeatherList =
                _areaGroups
                .Select(areaGroup =>
                {
                    var lastArea = areaGroup.Areas.Last();
                    return
                        new WeatherListViewModel(
                            areaGroup,
                            timeList.Select(item =>
                                new WeatherListCellViewModel(
                                    () => item.headerText,
                                    _settingProvider)
                                {
                                    Background = (Brush)_brushConverter.ConvertFromString(item.index == 0 ? "#ffd700" : "#eeeeee"),
                                    CellPositionType = item.isLastTime ? "TopRight" : "Top",
                                }),
                            areaGroup.Areas
                                .Select(area =>
                                {
                                    var isLastArea = area == lastArea;
                                    return new WeatherOfAreaViewModel(
                                        area,
                                        timeList.Select(item => GetCellForWeather(item.index, area.ForecastWeather(item.startTime), item.isLastTime, isLastArea)),
                                        isLastArea,
                                        _settingProvider);
                                }),
                            _settingProvider
                        );
                })
                .ToArray();
            RaisePropertyChangedEvent(nameof(UpdatedDateTime));
            RaisePropertyChangedEvent(nameof(WeatherList));
        }

        private WeatherListCellViewModel GetCellForWeather(int columnIndex, WeatherType weather, bool isLastTime, bool isLastArea)
        {
            string foreground = "black";
            string background = "white";
            switch (weather)
            {
                case WeatherType.暴雨:
                case WeatherType.雨:
                    foreground = "blue";
                    background = "aqua";
                    break;
                case WeatherType.快晴:
                case WeatherType.晴れ:
                    foreground = "deepskyblue";
                    background = "azure";
                    break;
                case WeatherType.砂塵:
                    foreground = "saddlebrown";
                    background = "burlywood";
                    break;
                case WeatherType.灼熱波:
                    foreground = "crimson";
                    background = "salmon";
                    break;
                case WeatherType.吹雪:
                case WeatherType.雪:
                    foreground = "royalblue";
                    background = "lightcyan";
                    break;
                case WeatherType.霧:
                case WeatherType.曇り:
                case WeatherType.風:
                case WeatherType.暴風:
                    foreground = "#333333";
                    background = "#bbbbbb";
                    break;
                case WeatherType.妖霧:
                case WeatherType.放電:
                case WeatherType.雷:
                case WeatherType.雷雨:
                case WeatherType.霊風:
                    foreground = "#551966";
                    background = "#eec0ee";
                    break;
                default:
                    break;
            }
            Brush foregroundBrush;
            Brush backgroundBrush;
            if (false && columnIndex == -1)
            {
                // not used

                var lightForegroundColor = (Color)ColorConverter.ConvertFromString("#000000");
                var foregroundColor = (Color)ColorConverter.ConvertFromString(foreground);
                foregroundBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            (byte)((lightForegroundColor.R + foregroundColor.R) / 2),
                            (byte)((lightForegroundColor.G + foregroundColor.G) / 2),
                            (byte)((lightForegroundColor.B + foregroundColor.B) / 2)));
                var darkBackgroundColor = (Color)ColorConverter.ConvertFromString("#ffffff");
                var backgroundColor = (Color)ColorConverter.ConvertFromString(background);
                backgroundBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            (byte)((darkBackgroundColor.R + backgroundColor.R) / 2),
                            (byte)((darkBackgroundColor.G + backgroundColor.G) / 2),
                            (byte)((darkBackgroundColor.B + backgroundColor.B) / 2)));
            }
            else
            {
                foregroundBrush = (Brush)_brushConverter.ConvertFromString(foreground);
                backgroundBrush = (Brush)_brushConverter.ConvertFromString(background);
            }
            string cellPositionType;
            if (isLastArea)
                cellPositionType = isLastTime ? "BottomRight" : "Bottom";
            else
                cellPositionType = isLastTime ? "Right" : "";
            return
                new WeatherListCellViewModel(
                    () => weather.GetText(),
                    _settingProvider)
                {
                    Background = backgroundBrush,
                    Foreground = foregroundBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    CellPositionType = cellPositionType
                };
        }

        private void UpdateFishingChanceList(DateTime now)
        {
            var eorzeaNow = now.ToEorzeaDateTime().GetStartOf8Hour();
            // 開始時刻の8時間前から、開始時刻の[Properties.Settings.Default.DaysOfForecast]日後まで
            var wholePeriod = new EorzeaDateTimeHourRegions(new[]
            {
                new EorzeaDateTimeRegion(
                    eorzeaNow - EorzeaTimeSpan.FromHours(8),
                    eorzeaNow + EorzeaTimeSpan.FromDays(_settingProvider.ForecastWeatherDays)
                )
            });
            var forecastPeriod = new EorzeaDateTimeHourRegions(new[]
            {
                new EorzeaDateTimeRegion(
                    eorzeaNow,
                    eorzeaNow + EorzeaTimeSpan.FromDays(_settingProvider.ForecastWeatherDays)
                )
            });
            var founds =
                _fishes
                .Where(fish => _settingProvider.GetIsEnabledFishFilter(fish))
                .OrderByDescending(fish => fish.DifficultyValue)
                .SelectMany(fish => fish.GetFishingChance(wholePeriod))
                .Where(result => result != null && !result.Regions.Intersect(forecastPeriod).IsEmpty)
                .ToArray();
            var comparer = new FishChanceTimeRegionsComparer();
            FishingChanceTimeList =
                Enumerable.Range(0, 24 * _settingProvider.ForecastWeatherDays + 8)
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

        private void _settingProvider_SelectedMainViewTabIndexChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedMainViewTabIndex));
        }

        private void _settingProvider_ForecastWeatherDaysChanged(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            UpdateWeatherList(now);
            UpdateFishingChanceList(now);
            RaisePropertyChangedEvent(nameof(ForecastWeatherDays));
            RaisePropertyChangedEvent(nameof(WeatherList));
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
    }
}
