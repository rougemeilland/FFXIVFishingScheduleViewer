using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer
{
    class DataContext
        : ViewModel
    {
        private class FishChanceTimeRegionsComparer
            : IEqualityComparer<FishChanceTimeRegions>
        {
            public bool Equals(FishChanceTimeRegions x, FishChanceTimeRegions y)
            {
                return x.Fish.Id.Equals(y.Fish.Id);
            }

            public int GetHashCode(FishChanceTimeRegions obj)
            {
                return obj.Fish.Id.GetHashCode();
            }
        }

        private bool _isDisposed;
        private Dispatcher _dispatcher;
        private AreaGroupCollection _areaGroups;
        private FishCollection _fishes;
        private WeatherListViewModel[] _weatherList;
        private ISettingProvider _settingProvider;
        private DateTime _currentTime;
        private bool _isPendingFishFilterChandedEvent;
        private BrushConverter _brushConverter;

        public DataContext(Dispatcher dispatcher, AreaGroupCollection areaGroups, FishCollection fishes, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _dispatcher = dispatcher;
            _areaGroups = areaGroups;
            _fishes = fishes;
            _settingProvider = settingProvider;
            _currentTime = DateTime.MinValue;
            _brushConverter = new BrushConverter();
            var source =
                _fishes
                .SelectMany(fish => fish.FishingSpots, (fish, fishingSpot) => new { fish, fishingSpot })
                .Select(item => new { item.fish, item.fishingSpot, area = item.fishingSpot.Area, areaGroup = item.fishingSpot.Area.AreaGroup });
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
                        .ThenBy(item => item.Name)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.fishingSpot, item => item.fishes);
            FishSettingOfWorld =
                source
                .GroupBy(item => item.areaGroup)
                .Select(g => new { areaGroup = g.Key, fishCount = g.Count() })
                .OrderBy(areaGroupInfo => areaGroupInfo.areaGroup.Order)
                .Select(areaGroupInfo => new FishSettingOfAreaGroupViewModel(
                    areaGroupInfo.areaGroup,
                    areaList[areaGroupInfo.areaGroup]
                        .Select(areaInfo => new FishSettingOfAreaViewModel(
                            areaInfo.area,
                            fishingSpotList[areaInfo.area]
                                .Select(fishingSpotInfo =>
                                {
                                    var fishesOfSpot = fishListOfFishingSpot[fishingSpotInfo.fishingSpot];
                                    var firstFish = fishesOfSpot.First();
                                    var lastFish = fishesOfSpot.Last();
                                    return
                                        new FishSettingOfFishingSpotViewModel(
                                            fishingSpotInfo.fishingSpot,
                                            fishesOfSpot
                                                .Select(fish =>
                                                {
                                                    string cellPositionType;
                                                    if (fish == firstFish)
                                                        cellPositionType = "Top";
                                                    else if (fish == lastFish)
                                                        cellPositionType = "Bottom";
                                                    else
                                                        cellPositionType = "";
                                                    return
                                                        new FishSettingViewModel(
                                                            fish,
                                                            fishingSpotInfo.fishingSpot,
                                                            cellPositionType,
                                                            _settingProvider);
                                                }),
                                            _settingProvider);
                                }),
                            _settingProvider)),
                    _settingProvider))
                .ToArray();
            _weatherList = new WeatherListViewModel[11];
            GUIText = GUITextTranslate.Instance;
            About = new AboutViewModel(_settingProvider);
            UpdatedDateTime = DateTime.UtcNow;
            FishChanceList = new FishChanceTimeRegions[0];
            OptionMenuCommand = null;
            ExitMenuCommand = null;
            About.AboutMenuCommand = null;
            _settingProvider = settingProvider;

            _settingProvider.MainWindowTabSelected += _settingProvider_MainWindowTabSelected;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            _settingProvider.ForecastWeatherDaysChanged += _settingProvider_ForecastWeatherDaysChanged;
            _settingProvider.FishMemoChanged += _settingProvider_FishMemoChanged;
            _settingProvider.FishFilterChanded += _settingProvider_FishFilterChanded;
            _isPendingFishFilterChandedEvent = false;

            CurrentTime = DateTime.UtcNow;
            CurrentDateTimeText = "";
        }

        public IEnumerable<FishSettingOfAreaGroupViewModel> FishSettingOfWorld { get; }
        public GUITextTranslate GUIText { get; }
        public WeatherListViewModel WeatherListOfLaNoscea => _weatherList[0];
        public WeatherListViewModel WeatherListOfTheBlackShroud => _weatherList[1];
        public WeatherListViewModel WeatherListOfThanalan => _weatherList[2];
        public WeatherListViewModel WeatherListOfCoerthas => _weatherList[3];
        public WeatherListViewModel WeatherListOfAbalathia => _weatherList[4];
        public WeatherListViewModel WeatherListOfDravania => _weatherList[5];
        public WeatherListViewModel WeatherListOfGyrAbania => _weatherList[6];
        public WeatherListViewModel WeatherListOfHingashi => _weatherList[7];
        public WeatherListViewModel WeatherListOfOthard => _weatherList[8];
        public WeatherListViewModel WeatherListOfNorvrandt => _weatherList[9];
        public WeatherListViewModel WeatherListOfMorDhona => _weatherList[10];
        public IEnumerable<FishChanceTimeRegions> FishChanceList { get; private set; }
        public IEnumerable<EorzeaDateTime> FishChanceTimeList { get; private set; }
        public DateTime UpdatedDateTime { get; private set; }

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
                        UpdateFishChanceList(_currentTime);
                    }
                    else
                    {
                        var previousEorzeaTime = previousTime.ToEorzeaDateTime();
                        var currentEorzeaTime = _currentTime.ToEorzeaDateTime();
                        if (previousEorzeaTime.EpochHours / 8 != currentEorzeaTime.EpochHours / 8)
                        {
                            UpdateWeatherList(_currentTime);
                            UpdateFishChanceList(_currentTime);
                        }
                    }
                    UpdateCurrentDateTimeText(_currentTime);
                    RaisePropertyChangedEvent(nameof(CurrentTime));
                }
            }
        }

        public string CurrentDateTimeText { get; private set; }
        public AboutViewModel About { get; set; }
        public ICommand OptionMenuCommand { get; set; }
        public ICommand ExitMenuCommand { get; set; }

        public bool IsSelectedForecastWeatherTab
        {
            get => _settingProvider.GetIsSelectedMainWindowTab(MainWindowTabType.ForecastWeather);
            set => _settingProvider.SetIsSelectedMainWindowTab(MainWindowTabType.ForecastWeather, value);
        }

        public bool IsSelectedChanceTab
        {
            get => _settingProvider.GetIsSelectedMainWindowTab(MainWindowTabType.Chance);
            set => _settingProvider.SetIsSelectedMainWindowTab(MainWindowTabType.Chance, value);
        }

        public int ForecastWeatherDays
        {
            get => _settingProvider.ForecastWeatherDays;
            set => _settingProvider.ForecastWeatherDays = value;
        }

        public string UserLanguage
        {
            get => _settingProvider.UserLanguage;
            set => _settingProvider.UserLanguage = value;
        }

        public void SetFishFilter(Fish fish, bool isEnabled)
        {
            _settingProvider.SetIsEnabledFishFilter(fish, isEnabled);
        }

        public string GetFishMemo(Fish fish)
        {
            return _settingProvider.GetFishMemo(fish);
        }

        public void SetFishMemo(Fish fish, string memo)
        {
            _settingProvider.SetFishMemo(fish, memo);
        }

        public event EventHandler<Fish> FishMemoChanged;

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var item in FishSettingOfWorld)
                    item?.Dispose();
                About.Dispose();
                _settingProvider.MainWindowTabSelected -= _settingProvider_MainWindowTabSelected;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _settingProvider.ForecastWeatherDaysChanged -= _settingProvider_ForecastWeatherDaysChanged;
                _settingProvider.FishMemoChanged -= _settingProvider_FishMemoChanged;
                _settingProvider.FishFilterChanded -= _settingProvider_FishFilterChanded;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_MainWindowTabSelected(object sender, MainWindowTabType e)
        {
            switch (e)
            {
                case MainWindowTabType.ForecastWeather:
                    RaisePropertyChangedEvent(nameof(IsSelectedForecastWeatherTab));
                    break;
                case MainWindowTabType.Chance:
                    RaisePropertyChangedEvent(nameof(IsSelectedChanceTab));
                    break;
                default:
                    break;
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(UserLanguage));
            RaisePropertyChangedEvent(nameof(FishChanceList));
            RaisePropertyChangedEvent(nameof(GUIText));
        }

        private void _settingProvider_ForecastWeatherDaysChanged(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            UpdateWeatherList(now);
            UpdateFishChanceList(now);
            RaisePropertyChangedEvent(nameof(ForecastWeatherDays));
            RaisePropertyChangedEvent(nameof(WeatherListOfLaNoscea));
            RaisePropertyChangedEvent(nameof(WeatherListOfTheBlackShroud));
            RaisePropertyChangedEvent(nameof(WeatherListOfThanalan));
            RaisePropertyChangedEvent(nameof(WeatherListOfCoerthas));
            RaisePropertyChangedEvent(nameof(WeatherListOfAbalathia));
            RaisePropertyChangedEvent(nameof(WeatherListOfDravania));
            RaisePropertyChangedEvent(nameof(WeatherListOfGyrAbania));
            RaisePropertyChangedEvent(nameof(WeatherListOfHingashi));
            RaisePropertyChangedEvent(nameof(WeatherListOfOthard));
            RaisePropertyChangedEvent(nameof(WeatherListOfNorvrandt));
            RaisePropertyChangedEvent(nameof(WeatherListOfMorDhona));
            RaisePropertyChangedEvent(nameof(FishChanceTimeList));
            RaisePropertyChangedEvent(nameof(FishChanceList));
        }

        private void _settingProvider_FishMemoChanged(object sender, Fish e)
        {
            var now = DateTime.UtcNow;
            UpdateFishChanceList(now);
            RaisePropertyChangedEvent(nameof(FishChanceList));
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
                    UpdateFishChanceList(now);
                    _isPendingFishFilterChandedEvent = false;
                }
            });
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
            foreach (var item in _weatherList)
                item?.Dispose();
            _weatherList =
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
                                    Background = (Brush)_brushConverter.ConvertFromString( item.index == 0 ? "#ffd700" : "#eeeeee"),
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
            RaisePropertyChangedEvent(nameof(WeatherListOfLaNoscea));
            RaisePropertyChangedEvent(nameof(WeatherListOfTheBlackShroud));
            RaisePropertyChangedEvent(nameof(WeatherListOfThanalan));
            RaisePropertyChangedEvent(nameof(WeatherListOfCoerthas));
            RaisePropertyChangedEvent(nameof(WeatherListOfAbalathia));
            RaisePropertyChangedEvent(nameof(WeatherListOfDravania));
            RaisePropertyChangedEvent(nameof(WeatherListOfGyrAbania));
            RaisePropertyChangedEvent(nameof(WeatherListOfHingashi));
            RaisePropertyChangedEvent(nameof(WeatherListOfOthard));
            RaisePropertyChangedEvent(nameof(WeatherListOfNorvrandt));
            RaisePropertyChangedEvent(nameof(WeatherListOfMorDhona));
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

        private void UpdateFishChanceList(DateTime now)
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
                .Select(fish => fish.GetFishingChance(wholePeriod, eorzeaNow))
                .Where(result => result != null && !result.Regions.Intersect(forecastPeriod).IsEmpty)
                .ToArray();
            var comparer = new FishChanceTimeRegionsComparer();
            FishChanceTimeList =
                Enumerable.Range(0, 24 * _settingProvider.ForecastWeatherDays + 8)
                .Select(index => eorzeaNow + EorzeaTimeSpan.FromHours(index - 8))
                .ToArray();
            FishChanceList =
                FishChanceTimeList
                .Select(time => founds
                    .Where(result => result.Regions.Contains(time))
                    .OrderByDescending(result => result.Fish.DifficultyValue)
                    .ThenBy(result => result.FishingCondition.FishingSpot.Area.AreaGroup.Order)
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
            RaisePropertyChangedEvent(nameof(FishChanceTimeList));
            RaisePropertyChangedEvent(nameof(FishChanceList));
        }
    }
}
