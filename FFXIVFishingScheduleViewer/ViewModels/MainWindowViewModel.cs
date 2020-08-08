using FFXIVFishingScheduleViewer.Models;
using FFXIVFishingScheduleViewer.Strings;
using FFXIVFishingScheduleViewer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class MainWindowViewModel
        : WindowViewModel
    {
        private bool _isDisposed;
        private Dispatcher _dispatcher;
        private GameData _gameData;
        private DateTime _currentTime;
        private BrushConverter _brushConverter;

        public MainWindowViewModel(Window ownerWindow, string[] args)
        {
            _isDisposed = false;
            _dispatcher = ownerWindow.Dispatcher;
            _gameData = new GameData(args);
            _currentTime = DateTime.MinValue;
            _brushConverter = new BrushConverter();
            WeatherListViewModels = new WeatherListViewModel[0];
            UpdatedDateTime = DateTime.UtcNow;
            FishingChanceListViewModel = new FishingChanceListViewModel(_gameData, _dispatcher);

            ShowDownloadPageCommand = null;
            OptionMenuCommand = null;
            ExitMenuCommand = null;
            ViewREADMEMenuCommand = null;
            AboutMenuCommand = null;

            _gameData.NewVersionOfApplicationChanged += _settingProvider_NewVersionOfApplicationChanged;
            _gameData.SettingProvider.SelectedMainViewTabIndexChanged += _settingProvider_SelectedMainViewTabIndexChanged;
            _gameData.SettingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            _gameData.SettingProvider.ForecastWeatherDaysChanged += _settingProvider_ForecastWeatherDaysChanged;
            _gameData.SettingProvider.FishingChanceListTextEffectChanged += _settingProvider_FishingChanceListTextEffectChanged;

            CurrentTime = DateTime.UtcNow;
            CurrentDateTimeText = "";
            NewVersionReleasedText = null;

            _gameData.CheckNewVersionReleased();


            ShowDownloadPageCommand = new SimpleCommand(p =>
            {
                System.Diagnostics.Process.Start(UrlOfDownloadPage);
            });
            OptionMenuCommand = new SimpleCommand(p1 =>
            {
                using (var viewModel = CreateOptionViewModel())
                {
                    var dialog = new OptionWindow
                    {
                        Owner = ownerWindow,
                        TypedViewModel = viewModel,
                    };
                    dialog.TypedViewModel.CloseWindowCommand = new SimpleCommand(p2 =>
                    {
                        dialog.DialogResult = true;
                        dialog.Close();
                    });
                    dialog.ShowDialog();
                }
            });
            ExitMenuCommand = new SimpleCommand(p => ownerWindow.Close());
            ViewREADMEMenuCommand = new SimpleCommand(p =>
            {
                System.Diagnostics.Process.Start(Translate.Instance[new TranslationTextId(TranslationCategory.Url, "README")]);
            });
            AboutMenuCommand = new SimpleCommand(p =>
            {
                using (var viewModel = CreateAboutViewModel())
                {
                    var dialog = new AboutWindow
                    {
                        Owner = ownerWindow,
                        ViewModel = viewModel,
                    };
                    dialog.ShowDialog();
                }
            });

            MinWidth = 640;
            MinHeight = 480;

            CurrentTime = DateTime.UtcNow;
        }

        public override string WindowTitleText => GUIText["Title.MainWindow"];
        public string CurrentDateTimeText { get; private set; }
        public string NewVersionReleasedText { get; private set; }
        public string AboutMenuText => string.Format(GUITextTranslate.Instance["Menu.About"], _gameData.ProductName);

        public int SelectedMainViewTabIndex
        {
            get => _gameData.SettingProvider.SelectedMainViewTabIndex;
            set => _gameData.SettingProvider.SelectedMainViewTabIndex = value;
        }

        public FishingChanceListTextEffectType FishingChanceListTextEffect
        {
            get => _gameData.SettingProvider.FishingChanceListTextEffect;
        }

        public IEnumerable<WeatherListViewModel> WeatherListViewModels { get; private set; }
        public FishingChanceListViewModel FishingChanceListViewModel { get; }
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
                        FishingChanceListViewModel.CurrentTime = _currentTime;
                    }
                    else
                    {
                        var previousEorzeaTime = previousTime.ToEorzeaDateTime();
                        var currentEorzeaTime = _currentTime.ToEorzeaDateTime();
                        if (previousEorzeaTime.EpochHours / 8 != currentEorzeaTime.EpochHours / 8)
                        {
                            UpdateWeatherList(_currentTime);
                            FishingChanceListViewModel.CurrentTime = _currentTime;
                        }
                    }
                    UpdateCurrentDateTimeText(_currentTime);
                    RaisePropertyChangedEvent(nameof(CurrentTime));
                }
            }
        }

        public int ForecastWeatherDays => _gameData.SettingProvider.ForecastWeatherDays;
        public DateTime UpdatedDateTime { get; private set; }
        public string UrlOfDownloadPage => _gameData.UrlOfDownloadPage;

        public AboutWindowViewModel CreateAboutViewModel()
        {
            return new AboutWindowViewModel(_gameData);
        }

        public OptionWindowViewModel CreateOptionViewModel()
        {
            return new OptionWindowViewModel(_gameData);
        }

        public TimeSpan GetTimerInterval()
        {
            var nextTime = (CurrentTime.ToEorzeaDateTime().GetStartOfMinute() + EorzeaTimeSpan.FromMinutes(1)).ToEarthDateTime();
            var now = DateTime.UtcNow;
            while (nextTime - now <= TimeSpan.FromSeconds(1))
                nextTime = (nextTime.ToEorzeaDateTime() + EorzeaTimeSpan.FromMinutes(1)).ToEarthDateTime();
            return nextTime - now;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _gameData.SettingProvider.SelectedMainViewTabIndexChanged -= _settingProvider_SelectedMainViewTabIndexChanged;
                _gameData.SettingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _gameData.SettingProvider.ForecastWeatherDaysChanged -= _settingProvider_ForecastWeatherDaysChanged;
                _gameData.NewVersionOfApplicationChanged -= _settingProvider_NewVersionOfApplicationChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            UpdateCurrentDateTimeText(DateTime.UtcNow);
            UpdateNewVersionReleasedText();
            RaisePropertyChangedEvent(nameof(WindowTitleText));
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
            NewVersionReleasedText =
                _gameData.NewVersionOfApplication != null
                ? string.Format(
                    GUITextTranslate.Instance["Label.NewVersionReleased"],
                    _gameData.NewVersionOfApplication,
                    _gameData.CurrentVersionOfApplication)
                : null;
            RaisePropertyChangedEvent(nameof(NewVersionReleasedText));
        }

        private void UpdateWeatherList(DateTime now)
        {
            UpdatedDateTime = now;
            var eorzeaNow = UpdatedDateTime.ToEorzeaDateTime().GetStartOf8Hour();
            var timeList = Enumerable.Range(-1, _gameData.SettingProvider.ForecastWeatherDays * 3 + 2)
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
                                    var isLastTime = index == _gameData.SettingProvider.ForecastWeatherDays * 3;
                                    return new { index, headerText, startTime, isLastTime };
                                })
                                .ToArray();
            foreach (var item in WeatherListViewModels)
                item?.Dispose();
            WeatherListViewModels =
                _gameData.AreaGroups
                .Select(areaGroup =>
                {
                    var lastArea = areaGroup.Areas.Last();
                    return
                        new WeatherListViewModel(
                            areaGroup,
                            timeList.Select(item =>
                                new WeatherListCellViewModel(
                                    () => item.headerText,
                                    _gameData.SettingProvider)
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
                                        _gameData.SettingProvider);
                                }),
                            _gameData.SettingProvider
                        );
                })
                .ToArray();
            RaisePropertyChangedEvent(nameof(UpdatedDateTime));
            RaisePropertyChangedEvent(nameof(WeatherListViewModels));
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
                    _gameData.SettingProvider)
                {
                    Background = backgroundBrush,
                    Foreground = foregroundBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    CellPositionType = cellPositionType
                };
        }

        private void _settingProvider_SelectedMainViewTabIndexChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedMainViewTabIndex));
        }

        private void _settingProvider_ForecastWeatherDaysChanged(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            UpdateWeatherList(now);
            RaisePropertyChangedEvent(nameof(ForecastWeatherDays));
            RaisePropertyChangedEvent(nameof(WeatherListViewModels));
        }
    }
}
