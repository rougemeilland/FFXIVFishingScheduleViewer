using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FishingScheduler
{
    class DataContext
        : ViewModel
    {
        private class FishChanceTimeRegionsComparer
            : IEqualityComparer<FishChanceTimeRegions>
        {
            public bool Equals(FishChanceTimeRegions x, FishChanceTimeRegions y)
            {
                return x.Fish.Name.Equals(y.Fish.Name);
            }

            public int GetHashCode(FishChanceTimeRegions obj)
            {
                return obj.Fish.Name.GetHashCode();
            }
        }

        private class CommandImp
            : ICommand
        {
            private Action<object> _action;

            public CommandImp(Action<object> action)
            {
                _action = action;
            }

#pragma warning disable 0067
            public event EventHandler CanExecuteChanged;
#pragma warning disable 0067

            public bool CanExecute(object parameter)
            {
                return _action != null;
            }

            public void Execute(object parameter)
            {
                _action(parameter);
            }
        }


        private WeatherListViewModel[] _weatherList;
        private ISettingProvider _settingProvider;

        public DataContext(ISettingProvider settingProvider)
        {
            _weatherList = new WeatherListViewModel[11];
            UpdatedDateTime = DateTime.MinValue;
            FishChanceList = new FishChanceTimeRegions[0];
            OptionCommand = new CommandImp(p => OnOptionCommand(this, p));
            ExitCommand = new CommandImp(p => OnExitCommand(this, p));
            IsModifiedFishChanceList = false;
            _settingProvider = settingProvider;
        }

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
        public ICommand OptionCommand { get; }
        public ICommand ExitCommand { get; }
        public event EventHandler<object> OnOptionCommand;
        public event EventHandler<object> OnExitCommand;

        public bool IsSelectedForecastWeatherTab
        {
            get
            {
                return _settingProvider.GetIsSelectedMainWindowTab(MainWindowTabType.ForecastWeather);
            }

            set
            {
                if (_settingProvider.SetIsSelectedMainWindowTab(MainWindowTabType.ForecastWeather, value))
                    RaisePropertyChangedEvent(nameof(IsSelectedForecastWeatherTab));
            }
        }

        public bool IsSelectedChanceTab
        {
            get
            {
                return _settingProvider.GetIsSelectedMainWindowTab(MainWindowTabType.Chance);
            }

            set
            {
                if (_settingProvider.SetIsSelectedMainWindowTab(MainWindowTabType.Chance, value))
                    RaisePropertyChangedEvent(nameof(IsSelectedChanceTab));
            }
        }

        public bool IsModifiedFishChanceList { get; set; }

        public void UpdateWeatherList(KeyValueCollection<string, AreaGroup> areaGroups, KeyValueCollection<string, Fish> fishes)
        {
            UpdatedDateTime = DateTime.UtcNow;
            var eorzeaNow = UpdatedDateTime.ToEorzeaDateTime();
            eorzeaNow -= EorzeaTimeSpan.FromSeconds(eorzeaNow.EpochSeconds % (60 * 60 * 8));
            var timeList = Enumerable.Range(-1, 23)
                                .Select(index =>
                                {
                                    var startTime = eorzeaNow + EorzeaTimeSpan.FromHours(index * 8);
                                    var localStartTime = startTime.ToEarthDateTime().ToLocalTime();
                                    string headerText =
                                        string.Format(
                                            "{2:D02}:{3:D02}:{4:D02}\n(ET{0:D02}:{1:D02})",
                                            startTime.Hour,
                                            startTime.Minute,
                                            localStartTime.Hour,
                                            localStartTime.Minute,
                                            localStartTime.Second);
                                    return new { index, headerText, startTime };
                                })
                                .ToArray();
            var columnHeaders =
            _weatherList = areaGroups
                .Select(areaGroup =>
                    new WeatherListViewModel(
                        areaGroup.AreaGroupName,
                        timeList.Select(item => item.headerText),
                        areaGroup.Areas
                            .Select(area => new WeatherOfAreaViewModel(
                                area.AreaName,
                                timeList.Select(item =>
                                {
                                    var weather = area.ForecastWeather(item.startTime);
                                    var weatherName = weather.GetText();
                                    string foreground = "black";
                                    string background = "white";
                                    switch (item.index)
                                    {
                                        case -1:
                                            foreground = "dimgray";
                                            background = "lightgray";
                                            break;
                                        case 0:
                                            background = "yellowgreen";
                                            break;
                                        default:
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
                                            break;
                                    }
                                    var converter = new System.Windows.Media.BrushConverter();
                                    return
                                        new WeatherViewModel(
                                            weatherName,
                                            (System.Windows.Media.Brush)converter.ConvertFromString(foreground),
                                            (System.Windows.Media.Brush)converter.ConvertFromString(background));
                                }))),
                        _settingProvider
                    )
                )
                .ToArray();
            UpdateFishChanceList(areaGroups, fishes, eorzeaNow);

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

        public void UpdateFishChanceList(KeyValueCollection<string, AreaGroup> areaGroups, KeyValueCollection<string, Fish> fishes, EorzeaDateTime eorzeaNow)
        {
            // 開始時刻の8時間前から、開始時刻の[Properties.Settings.Default.DaysOfForecast]日後まで
            var wholePeriod = new EorzeaDateTimeHourRegions(new[]
            {
                new EorzeaDateTimeRegion(
                    eorzeaNow - EorzeaTimeSpan.FromHours(8),
                    eorzeaNow + EorzeaTimeSpan.FromDays(Properties.Settings.Default.DaysOfForecast)
                )
            });
            var worecastPeriod = new EorzeaDateTimeHourRegions(new[]
            {
                new EorzeaDateTimeRegion(
                    eorzeaNow,
                    eorzeaNow + EorzeaTimeSpan.FromDays(Properties.Settings.Default.DaysOfForecast)
                )
            });
            var founds =
                fishes
                .Where(fish => _settingProvider.GetIsEnabledFishFilter(fish.Name))
                .OrderByDescending(fish => fish.DifficultyValue)
                .Select(fish => fish.GetFishingChance(areaGroups, wholePeriod, eorzeaNow))
                .Where(result => result != null && !result.Regions.Intersect(worecastPeriod).IsEmpty)
                .ToArray();
            var comparer = new FishChanceTimeRegionsComparer();
            FishChanceTimeList =
                Enumerable.Range(0, 24 * Properties.Settings.Default.DaysOfForecast + 8)
                .Select(index => eorzeaNow + EorzeaTimeSpan.FromHours(index - 8))
                .ToArray();
            FishChanceList =
                FishChanceTimeList
                .Select(time => founds
                    .Where(result => result.Regions.Contains(time))
                    .OrderByDescending(result => result.Fish.DifficultyValue)
                    .ThenBy(result => result.FishingCondition.FishingGround.Area.AreaGroup.Order)
                    .ThenBy(result => result.FishingCondition.FishingGround.Area.Order)
                    .ThenBy(result => result.FishingCondition.FishingGround.Order)
                    .FirstOrDefault())
                .Where(result => result != null)
                .Distinct(comparer)
                .OrderBy(result => result.Regions.Begin)
                .ThenBy(result => result.Regions.End)
                .ToArray();
        }
    }
}
