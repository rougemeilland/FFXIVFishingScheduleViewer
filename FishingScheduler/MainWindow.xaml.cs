using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FishingScheduler
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : Window, ISettingProvider
    {
        private ISettingProvider _settingProvider;
        private DataContext _dataContext;
        private KeyValueCollection<string, AreaGroup> _areaGroups;
        private KeyValueCollection<string, Area> _areas;
        private KeyValueCollection<string, FishingGround> _fishGrounds;
        private KeyValueCollection<string, FishingBait> _fishingBates;
        private KeyValueCollection<string, Fish> _fishes;
        private Grid _currentTimeIndicatorGrid;
        private IDictionary<string, string> _filteredfishNames;
        private IDictionary<string, string> _expandedAreaGroupNames;
        private IDictionary<string, string> _selectedTabNames;
        private IDictionary<string, string> _fishMemoList;

        public MainWindow()
        {
            InitializeComponent();

            InitializeData();

            _settingProvider = this;
            _dataContext = new DataContext(_settingProvider);
            DataContext = _dataContext;
            _currentTimeIndicatorGrid = null;
            _filteredfishNames =
                Properties.Settings.Default.FilteredFishNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");
            _expandedAreaGroupNames =
                Properties.Settings.Default.ExpandedAreaGroupNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");
            _selectedTabNames =
                Properties.Settings.Default.SelectedTabNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");
            if (!_selectedTabNames.Any())
                _selectedTabNames[MainWindowTabType.ForecastWeather.ToString()] = "*";
            _fishMemoList =
                Properties.Settings.Default.FishMemoList
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                .Where(columns => columns.Length == 2)
                .Select(columns => new { name = columns[0].SimpleDecode(), text = columns[1].SimpleDecode()})
                .ToDictionary(item => item.name, item => item.text);
            _dataContext.OnOptionCommand += _dataContext_OnOptionCommand;
            _dataContext.OnExitCommand += _dataContext_OnExitCommand;
            Properties.Settings.Default.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Properties.Settings.Default.FilteredFishNames):
                        _dataContext.IsModifiedFishChanceList = true;
                        Task.Run(() =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (_dataContext.IsModifiedFishChanceList)
                                {
                                    var eorzeaTimeNow = DateTime.UtcNow.ToEorzeaDateTime();
                                    _dataContext.UpdateFishChanceList(_areaGroups, _fishes, eorzeaTimeNow);
                                    UpdateFishChanceListView();
                                    _dataContext.IsModifiedFishChanceList = false;
                                }
                            });
                        });
                        break;
                    default:
                        break;
                }
            };
            RecoverWindowBounds();
            UpdateDaemon();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowBounds();
            base.OnClosing(e);
        }

        private void RecoverWindowBounds()
        {
            var settings = Properties.Settings.Default;
            if (!double.IsNaN(settings.MainWindowLeft) && !double.IsNaN(settings.MainWindowTop) && !double.IsNaN(settings.MainWindowWidth) && !double.IsNaN(settings.MainWindowHeight))
            {
                if (settings.MainWindowLeft >= 0 &&
                    (settings.MainWindowLeft + settings.MainWindowWidth) < SystemParameters.VirtualScreenWidth)
                {
                    Left = settings.MainWindowLeft;
                }

                if (settings.MainWindowTop >= 0 &&
                    (settings.MainWindowTop + settings.MainWindowHeight) < SystemParameters.VirtualScreenHeight)
                {
                    Top = settings.MainWindowTop;
                }

                if (settings.MainWindowWidth > 0 &&
                    settings.MainWindowWidth <= SystemParameters.WorkArea.Width)
                {
                    Width = settings.MainWindowWidth;
                }

                if (settings.MainWindowHeight > 0 &&
                    settings.MainWindowHeight <= SystemParameters.WorkArea.Height)
                {
                    Height = settings.MainWindowHeight;
                }
            }
            if (settings.MainWindowMaximized)
            {
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }
        }

        private void SaveWindowBounds()
        {
            WindowState = WindowState.Normal; // 最大化解除
            var settings = Properties.Settings.Default;
            settings.MainWindowMaximized = WindowState == WindowState.Maximized;
            settings.MainWindowLeft = Left;
            settings.MainWindowTop = Top;
            settings.MainWindowWidth = Width;
            settings.MainWindowHeight = Height;
            settings.Save();
        }

        private void _dataContext_OnOptionCommand(object sender, object e)
        {
            var dialog = new OptionWindow(_areaGroups, _fishes, this);
            dialog.ShowDialog();
        }

        private void _dataContext_OnExitCommand(object sender, object e)
        {
            Close();
        }

        private async void UpdateDaemon()
        {
            while (true)
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateView();
                });
                var earthTimeNow = _dataContext.UpdatedDateTime;
                var eorzeaTimeNow = earthTimeNow.ToEorzeaDateTime();
                var nextEorzeaTime = EorzeaDateTime.From(eorzeaTimeNow.Year, eorzeaTimeNow.Month, eorzeaTimeNow.Day, eorzeaTimeNow.Hour / 8 * 8, 0, 0) + EorzeaTimeSpan.FromHours(8);
                var nextEarthTime = nextEorzeaTime.ToEarthDateTime();
                var delayTime = nextEarthTime - earthTimeNow;
                await Task.Delay(delayTime);
            }
        }

        private void UpdateView()
        {
            _dataContext.UpdateWeatherList(_areaGroups, _fishes);
            UpdateFishChanceListView();
            UpdateCurrentTimeIndicator();
        }

        private void UpdateFishChanceListView()
        {
            FishChanceGrid.Children.Clear();
            FishChanceGrid.ColumnDefinitions.Clear();
            FishChanceGrid.RowDefinitions.Clear();

            var converter = new BrushConverter();
            var borderColor = (Brush)converter.ConvertFromString("#222222");
            var headerCackgroundColor = (Brush)converter.ConvertFromString("#888888");
            var currentTimeIndicatorColor = (Brush)converter.ConvertFromString("springgreen");
            var transparentBrush = new SolidColorBrush(Colors.Transparent);
            FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            var dataColumnCount = Properties.Settings.Default.DaysOfForecast * 24 + 8;
            for (var count = dataColumnCount; count > 0; --count)
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Pixel) });
            FishChanceGrid.RowDefinitions.Add(new RowDefinition());
            FishChanceGrid.RowDefinitions.Add(new RowDefinition());
            {
                {
                    var c = new Border
                    {
                        Child = new TextBlock()
                        {
                            Text = "魚の名前",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.Bold,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(5),
                        },
                        BorderBrush = borderColor,
                        BorderThickness = new Thickness(2, 2, 1, 2),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Background = headerCackgroundColor,
                    };
                    Grid.SetColumn(c, 0);
                    Grid.SetRow(c, 0);
                    Grid.SetRowSpan(c, 2);
                    FishChanceGrid.Children.Add(c);
                }
                {
                    var c = new Border
                    {
                        Child = new TextBlock()
                        {
                            Text = "釣り場",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Left,
                            Margin = new Thickness(5),
                        },
                        BorderBrush = borderColor,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        BorderThickness = new Thickness(0, 2, 2, 1),
                        Background = headerCackgroundColor,
                    };
                    Grid.SetColumn(c, 1);
                    Grid.SetRow(c, 0);
                    FishChanceGrid.Children.Add(c);
                }
                {
                    var c = new Border
                    {
                        Child = new TextBlock()
                        {
                            Text = "用意する釣り餌",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Left,
                            Margin = new Thickness(5),
                        },
                        BorderBrush = borderColor,
                        BorderThickness = new Thickness(0, 0, 2, 2),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Background = headerCackgroundColor,
                    };
                    Grid.SetColumn(c, 1);
                    Grid.SetRow(c, 1);
                    FishChanceGrid.Children.Add(c);
                }
                {
                    // 時刻
                    var c = new Border
                    {
                        Child = new TextBlock()
                        {
                            Text = "エオルアゼア時刻",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Left,
                            Margin = new Thickness(5),
                        },
                        BorderBrush = borderColor,
                        BorderThickness = new Thickness(0, 2, 2, 1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Background = headerCackgroundColor,
                    };
                    Grid.SetColumn(c, 2);
                    Grid.SetColumnSpan(c, FishChanceGrid.ColumnDefinitions.Count - 2);
                    Grid.SetRow(c, 0);
                    FishChanceGrid.Children.Add(c);
                }
                var currentCulumnIndex = 2;
                foreach (var time in _dataContext.FishChanceTimeList)
                {
                    Brush backgroundColor = GetBackgroundColorOfTime(converter, time);
                    {
                        var c = new Border
                        {
                            BorderBrush = borderColor,
                            BorderThickness = new Thickness(0, 0, 0, 2),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = backgroundColor,
                        };
                        Grid.SetColumn(c, currentCulumnIndex);
                        Grid.SetRow(c, 1);
                        FishChanceGrid.Children.Add(c);
                    }
                    {
                        var c = new TextBlock()
                        {
                            Text = time.Hour % 4 == 0 ? time.Hour.ToString() : "",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center,
                            Background = transparentBrush,
                        };
                        Grid.SetColumn(c, currentCulumnIndex);
                        Grid.SetRow(c, 1);
                        FishChanceGrid.Children.Add(c);
                    }
                    currentCulumnIndex += 1;
                }
            }
            Action<string> removeFilterAction = (fishName) =>
            {
                _settingProvider.SetIsEnabledFishFilter(fishName, false);
            };
            var currentRowIndex = 2;
            foreach (var chance in _dataContext.FishChanceList)
            {
                var contextMenu = new ContextMenu();
                var removeFilterMenuItem = new MenuItem { Header = string.Format("Don't display '{0}'", chance.Fish.Name) };
                contextMenu.Items.Add(removeFilterMenuItem);
                removeFilterMenuItem.Click += (s, e) =>
                {
                    removeFilterAction(chance.Fish.Name);
                };
                var backgroundColorOfFish = chance.Fish.DifficultySymbol.GetBackgroundColor();
                FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                {
                    var stack = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5),
                    };
                    var c = new Border
                    {
                        Child = stack,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        BorderBrush = borderColor,
                        BorderThickness = new Thickness(2, 0, 1, 2),
                        Background = backgroundColorOfFish,
                        ContextMenu = contextMenu,
                    };
                    Grid.SetColumn(c, 0);
                    Grid.SetRow(c, currentRowIndex);
                    Grid.SetRowSpan(c, 2);
                    FishChanceGrid.Children.Add(c);
                    stack.Children.Add(new TextBlock()
                    {
                        Text = string.Format("{0}", chance.Fish.Name),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ToolTip = "魚の名前です。",
                    });
                    stack.Children.Add(new TextBlock()
                    {
                        Text = string.Format("遭遇頻度: {0}", chance.Fish.DifficultySymbol.GetText()),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ToolTip = "その魚を釣るための時刻・天候の条件がどれだけ起こりやすいか、の目安です。\n必ずしも釣り自体の難易度とは一致していません。",
                    });
                }
                {
                    var c = new Border
                    {
                        Child = new TextBlock()
                        {
                            Text = string.Format(
                                "{0} ({1})",
                                chance.FishingCondition.FishingGround.FishingGroundName,
                                chance.FishingCondition.FishingGround.Area.AreaName),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5),
                            ToolTip = "魚を釣るための釣り場です。",
                        },
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        BorderBrush = borderColor,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = backgroundColorOfFish,
                        ContextMenu = contextMenu,
                    };
                    Grid.SetColumn(c, 1);
                    Grid.SetRow(c, currentRowIndex);
                    FishChanceGrid.Children.Add(c);
                }
                {
                    var c = new Border
                    {
                        Child = new TextBlock()
                        {
                            Text = string.Join(", ", chance.Fish.FishingBaits.Select(fishingBait => fishingBait.Name)),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5),
                        },
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        BorderBrush = borderColor,
                        BorderThickness = new Thickness(0, 0, 1, 2),
                        Background = backgroundColorOfFish,
                        ContextMenu = contextMenu,
                        ToolTip = "魚を釣るために用意すべき釣り餌の例です。\n漁師の直感のための前提となる魚を釣るために複数の釣り餌が必要になる場合もあります。",
                    };
                    Grid.SetColumn(c, 1);
                    Grid.SetRow(c, currentRowIndex + 1);
                    FishChanceGrid.Children.Add(c);
                }
                var currentCulumnIndex = 2;
                foreach (var time in _dataContext.FishChanceTimeList)
                {
                    UIElement c;
                    if (chance.Regions.Contains(time))
                    {
                        c = new Border
                        {
                            BorderBrush = borderColor,
                            BorderThickness = new Thickness(0, 0, 0, 0),
                            Background = backgroundColorOfFish,
                        };
                    }
                    else
                    {
                        c = new Border
                        {
                            BorderBrush = borderColor,
                            BorderThickness = new Thickness(0, 0, 0, 0),
                            Background = GetBackgroundColorOfTime(converter, time),
                        };
                    }
                    Grid.SetColumn(c, currentCulumnIndex);
                    Grid.SetRow(c, currentRowIndex);
                    Grid.SetRowSpan(c, 2);
                    FishChanceGrid.Children.Add(c);
                    currentCulumnIndex += 1;
                }
                currentRowIndex += 2;
            }
            {
                _currentTimeIndicatorGrid = new Grid()
                {
                    Background = transparentBrush,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                _currentTimeIndicatorGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                _currentTimeIndicatorGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Pixel) });
                _currentTimeIndicatorGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                _currentTimeIndicatorGrid.RowDefinitions.Add(new RowDefinition());
                var c = new Border()
                {

                    Width = 5,
                    BorderBrush = currentTimeIndicatorColor,
                    BorderThickness = new Thickness(2, 2, 2, 2),
                    Background = currentTimeIndicatorColor,
                };
                Grid.SetColumn(c, 1);
                Grid.SetRow(c, 0);
                _currentTimeIndicatorGrid.Children.Add(c);
                Grid.SetColumn(_currentTimeIndicatorGrid, 2);
                Grid.SetRow(_currentTimeIndicatorGrid, 0);
                Grid.SetColumnSpan(_currentTimeIndicatorGrid, FishChanceGrid.ColumnDefinitions.Count - 2);
                Grid.SetRowSpan(_currentTimeIndicatorGrid, FishChanceGrid.RowDefinitions.Count);
                FishChanceGrid.Children.Add(_currentTimeIndicatorGrid);
            }
            currentRowIndex = 2;
            foreach (var chance in _dataContext.FishChanceList)
            {
                var contextMenu = new ContextMenu();
                var removeFilterMenuItem = new MenuItem { Header = string.Format("Don't display '{0}'", chance.Fish.Name) };
                contextMenu.Items.Add(removeFilterMenuItem);
                removeFilterMenuItem.Click += (s, e) =>
                {
                    removeFilterAction(chance.Fish.Name);
                };
                var now = _dataContext.FishChanceTimeList.Skip(8).First();
                var forecastWeatherRegion = new EorzeaDateTimeRegion(now, EorzeaTimeSpan.FromDays(Properties.Settings.Default.DaysOfForecast));
                var firstRegionOfChance =
                    chance.Regions
                        .Intersect(new EorzeaDateTimeHourRegions(new[] { forecastWeatherRegion }))
                        .DateTimeRegions
                        .First();
                var conditionText = string.Join(", ", chance.FishingCondition.Desctriptions);
                var c = new Border
                {
                    Child = new TextBlock
                    {
                        Text =
                            string.Format(
                                "ET {0} ( LT {1} )\n条件: [{2}]{3}",
                                firstRegionOfChance.FormatEorzeaTimeRegion(forecastWeatherRegion),
                                firstRegionOfChance.FormatLocalTimeRegion(forecastWeatherRegion),
                                string.IsNullOrEmpty(conditionText) ? "なし" : conditionText,
                                string.IsNullOrEmpty(chance.Fish.Memo) ? "" :
                                    string.Format("\n【メモ】\n{0}",
                                        string.Join("\n",
                                            chance.Fish.Memo.Split(
                                                "\n".ToCharArray(),
                                                StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => string.Format("・{0}", s))))),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = transparentBrush,
                        Margin = new Thickness(5),
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    BorderBrush = borderColor,
                    BorderThickness = new Thickness(0, 0, 2, 2),
                    Background = transparentBrush,
                    ContextMenu = contextMenu,
                };
                Grid.SetColumn(c, 2);
                Grid.SetRow(c, currentRowIndex);
                Grid.SetColumnSpan(c, dataColumnCount);
                Grid.SetRowSpan(c, 2);
                FishChanceGrid.Children.Add(c);
                currentRowIndex += 2;
            }
            currentRowIndex = 2;
            foreach (var chance in _dataContext.FishChanceList)
            {
                var contextMenu = new ContextMenu();
                var removeFilterMenuItem = new MenuItem { Header = string.Format("Don't display '{0}'", chance.Fish.Name) };
                contextMenu.Items.Add(removeFilterMenuItem);
                removeFilterMenuItem.Click += (s, e) =>
                {
                    removeFilterAction(chance.Fish.Name);
                };
                var backgroundColorOfFish = chance.Fish.DifficultySymbol.GetBackgroundColor();
                var wholeRegion =
                    new EorzeaDateTimeRegion(
                        _dataContext.FishChanceTimeList.First(),
                        EorzeaTimeSpan.FromDays(Properties.Settings.Default.DaysOfForecast));
                foreach (var region in chance.Regions.DateTimeRegions)
                {
                    var startColumnIndex = (int)(region.Begin - wholeRegion.Begin).EorzeaTimeHours + 2;
                    var columnSpan = (int)region.Span.EorzeaTimeHours;
                    var toolTipText =
                        string.Format(
                            "{0}\nET {1}\nLT {2}",
                            chance.Fish.Name,
                            region.FormatEorzeaTimeRegion(wholeRegion),
                            region.FormatLocalTimeRegion(wholeRegion));
                    var c = new Border
                    {
                        ToolTip = toolTipText,
                        BorderBrush = transparentBrush,
                        BorderThickness = new Thickness(0, 0, 0, 0),
                        Background = transparentBrush,
                        ContextMenu = contextMenu,
                    };
                    Grid.SetColumn(c, startColumnIndex);
                    Grid.SetColumnSpan(c, columnSpan);
                    Grid.SetRow(c, currentRowIndex);
                    Grid.SetRowSpan(c, 2);
                    FishChanceGrid.Children.Add(c);
                }
                currentRowIndex += 2;
            }
        }

        private async void UpdateCurrentTimeIndicator()
        {
            while (true)
            {
                var earthTimeNow = DateTime.UtcNow;
                var localTimeNow = earthTimeNow.ToLocalTime();
                var eorzeaTimeNow = earthTimeNow.ToEorzeaDateTime();
                var leftWeight = (eorzeaTimeNow - _dataContext.FishChanceTimeList.First()).EorzeaTimeSeconds;
                var rightWeight = (_dataContext.FishChanceTimeList.Last() + EorzeaTimeSpan.FromHours(1) - eorzeaTimeNow).EorzeaTimeSeconds;
                var nextEorzeatime = eorzeaTimeNow + EorzeaTimeSpan.FromSeconds(60 - eorzeaTimeNow.Second);
                await Dispatcher.InvokeAsync(() =>
                {
                    _currentTimeIndicatorGrid.ColumnDefinitions[0].Width = new GridLength(leftWeight, GridUnitType.Star);
                    _currentTimeIndicatorGrid.ColumnDefinitions[2].Width = new GridLength(rightWeight, GridUnitType.Star);
                    CurrentEorzeaDateTime.Text = string.Format("{0:D02}:{1:D02}", eorzeaTimeNow.Hour, eorzeaTimeNow.Minute);
                    CurrentEarthDateTime.Text = string.Format("{0:D02}:{1:D02}:{2:D02}", localTimeNow.Hour, localTimeNow.Minute, localTimeNow.Second);
                });
                var interval = nextEorzeatime.ToEarthDateTime() - earthTimeNow;
                await Task.Delay(interval);
            }
        }

        private static Brush GetBackgroundColorOfTime(BrushConverter converter, EorzeaDateTime time)
        {
            string backgroundColorName;
            switch (time.Hour)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 20:
                case 21:
                case 22:
                case 23:
                    backgroundColorName = "cornflowerblue";
                    break;
                case 4:
                case 19:
                    backgroundColorName = "#969fe5";
                    break;
                case 5:
                case 18:
                    backgroundColorName = "#c9aade";
                    break;
                case 6:
                case 17:
                    backgroundColorName = "mistyrose";
                    break;
                case 7:
                case 16:
                    backgroundColorName = "seashell";
                    break;
                case 8:
                case 15:
                    backgroundColorName = "lemonchiffon";
                    break;
                case 9:
                case 14:
                    backgroundColorName = "lightyellow";
                    break;
                case 10:
                case 13:
                    backgroundColorName = "#ffffe8";
                    break;
                case 11:
                case 12:
                    backgroundColorName = "ivory";
                    break;
                default:
                    backgroundColorName = "white";
                    break;
            }
            var backgroundColor = (Brush)converter.ConvertFromString(backgroundColorName);
            return backgroundColor;
        }

        private void InitializeData()
        {
            _areaGroups = new KeyValueCollection<string, AreaGroup>();
            _areas = new KeyValueCollection<string, Area>();
            {
                var areaGroup = new AreaGroup("ラノシア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "リムサ・ロミンサ：上甲板層", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "リムサ・ロミンサ：上甲板層"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "リムサ・ロミンサ：下甲板層", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "リムサ・ロミンサ：下甲板層"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "中央ラノシア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ゼファードリフト沿岸"));
                    area.AddFishingGroup(new FishingGround(area, "ローグ川"));
                    area.AddFishingGroup(new FishingGround(area, "西アジェレス川"));
                    area.AddFishingGroup(new FishingGround(area, "サマーフォード沿岸"));
                    area.AddFishingGroup(new FishingGround(area, "ニーム川"));
                    area.AddFishingGroup(new FishingGround(area, "ささやきの谷"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "低地ラノシア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "モーニングウィドー"));
                    area.AddFishingGroup(new FishingGround(area, "モラビー湾西岸"));
                    area.AddFishingGroup(new FishingGround(area, "シダーウッド沿岸部"));
                    area.AddFishingGroup(new FishingGround(area, "オシュオン灯台"));
                    area.AddFishingGroup(new FishingGround(area, "キャンドルキープ埠頭"));
                    area.AddFishingGroup(new FishingGround(area, "モラビー造船廠"));
                    area.AddFishingGroup(new FishingGround(area, "エンプティハート"));
                    area.AddFishingGroup(new FishingGround(area, "ソルトストランド"));
                    area.AddFishingGroup(new FishingGround(area, "ブラインドアイアン坑道"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.快晴, 45), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.暴雨, 5), };
                    var area = new Area(areaGroup, "東ラノシア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "南ブラッドショア"));
                    area.AddFishingGroup(new FishingGround(area, "コスタ・デル・ソル"));
                    area.AddFishingGroup(new FishingGround(area, "北ブラッドショア"));
                    area.AddFishingGroup(new FishingGround(area, "ロータノ海沖合：船首"));
                    area.AddFishingGroup(new FishingGround(area, "ロータノ海沖合：船尾"));
                    area.AddFishingGroup(new FishingGround(area, "隠れ滝"));
                    area.AddFishingGroup(new FishingGround(area, "東アジェレス川"));
                    area.AddFishingGroup(new FishingGround(area, "レインキャッチャー樹林"));
                    area.AddFishingGroup(new FishingGround(area, "レインキャッチャー沼沢地"));
                    area.AddFishingGroup(new FishingGround(area, "レッドマンティス滝"));
                    area.AddFishingGroup(new FishingGround(area, "常夏の島北"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.暴風, 10), };
                    var area = new Area(areaGroup, "西ラノシア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "スウィフトパーチ入植地"));
                    area.AddFishingGroup(new FishingGround(area, "スカルバレー沿岸部"));
                    area.AddFishingGroup(new FishingGround(area, "ブルワーズ灯台"));
                    area.AddFishingGroup(new FishingGround(area, "ハーフストーン沿岸部"));
                    area.AddFishingGroup(new FishingGround(area, "幻影諸島北岸"));
                    area.AddFishingGroup(new FishingGround(area, "船の墓場"));
                    area.AddFishingGroup(new FishingGround(area, "サプサ産卵地"));
                    area.AddFishingGroup(new FishingGround(area, "幻影諸島南岸"));
                    area.AddFishingGroup(new FishingGround(area, "船隠しの港"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 10), };
                    var area = new Area(areaGroup, "高地ラノシア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "オークウッド"));
                    area.AddFishingGroup(new FishingGround(area, "愚か者の滝"));
                    area.AddFishingGroup(new FishingGround(area, "ブロンズレイク・シャロー"));
                    area.AddFishingGroup(new FishingGround(area, "ブロンズレイク北東岸"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 15), new WeatherPercentageOfArea(WeatherType.雨, 15), };
                    var area = new Area(areaGroup, "外地ラノシア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ロングクライム渓谷"));
                    area.AddFishingGroup(new FishingGround(area, "ブロンズレイク北西岸"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), };
                    var area = new Area(areaGroup, "ミスト・ヴィレッジ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ミスト・ヴィレッジ"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("黒衣森");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "グリダニア：新市街", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "グリダニア：翡翠湖畔"));
                    area.AddFishingGroup(new FishingGround(area, "グリダニア：紅茶川水系下流"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "グリダニア：旧市街", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "グリダニア：囁きの渓谷"));
                    area.AddFishingGroup(new FishingGround(area, "グリダニア：紅茶川水系上流"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雷, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "黒衣森：中央森林", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "葉脈水系"));
                    area.AddFishingGroup(new FishingGround(area, "鏡池"));
                    area.AddFishingGroup(new FishingGround(area, "エバーシェイド"));
                    area.AddFishingGroup(new FishingGround(area, "芽吹の池"));
                    area.AddFishingGroup(new FishingGround(area, "ハウケタ御用邸"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雷, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "黒衣森：東部森林", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "花蜜桟橋"));
                    area.AddFishingGroup(new FishingGround(area, "さざなみ小川"));
                    area.AddFishingGroup(new FishingGround(area, "十二神大聖堂"));
                    area.AddFishingGroup(new FishingGround(area, "青翠の奈落"));
                    area.AddFishingGroup(new FishingGround(area, "シルフランド渓谷"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.雷雨, 5), new WeatherPercentageOfArea(WeatherType.雷, 15), new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.快晴, 30), };
                    var area = new Area(areaGroup, "黒衣森：南部森林", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ハズーバ支流：上流"));
                    area.AddFishingGroup(new FishingGround(area, "ハズーバ支流：下流"));
                    area.AddFishingGroup(new FishingGround(area, "ハズーバ支流：東"));
                    area.AddFishingGroup(new FishingGround(area, "ハズーバ支流：中流"));
                    area.AddFishingGroup(new FishingGround(area, "ゴブリン族の生簀"));
                    area.AddFishingGroup(new FishingGround(area, "根渡り沼"));
                    area.AddFishingGroup(new FishingGround(area, "ウルズの恵み"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.暴雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 5), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.快晴, 30), };
                    var area = new Area(areaGroup, "黒衣森：北部森林", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "さざめき川"));
                    area.AddFishingGroup(new FishingGround(area, "フォールゴウド秋瓜湖畔"));
                    area.AddFishingGroup(new FishingGround(area, "プラウドクリーク"));
                    area.AddFishingGroup(new FishingGround(area, "タホトトル湖畔"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 5), new WeatherPercentageOfArea(WeatherType.雨, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 15), };
                    var area = new Area(areaGroup, "ラベンダーベッド", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ラベンダーベッド"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ザナラーン");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "ウルダハ：ナル回廊", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "ウルダハ：ザル回廊", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "西ザナラーン", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ノフィカの井戸"));
                    area.AddFishingGroup(new FishingGround(area, "足跡の谷"));
                    area.AddFishingGroup(new FishingGround(area, "ベスパーベイ"));
                    area.AddFishingGroup(new FishingGround(area, "クレセントコーヴ"));
                    area.AddFishingGroup(new FishingGround(area, "シルバーバザー"));
                    area.AddFishingGroup(new FishingGround(area, "ウエストウインド岬"));
                    area.AddFishingGroup(new FishingGround(area, "ムーンドリップ洞窟"));
                    area.AddFishingGroup(new FishingGround(area, "パラタの墓所"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.砂塵, 15), new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "中央ザナラーン", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "スートクリーク上流"));
                    area.AddFishingGroup(new FishingGround(area, "スートクリーク下流"));
                    area.AddFishingGroup(new FishingGround(area, "クラッチ狭間"));
                    area.AddFishingGroup(new FishingGround(area, "アンホーリーエアー"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), new WeatherPercentageOfArea(WeatherType.暴雨, 15), };
                    var area = new Area(areaGroup, "東ザナラーン", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ドライボーン北湧水地"));
                    area.AddFishingGroup(new FishingGround(area, "ドライボーン南湧水地"));
                    area.AddFishingGroup(new FishingGround(area, "ユグラム川"));
                    area.AddFishingGroup(new FishingGround(area, "バーニングウォール"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.灼熱波, 20), new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "南ザナラーン", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "リザードクリーク"));
                    area.AddFishingGroup(new FishingGround(area, "ザハラクの湧水"));
                    area.AddFishingGroup(new FishingGround(area, "忘れられたオアシス"));
                    area.AddFishingGroup(new FishingGround(area, "サゴリー砂海"));
                    area.AddFishingGroup(new FishingGround(area, "サゴリー砂丘"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.晴れ, 15), new WeatherPercentageOfArea(WeatherType.曇り, 30), new WeatherPercentageOfArea(WeatherType.霧, 50), };
                    var area = new Area(areaGroup, "北ザナラーン", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ブルーフォグ湧水地"));
                    area.AddFishingGroup(new FishingGround(area, "青燐泉"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 20), new WeatherPercentageOfArea(WeatherType.曇り, 25), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 5), };
                    var area = new Area(areaGroup, "ゴブレットビュート", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ゴブレットビュート"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("クルザス");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雪, 60), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "イシュガルド：上層", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雪, 60), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "イシュガルド：下層", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.吹雪, 20), new WeatherPercentageOfArea(WeatherType.雪, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "クルザス中央高地", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "クルザス川"));
                    area.AddFishingGroup(new FishingGround(area, "聖ダナフェンの旅程"));
                    area.AddFishingGroup(new FishingGround(area, "剣ヶ峰山麓"));
                    area.AddFishingGroup(new FishingGround(area, "キャンプ・ドラゴンヘッド溜池"));
                    area.AddFishingGroup(new FishingGround(area, "調査隊の氷穴"));
                    area.AddFishingGroup(new FishingGround(area, "聖ダナフェンの落涙"));
                    area.AddFishingGroup(new FishingGround(area, "スノークローク大氷壁"));
                    area.AddFishingGroup(new FishingGround(area, "イシュガルド大雲海"));
                    area.AddFishingGroup(new FishingGround(area, "ウィッチドロップ"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.吹雪, 20), new WeatherPercentageOfArea(WeatherType.雪, 40), new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.快晴, 5), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), };
                    var area = new Area(areaGroup, "クルザス西部高地", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "リバーズミート"));
                    area.AddFishingGroup(new FishingGround(area, "グレイテール滝"));
                    area.AddFishingGroup(new FishingGround(area, "クルザス不凍池"));
                    area.AddFishingGroup(new FishingGround(area, "クリアプール"));
                    area.AddFishingGroup(new FishingGround(area, "ドラゴンスピット"));
                    area.AddFishingGroup(new FishingGround(area, "ベーンプール南"));
                    area.AddFishingGroup(new FishingGround(area, "アッシュプール"));
                    area.AddFishingGroup(new FishingGround(area, "ベーンプール西"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("モードゥナ");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 15), new WeatherPercentageOfArea(WeatherType.妖霧, 30), new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 25), };
                    var area = new Area(areaGroup, "モードゥナ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "銀泪湖北岸"));
                    area.AddFishingGroup(new FishingGround(area, "タングル湿林源流"));
                    area.AddFishingGroup(new FishingGround(area, "唄う裂谷"));
                    area.AddFishingGroup(new FishingGround(area, "早霜峠"));
                    area.AddFishingGroup(new FishingGround(area, "タングル湿林"));
                    area.AddFishingGroup(new FishingGround(area, "唄う裂谷北部"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("アバラシア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.霊風, 10), };
                    var area = new Area(areaGroup, "アバラシア雲海", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ヴール・シアンシラン"));
                    area.AddFishingGroup(new FishingGround(area, "雲溜まり"));
                    area.AddFishingGroup(new FishingGround(area, "クラウドトップ"));
                    area.AddFishingGroup(new FishingGround(area, "ブルーウィンドウ"));
                    area.AddFishingGroup(new FishingGround(area, "モック・ウーグル島"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 35), new WeatherPercentageOfArea(WeatherType.曇り, 35), new WeatherPercentageOfArea(WeatherType.雷, 30), };
                    var area = new Area(areaGroup, "アジス・ラー", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "アルファ管区"));
                    area.AddFishingGroup(new FishingGround(area, "廃液溜まり"));
                    area.AddFishingGroup(new FishingGround(area, "超星間交信塔"));
                    area.AddFishingGroup(new FishingGround(area, "デルタ管区"));
                    area.AddFishingGroup(new FishingGround(area, "パプスの大樹"));
                    area.AddFishingGroup(new FishingGround(area, "ハビスフィア"));
                    area.AddFishingGroup(new FishingGround(area, "アジス・ラー旗艦島"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ドラヴァニア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.暴雨, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "イディルシャイア", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), new WeatherPercentageOfArea(WeatherType.砂塵, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "高地ドラヴァニア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "悲嘆の飛泉"));
                    area.AddFishingGroup(new FishingGround(area, "ウィロームリバー"));
                    area.AddFishingGroup(new FishingGround(area, "スモーキングウェイスト"));
                    area.AddFishingGroup(new FishingGround(area, "餌食の台地"));
                    area.AddFishingGroup(new FishingGround(area, "モーン大岩窟"));
                    area.AddFishingGroup(new FishingGround(area, "モーン大岩窟西"));
                    area.AddFishingGroup(new FishingGround(area, "アネス・ソー"));
                    area.AddFishingGroup(new FishingGround(area, "光輪の祭壇"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.暴雨, 10), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "低地ドラヴァニア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "サリャク河"));
                    area.AddFishingGroup(new FishingGround(area, "クイックスピル・デルタ"));
                    area.AddFishingGroup(new FishingGround(area, "サリャク河上流"));
                    area.AddFishingGroup(new FishingGround(area, "サリャク河中州"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.暴風, 10), new WeatherPercentageOfArea(WeatherType.放電, 20), new WeatherPercentageOfArea(WeatherType.快晴, 30), new WeatherPercentageOfArea(WeatherType.晴れ, 30), };
                    var area = new Area(areaGroup, "ドラヴァニア雲海", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "エイル・トーム"));
                    area.AddFishingGroup(new FishingGround(area, "グリーンスウォード島"));
                    area.AddFishingGroup(new FishingGround(area, "ウェストン・ウォーター"));
                    area.AddFishingGroup(new FishingGround(area, "ランドロード遺構"));
                    area.AddFishingGroup(new FishingGround(area, "ソーム・アル笠雲"));
                    area.AddFishingGroup(new FishingGround(area, "サルウーム・カシュ"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ギラバニア");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 45), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), };
                    var area = new Area(areaGroup, "ラールガーズリーチ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ミラージュクリーク上流"));
                    area.AddFishingGroup(new FishingGround(area, "ラールガーズリーチ"));
                    area.AddFishingGroup(new FishingGround(area, "星導山寺院入口"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 45), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷, 10), };
                    var area = new Area(areaGroup, "ギラバニア辺境地帯", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ティモン川"));
                    area.AddFishingGroup(new FishingGround(area, "夜の森"));
                    area.AddFishingGroup(new FishingGround(area, "流星の尾"));
                    area.AddFishingGroup(new FishingGround(area, "ベロジナ川"));
                    area.AddFishingGroup(new FishingGround(area, "ミラージュクリーク"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 50), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.砂塵, 5), };
                    var area = new Area(areaGroup, "ギラバニア山岳地帯", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "夫婦池"));
                    area.AddFishingGroup(new FishingGround(area, "スロウウォッシュ"));
                    area.AddFishingGroup(new FishingGround(area, "ヒース滝"));
                    area.AddFishingGroup(new FishingGround(area, "裁定者の像"));
                    area.AddFishingGroup(new FishingGround(area, "ブルズバス"));
                    area.AddFishingGroup(new FishingGround(area, "アームズ・オブ・ミード"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 10), };
                    var area = new Area(areaGroup, "ギラバニア湖畔地帯", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ロッホ・セル湖"));
                    area.AddFishingGroup(new FishingGround(area, "ロッホ・セル湖底北西"));
                    area.AddFishingGroup(new FishingGround(area, "ロッホ・セル湖底中央"));
                    area.AddFishingGroup(new FishingGround(area, "ロッホ・セル湖底南東"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (ギラバニア湖畔地帯)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("オサード");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雷, 10), new WeatherPercentageOfArea(WeatherType.風, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 25), };
                    var area = new Area(areaGroup, "紅玉海", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "紅玉台場近海"));
                    area.AddFishingGroup(new FishingGround(area, "獄之蓋近海"));
                    area.AddFishingGroup(new FishingGround(area, "ベッコウ島近海"));
                    area.AddFishingGroup(new FishingGround(area, "沖之岩近海"));
                    area.AddFishingGroup(new FishingGround(area, "オノコロ島近海"));
                    area.AddFishingGroup(new FishingGround(area, "イサリ村沿岸"));
                    area.AddFishingGroup(new FishingGround(area, "ゼッキ島近海"));
                    area.AddFishingGroup(new FishingGround(area, "紅玉台場周辺"));
                    area.AddFishingGroup(new FishingGround(area, "碧のタマミズ周辺"));
                    area.AddFishingGroup(new FishingGround(area, "スイの里周辺"));
                    area.AddFishingGroup(new FishingGround(area, "アドヴェンチャー号周辺"));
                    area.AddFishingGroup(new FishingGround(area, "紫水宮周辺"));
                    area.AddFishingGroup(new FishingGround(area, "小林丸周辺"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (紅玉海:アカククリ×10)"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (紅玉海:イチモンジ×10)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "ヤンサ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "アオサギ池"));
                    area.AddFishingGroup(new FishingGround(area, "アオサギ川"));
                    area.AddFishingGroup(new FishingGround(area, "ナマイ村溜池"));
                    area.AddFishingGroup(new FishingGround(area, "無二江東"));
                    area.AddFishingGroup(new FishingGround(area, "無二江西"));
                    area.AddFishingGroup(new FishingGround(area, "梅泉郷"));
                    area.AddFishingGroup(new FishingGround(area, "七彩渓谷"));
                    area.AddFishingGroup(new FishingGround(area, "七彩溝"));
                    area.AddFishingGroup(new FishingGround(area, "城下船場"));
                    area.AddFishingGroup(new FishingGround(area, "ドマ城前"));
                    area.AddFishingGroup(new FishingGround(area, "無二江川底南西"));
                    area.AddFishingGroup(new FishingGround(area, "無二江川底南"));
                    area.AddFishingGroup(new FishingGround(area, "高速魔導駆逐艇L‐XXIII周辺"));
                    area.AddFishingGroup(new FishingGround(area, "沈没川船周辺"));
                    area.AddFishingGroup(new FishingGround(area, "大龍瀑川底"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (ヤンサ)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴風, 5), new WeatherPercentageOfArea(WeatherType.風, 5), new WeatherPercentageOfArea(WeatherType.雨, 7), new WeatherPercentageOfArea(WeatherType.霧, 8), new WeatherPercentageOfArea(WeatherType.曇り, 10), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 25), };
                    var area = new Area(areaGroup, "アジムステップ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ネム・カール"));
                    area.AddFishingGroup(new FishingGround(area, "ハク・カール"));
                    area.AddFishingGroup(new FishingGround(area, "ヤト・カール上流"));
                    area.AddFishingGroup(new FishingGround(area, "アジム・カート"));
                    area.AddFishingGroup(new FishingGround(area, "タオ・カール"));
                    area.AddFishingGroup(new FishingGround(area, "ヤト・カール下流"));
                    area.AddFishingGroup(new FishingGround(area, "ドタール・カー"));
                    area.AddFishingGroup(new FishingGround(area, "アジム・カート湖底西"));
                    area.AddFishingGroup(new FishingGround(area, "アジム・カート湖底東"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (アジムステップ)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴雨, 5), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "ドマ町人地", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ドマ町人地"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ひんがしの国");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "クガネ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "波止場全体"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "シロガネ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "シロガネ"));
                    area.AddFishingGroup(new FishingGround(area, "シロガネ水路"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("ノルヴラント");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 5), };
                    var area = new Area(areaGroup, "クリスタリウム", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "三学科の座"));
                    area.AddFishingGroup(new FishingGround(area, "四学科の座"));
                    area.AddFishingGroup(new FishingGround(area, "クリスタリウム居室"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴風, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 15), };
                    var area = new Area(areaGroup, "ユールモア", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "廃船街"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.快晴, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.雷雨, 5), };
                    var area = new Area(areaGroup, "レイクランド", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "風化した裂け目"));
                    area.AddFishingGroup(new FishingGround(area, "錆ついた貯水池"));
                    area.AddFishingGroup(new FishingGround(area, "始まりの湖"));
                    area.AddFishingGroup(new FishingGround(area, "サレン郷"));
                    area.AddFishingGroup(new FishingGround(area, "ケンの島 (釣り)"));
                    area.AddFishingGroup(new FishingGround(area, "始まりの湖北東"));
                    area.AddFishingGroup(new FishingGround(area, "ケンの島 (銛)"));
                    area.AddFishingGroup(new FishingGround(area, "始まりの湖南東"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (レイクランド)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.暴風, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.快晴, 15), };
                    var area = new Area(areaGroup, "コルシア島", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "ワッツリバー上流"));
                    area.AddFishingGroup(new FishingGround(area, "ホワイトオイルフォールズ"));
                    area.AddFishingGroup(new FishingGround(area, "ワッツリバー下流"));
                    area.AddFishingGroup(new FishingGround(area, "シャープタンの泉"));
                    area.AddFishingGroup(new FishingGround(area, "コルシア島沿岸西"));
                    area.AddFishingGroup(new FishingGround(area, "シーゲイザーの入江"));
                    area.AddFishingGroup(new FishingGround(area, "コルシア島沿岸東"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 45), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.砂塵, 10), new WeatherPercentageOfArea(WeatherType.灼熱波, 10), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "アム・アレーン", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "砂の川"));
                    area.AddFishingGroup(new FishingGround(area, "ナバースの断絶"));
                    area.AddFishingGroup(new FishingGround(area, "アンバーヒル"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.曇り, 15), new WeatherPercentageOfArea(WeatherType.雷雨, 10), new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), };
                    var area = new Area(areaGroup, "イル・メグ", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "手鏡の湖"));
                    area.AddFishingGroup(new FishingGround(area, "姿見の湖"));
                    area.AddFishingGroup(new FishingGround(area, "上の子らの流れ"));
                    area.AddFishingGroup(new FishingGround(area, "中の子らの流れ"));
                    area.AddFishingGroup(new FishingGround(area, "末の子らの流れ"));
                    area.AddFishingGroup(new FishingGround(area, "聖ファスリクの額"));
                    area.AddFishingGroup(new FishingGround(area, "コラードの排水溝"));
                    area.AddFishingGroup(new FishingGround(area, "リェー・ギア城北"));
                    area.AddFishingGroup(new FishingGround(area, "魚たちの街"));
                    area.AddFishingGroup(new FishingGround(area, "姿見の湖中央"));
                    area.AddFishingGroup(new FishingGround(area, "ジズム・ラーン"));
                    area.AddFishingGroup(new FishingGround(area, "姿見の湖南"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (イル・メグ)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.霧, 10), new WeatherPercentageOfArea(WeatherType.雨, 10), new WeatherPercentageOfArea(WeatherType.霊風, 10), new WeatherPercentageOfArea(WeatherType.快晴, 15), new WeatherPercentageOfArea(WeatherType.晴れ, 40), new WeatherPercentageOfArea(WeatherType.曇り, 15), };
                    var area = new Area(areaGroup, "ラケティカ大森林", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "トゥシ・メキタ湖"));
                    area.AddFishingGroup(new FishingGround(area, "血の酒坏"));
                    area.AddFishingGroup(new FishingGround(area, "ロツァトル川"));
                    area.AddFishingGroup(new FishingGround(area, "ミュリルの郷愁南"));
                    area.AddFishingGroup(new FishingGround(area, "ウォーヴンオウス"));
                    area.AddFishingGroup(new FishingGround(area, "ミュリルの落涙"));
                    area.AddFishingGroup(new FishingGround(area, "トゥシ・メキタ湖北"));
                    area.AddFishingGroup(new FishingGround(area, "ダワトリ溺没神殿"));
                    area.AddFishingGroup(new FishingGround(area, "トゥシ・メキタ湖中央"));
                    area.AddFishingGroup(new FishingGround(area, "トゥシ・メキタ湖南"));
                    area.AddFishingGroup(new FishingGround(area, "未知の漁場 (ラケティカ大森林)"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.曇り, 20), new WeatherPercentageOfArea(WeatherType.晴れ, 60), new WeatherPercentageOfArea(WeatherType.快晴, 20), };
                    var area = new Area(areaGroup, "テンペスト", weatherOfArea);
                    area.AddFishingGroup(new FishingGround(area, "フラウンダーの穴蔵"));
                    area.AddFishingGroup(new FishingGround(area, "陸人の墓標"));
                    area.AddFishingGroup(new FishingGround(area, "キャリバン海底谷北西"));
                    area.AddFishingGroup(new FishingGround(area, "キャリバンの古巣穴西"));
                    area.AddFishingGroup(new FishingGround(area, "キャリバンの古巣穴東"));
                    area.AddFishingGroup(new FishingGround(area, "プルプラ洞"));
                    area.AddFishingGroup(new FishingGround(area, "ノルヴラント大陸斜面"));
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            {
                var areaGroup = new AreaGroup("エウレカ");
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 30), new WeatherPercentageOfArea(WeatherType.暴風, 30), new WeatherPercentageOfArea(WeatherType.暴雨, 30), new WeatherPercentageOfArea(WeatherType.雪, 10), };
                    var area = new Area(areaGroup, "エウレカ：アネモス帯", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.霧, 18), new WeatherPercentageOfArea(WeatherType.灼熱波, 18), new WeatherPercentageOfArea(WeatherType.雪, 18), new WeatherPercentageOfArea(WeatherType.雷, 18), new WeatherPercentageOfArea(WeatherType.吹雪, 18), };
                    var area = new Area(areaGroup, "エウレカ：パゴス帯", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 10), new WeatherPercentageOfArea(WeatherType.灼熱波, 18), new WeatherPercentageOfArea(WeatherType.雷, 18), new WeatherPercentageOfArea(WeatherType.吹雪, 18), new WeatherPercentageOfArea(WeatherType.霊風, 18), new WeatherPercentageOfArea(WeatherType.雪, 18), };
                    var area = new Area(areaGroup, "エウレカ：ピューロス帯", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                {
                    var weatherOfArea = new[] { new WeatherPercentageOfArea(WeatherType.晴れ, 12), new WeatherPercentageOfArea(WeatherType.暴雨, 22), new WeatherPercentageOfArea(WeatherType.妖霧, 22), new WeatherPercentageOfArea(WeatherType.雷雨, 22), new WeatherPercentageOfArea(WeatherType.雪, 22), };
                    var area = new Area(areaGroup, "エウレカ：ヒュダトス帯", weatherOfArea);
                    areaGroup.AddArea(area);
                    _areas.Add(area.AreaName, area);
                }
                _areaGroups.Add(areaGroup.AreaGroupName, areaGroup);
            }
            _fishGrounds = new KeyValueCollection<string, FishingGround>();
            foreach (var areaGroup in _areaGroups)
            {
                foreach (var area in areaGroup.Areas)
                {
                    foreach (var fishingGround in area.FishingGrounds)
                        _fishGrounds.Add(fishingGround.FishingGroundName, fishingGround);
                }
            }
            _fishingBates = new KeyValueCollection<string, FishingBait>();
            foreach (var fishingBate in new[]
            {
                new FishingBait("モスプパ"),
                new FishingBait("ラグワーム"),
                new FishingBait("ザリガニボール"),
                new FishingBait("ピルバグ"),
                new FishingBait("ゴビーボール"),
                new FishingBait("ブラッドワーム"),
                new FishingBait("ユスリカ"),
                new FishingBait("ラットの尾"),
                new FishingBait("クラブボール"),
                new FishingBait("クロウフライ"),
                new FishingBait("バターワーム"),
                new FishingBait("フローティングミノー"),
                new FishingBait("ブラススプーン"),
                new FishingBait("シュリンプフィーダー"),
                new FishingBait("バスボール"),
                new FishingBait("チョコボフライ"),
                new FishingBait("スプーンワーム"),
                new FishingBait("ハナアブ"),
                new FishingBait("シルバースプーン"),
                new FishingBait("メタルジグ"),
                new FishingBait("シンキングミノー"),
                new FishingBait("サンドリーチ"),
                new FishingBait("ハニーワーム"),
                new FishingBait("ヘリングボール"),
                new FishingBait("フェザントフライ"),
                new FishingBait("ヘヴィメタルジグ"),
                new FishingBait("スピナー"),
                new FishingBait("クリルフィーダー"),
                new FishingBait("サンドゲッコー"),
                new FishingBait("テッポウムシ"),
                new FishingBait("ミスリルスプーン"),
                new FishingBait("スナーブルフライ"),
                new FishingBait("トップウォーターフロッグ"),
                new FishingBait("グロウワーム"),
                new FishingBait("ホバーワーム"),
                new FishingBait("ローリングストーン"),
                new FishingBait("レインボースプーン"),
                new FishingBait("スピナーベイト"),
                new FishingBait("ストリーマー"),
                new FishingBait("弓角"),
                new FishingBait("カディスラーヴァ"),
                new FishingBait("ポーラークリル"),
                new FishingBait("バルーンバグ"),
                new FishingBait("ストーンラーヴァ"),
                new FishingBait("ツチグモ"),
                new FishingBait("ゴブリンジグ"),
                new FishingBait("ブレーデッドジグ"),
                new FishingBait("レッドバルーン"),
                new FishingBait("マグマワーム"),
                new FishingBait("バイオレットワーム"),
                new FishingBait("ブルートリーチ"),
                new FishingBait("ジャンボガガンボ"),
                new FishingBait("イクラ"),
                new FishingBait("ドバミミズ"),
                new FishingBait("赤虫"),
                new FishingBait("蚕蛹"),
                new FishingBait("活海老"),
                new FishingBait("タイカブラ"),
                new FishingBait("サスペンドミノー"),
                new FishingBait("ザザムシ"),
                new FishingBait("アオイソメ"),
                new FishingBait("フルーツワーム"),
                new FishingBait("モエビ"),
                new FishingBait("デザートフロッグ"),
                new FishingBait("マーブルラーヴァ"),
                new FishingBait("オヴィムジャーキー"),
                new FishingBait("ロバーボール"),
                new FishingBait("ショートビルミノー"),
                new FishingBait("蟲箱"),
                new FishingBait("イカの切り身"),
                new FishingBait("淡水万能餌"),
                new FishingBait("海水万能餌"),
                new FishingBait("メタルスピナー"),
                new FishingBait("イワイソメ"),
                new FishingBait("クリル"),
                new FishingBait("ファットワーム"),
                new FishingBait("万能ルアー"),
                new FishingBait("ディアデム・バルーン"),
                new FishingBait("ディアデム・レッドバルーン"),
                new FishingBait("ディアデム・ガガンボ"),
                new FishingBait("ディアデム・ホバーワーム"),
                new FishingBait("スカイボール"),
                new FishingBait("スモールギグヘッド"),
                new FishingBait("ミドルギグヘッド"),
                new FishingBait("ラージギグヘッド"),
            })
            {
                _fishingBates.Add(fishingBate.Name, fishingBate);
            }
            _fishes = new KeyValueCollection<string, Fish>();
            foreach (var fish in new[]
            {
                // リムサ・ロミンサ
                new Fish("ゴールデンフィン", _fishGrounds["リムサ・ロミンサ：上甲板層"], _fishingBates["ピルバグ"], 9, 14),
                new Fish("メガオクトパス", _fishGrounds["リムサ・ロミンサ：下甲板層"], _fishingBates["ピルバグ"], 9, 17, "ピルバグ⇒(プレ)メルトールゴビー/ハーバーへリングHQ⇒(スト)"),

                // 中央ラノシア
                new Fish("ザルエラ", _fishGrounds["ゼファードリフト沿岸"], _fishingBates["ラットの尾"], 9, 14),
                new Fish("トリックスター", _fishGrounds["ローグ川"], _fishingBates["モスプパ"], 9, 14),
                new Fish("スナガクレ", _fishGrounds["西アジェレス川"], _fishingBates["ザリガニボール"]),
                new Fish("ギガシャーク", _fishGrounds["サマーフォード沿岸"], _fishingBates["ピルバグ"], WeatherType.快晴 | WeatherType.晴れ, "ラグワーム⇒ワフーHQ⇒"),
                new Fish("ハイパーチ", _fishGrounds["ニーム川"], _fishingBates["シンキングミノー"], 5, 8),
                new Fish("クリスタルパーチ", _fishGrounds["ささやきの谷"], _fishingBates["バターワーム"], WeatherType.曇り | WeatherType.霧 | WeatherType.風),

                // 低地ラノシア
                new Fish("オオゴエナマズ", _fishGrounds["モーニングウィドー"], _fishingBates["モスプパ"]),
                new Fish("オシュオンプリント", _fishGrounds["モラビー湾西岸"], _fishingBates["ゴビーボール"]),
                new Fish("シルドラ", _fishGrounds["シダーウッド沿岸部"], _fishingBates["スプーンワーム"], WeatherType.雨),
                new Fish("マヒマヒ", _fishGrounds["オシュオン灯台"], _fishingBates["弓角"], 10, 18),
                new Fish("シルバーソブリン", _fishGrounds["オシュオン灯台"], _fishingBates["弓角"]),
                new Fish("サーベルタイガーコッド", _fishGrounds["キャンドルキープ埠頭"], _fishingBates["ゴビーボール"], 16, 22),
                new Fish("ザ・リッパー", _fishGrounds["モラビー造船廠"], _fishingBates["ゴビーボール"], 21, 3),
                new Fish("フェアリークイーン", _fishGrounds["エンプティハート"], _fishingBates["スピナー"], WeatherType.曇り | WeatherType.霧 | WeatherType.風),
                new Fish("メテオサバイバー", _fishGrounds["ソルトストランド"], _fishingBates["ラットの尾"], 3, 5, WeatherType.曇り | WeatherType.風 | WeatherType.霧),
                new Fish("オヤジウオ", _fishGrounds["ブラインドアイアン坑道"], _fishingBates["ハナアブ"], 17, 19),

                // 東ラノシア
                new Fish(
                    "フルムーンサーディン",
                    new[]
                    {
                        _fishGrounds["南ブラッドショア"],
                        _fishGrounds["コスタ・デル・ソル"],
                        _fishGrounds["幻影諸島北岸"],
                        _fishGrounds["幻影諸島南岸"],
                        _fishGrounds["サプサ産卵地"],
                        _fishGrounds["ミスト・ヴィレッジ"],
                    },
                    _fishingBates["スプーンワーム"],
                    18,
                    6),
                new Fish(
                    "リトルサラオス",
                    new[]
                    {
                        _fishGrounds["コスタ・デル・ソル"],
                        _fishGrounds["ロータノ海沖合：船尾"],
                        _fishGrounds["常夏の島北"],
                    },
                    _fishingBates["ポーラークリル"],
                    WeatherType.雨 | WeatherType.暴雨),
                new Fish(
                    "ロックロブスター",
                    new[]
                    {
                        _fishGrounds["ロータノ海沖合：船尾"],
                        _fishGrounds["船隠しの港"],
                        _fishGrounds["シルバーバザー"],
                        _fishGrounds["クレセントコーヴ"],
                    },
                    _fishingBates["ポーラークリル"],
                    17,
                    22),
                new Fish(
                    "ダークスリーパー",
                    new[]
                    {
                        _fishGrounds["隠れ滝"],
                        _fishGrounds["花蜜桟橋"],
                        _fishGrounds["ハズーバ支流：上流"],
                        _fishGrounds["ハズーバ支流：中流"],
                        _fishGrounds["ハズーバ支流：東"],
                        _fishGrounds["フォールゴウド秋瓜湖畔"],
                        _fishGrounds["ラベンダーベッド"],
                    },
                    _fishingBates["ユスリカ"],
                    15,
                    10),
                new Fish(
                    "射手魚",
                    new[]
                    {
                        _fishGrounds["東アジェレス川"],
                        _fishGrounds["レインキャッチャー樹林"],
                    },
                    _fishingBates["スナーブルフライ"],
                    WeatherType.晴れ | WeatherType.快晴),
                new Fish(
                    "雷紋魚",
                    new[]
                    {
                        _fishGrounds["レインキャッチャー樹林"],
                        _fishGrounds["ロングクライム渓谷"],
                    },
                    _fishingBates["テッポウムシ"],
                    WeatherType.雨),
                new Fish("ビアナックブーン", _fishGrounds["南ブラッドショア"], _fishingBates["シュリンプフィーダー"], 20, 23),
                new Fish("シャークトゥーナ", _fishGrounds["コスタ・デル・ソル"], _fishingBates["スプーンワーム"], 19, 21, WeatherType.快晴 | WeatherType.晴れ, "スプーンワーム⇒(プレ)フルムーンサーディンHQ⇒(スト)"),
                new Fish("ボンバードフィッシュ", _fishGrounds["北ブラッドショア"], _fishingBates["ヘリングボール"], 9, 15, WeatherType.快晴),
                new Fish("オールドマーリン", _fishGrounds["ロータノ海沖合：船首"], _fishingBates["ピルバグ"], WeatherType.雨 | WeatherType.暴雨, WeatherType.快晴, "ピルバグ⇒ハーバーへリングHQ⇒オーガバラクーダーHQ⇒"),
                new Fish("ネプトの竜", _fishGrounds["ロータノ海沖合：船尾"], _fishingBates["ポーラークリル"], WeatherType.雨 | WeatherType.暴雨, "(要リトルサラオス×3) ポーラークリル⇒\nポーラークリル⇒"),
                new Fish("ソルター", _fishGrounds["隠れ滝"], _fishingBates["ハニーワーム"], 17, 20, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ドラウンドスナイパー", _fishGrounds["東アジェレス川"], _fishingBates["スナーブルフライ"], WeatherType.快晴 | WeatherType.晴れ),
                new Fish("テルプシコレアン", _fishGrounds["レインキャッチャー樹林"], _fishingBates["ハニーワーム"], WeatherType.霧),
                new Fish("ミラースケイル", _fishGrounds["レインキャッチャー沼沢地"], _fishingBates["ユスリカ"], 9, 16, WeatherType.快晴 | WeatherType.晴れ, "ユスリカ⇒(プレ)銅魚HQ⇒(プレ)"),
                new Fish("黄金魚", _fishGrounds["レッドマンティス滝"], _fishingBates["ハニーワーム"], WeatherType.快晴 | WeatherType.晴れ, "(要オオモリナマズ×3、天候不問) ハニーワーム⇒(プレ)銀魚HQ⇒(プレ)金魚HQ⇒(スト)\nハニーワーム⇒(プレ)銀魚HQ⇒(スト)"),
                new Fish("海チョコチョコボ", _fishGrounds["常夏の島北"], _fishingBates["ラグワーム"], 8, 16, "ラグワーム⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)"),
                new Fish("リトルぺリュコス", _fishGrounds["常夏の島北"], _fishingBates["ラグワーム"], WeatherType.雨 | WeatherType.暴雨, "ラグワーム⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)"),

                // 西ラノシア
                new Fish("スケーリーフット", _fishGrounds["スウィフトパーチ入植地"], _fishingBates["ポーラークリル"], 19, 3),
                new Fish("ジャンクモンガー", _fishGrounds["スカルバレー沿岸部"], _fishingBates["ピルバグ"], 16, 2, "ピルバグ⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)"),
                new Fish("リムレーンズソード", _fishGrounds["ブルワーズ灯台"], _fishingBates["弓角"], 9, 14, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ローンリッパー", _fishGrounds["ハーフストーン沿岸部"], _fishingBates["ヘヴィメタルジグ"], WeatherType.暴風),
                new Fish("ヘルムズマンズハンド", _fishGrounds["幻影諸島北岸"], _fishingBates["ピルバグ"], 9, 15, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "ピルバグ⇒(プレ)オーシャンクラウドHQ⇒(スト)"),
                // モラモラは釣り手帳で「天候条件：あり」となっているが、各攻略情報を見る限り釣れない天候はない模様なので、ここには登録しない。
                new Fish("ラブカ", _fishGrounds["船の墓場"], _fishingBates["ラグワーム"], 17, 3, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "ラグワーム⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)ジャイアントスキッドHQ⇒(スト)"),
                new Fish("キャプテンズチャリス", _fishGrounds["サプサ産卵地"], _fishingBates["メタルジグ"], 23, 2, "メタルジグ⇒フルムーンサーディンHQ⇒"),
                new Fish("コエラカントゥス", _fishGrounds["幻影諸島南岸"], _fishingBates["スプーンワーム"], 22, 3, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "スプーンワーム⇒フルムーンサーディンHQ⇒"),
                new Fish("エンドセラス", _fishGrounds["幻影諸島南岸"], _fishingBates["スプーンワーム"], 20, 4, WeatherType.快晴 | WeatherType.晴れ, WeatherType.曇り | WeatherType.風 | WeatherType.霧, "スプーンワーム⇒フルムーンサーディンHQ⇒"),
                new Fish("シーハッグ", _fishGrounds["船隠しの港"], _fishingBates["ピルバグ"], 19, 2, WeatherType.快晴 | WeatherType.晴れ, WeatherType.快晴 | WeatherType.晴れ, "ピルバグ⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)"),

                // 高地ラノシア
                new Fish(
                    "ジェイドイール",
                    new[]
                    {
                        _fishGrounds["オークウッド"],
                        _fishGrounds["愚か者の滝"],
                        _fishGrounds["ブロンズレイク北東岸"],
                        _fishGrounds["ブロンズレイク・シャロー"],
                        _fishGrounds["ブロンズレイク北西岸"],
                    },
                    _fishingBates["バターワーム"],
                    17,
                    10),
                new Fish("トラマフィッシュ", _fishGrounds["オークウッド"], _fishingBates["スピナーベイト"], 17, 20, WeatherType.曇り | WeatherType.霧, "スピナーベイト⇒(プレ)スカルピンHQ⇒(スト)"),
                new Fish("ジャンヌ・トラウト", _fishGrounds["愚か者の滝"], _fishingBates["クロウフライ"], 4, 6),
                new Fish("ワーム・オブ・ニーム", _fishGrounds["ブロンズレイク・シャロー"], _fishingBates["バターワーム"], 19, 22),
                new Fish("スプリングキング", new []{ _fishGrounds["ブロンズレイク北東岸"], _fishGrounds["ブロンズレイク北西岸"] }, _fishingBates["スピナーベイト"], 16, 19),

                // 外地ラノシア
                new Fish("大鈍甲", _fishGrounds["ロングクライム渓谷"], _fishingBates["スピナーベイト"], 4, 9),
                new Fish("サンダーガット", _fishGrounds["ロングクライム渓谷"], _fishingBates["テッポウムシ"], 19, 3, WeatherType.雨),
                // スプリングキング (ブロンズレイク北西岸)

                // ミスト・ヴィレッジ
                new Fish("トゥイッチビアード", _fishGrounds["ミスト・ヴィレッジ"], _fishingBates["スプーンワーム"], 4, 6, WeatherType.快晴 | WeatherType.晴れ, "スプーンワーム⇒(プレ)フルムーンサーディンHQ⇒(スト)"),

                // グリダニア
                new Fish("雨乞魚", new[]{ _fishGrounds["グリダニア：翡翠湖畔"], _fishGrounds["グリダニア：囁きの渓谷"] }, _fishingBates["テッポウムシ"], 17, 2, WeatherType.雨),
                new Fish("招嵐王", _fishGrounds["グリダニア：翡翠湖畔"], _fishingBates["テッポウムシ"], 17, 2, WeatherType.雨 | WeatherType.雪),
                new Fish("ブラッディブルワー", _fishGrounds["グリダニア：紅茶川水系下流"], _fishingBates["ザリガニボール"]),
                new Fish("マトロンカープ", _fishGrounds["グリダニア：囁きの渓谷"], _fishingBates["ブラッドワーム"], 15, 21),
                new Fish("意地ブナ", _fishGrounds["グリダニア：紅茶川水系上流"], _fishingBates["クロウフライ"], 9, 14, WeatherType.曇り | WeatherType.霧),

                // 中央森林
                new Fish("カイラージョン", _fishGrounds["葉脈水系"], _fishingBates["モスプパ"], "モスプパ⇒ゼブラゴビーHQ⇒"),
                new Fish("人面魚", _fishGrounds["鏡池"], _fishingBates["バターワーム"], 21, 3, WeatherType.雨),
                new Fish(
                    "ブラックイール",
                    new[]
                    {
                        _fishGrounds["エバーシェイド"],
                        _fishGrounds["芽吹の池"],
                        _fishGrounds["青翠の奈落"],
                        _fishGrounds["ハズーバ支流：上流"],
                        _fishGrounds["ハズーバ支流：下流"],
                        _fishGrounds["ハズーバ支流：東"],
                        _fishGrounds["ユグラム川"],
                    },
                    _fishingBates["バスボール"],
                    17,
                    10),
                new Fish("レヴィンライト", _fishGrounds["エバーシェイド"], _fishingBates["ハナアブ"], 18, 23),
                new Fish("グリーンジェスター", _fishGrounds["芽吹の池"], _fishingBates["ハニーワーム"], 18, 21),
                new Fish("ブラッドバス", _fishGrounds["ハウケタ御用邸"], _fishingBates["ハニーワーム"], WeatherType.雷, "ハニーワーム⇒(プレ)銀魚HQ⇒(スト)"),

                // 東部森林
                new Fish("ダークアンブッシャー", _fishGrounds["花蜜桟橋"], _fishingBates["モスプパ"], 21, 3, "モスプパ⇒(プレ)ゼブラゴビーHQ⇒(プレ)"),
                new Fish("モルバ", _fishGrounds["さざなみ小川"], _fishingBates["ユスリカ"], 18, 2, "ユスリカ⇒(プレ)グラディエーターペタHQ⇒(スト)"),
                new Fish("ターミネーター", _fishGrounds["十二神大聖堂"], _fishingBates["バターワーム"], 21, 23),
                new Fish("シルフズベーン", _fishGrounds["青翠の奈落"], _fishingBates["バターワーム"], WeatherType.雨, "バターワーム⇒(プレ)銅魚HQ⇒(スト)"),
                new Fish(
                    "オークルート",
                    new[]
                    {
                        _fishGrounds["シルフランド渓谷"],
                        _fishGrounds["プラウドクリーク"],
                    },
                    _fishingBates["テッポウムシ"],
                    17,
                    10),
                new Fish("マッシュルームクラブ", _fishGrounds["シルフランド渓谷"], _fishingBates["スピナーベイト"], WeatherType.曇り | WeatherType.霧, "スピナーベイト⇒スカルピンHQ⇒"),
                new Fish("マジック・マッシュルームクラブ", _fishGrounds["シルフランド渓谷"], _fishingBates["スピナー"], WeatherType.雨 | WeatherType.雷, WeatherType.曇り | WeatherType.霧, "スピナーベイト⇒スカルピンHQ⇒"),

                // 南部森林
                new Fish("ビッグバイパー", _fishGrounds["ハズーバ支流：上流"], _fishingBates["バターワーム"], 18, 19),
                new Fish("フローティングボルダー", _fishGrounds["ハズーバ支流：下流"], _fishingBates["バスボール"], 0, 8, WeatherType.曇り | WeatherType.霧),
                new Fish("グリナー", _fishGrounds["ハズーバ支流：東"], _fishingBates["ハナアブ"]),
                new Fish("シンカー", _fishGrounds["ハズーバ支流：中流"], _fishingBates["シンキングミノー"], 21, 3, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ゴブスレイヤー", _fishGrounds["ゴブリン族の生簀"], _fishingBates["ハニーワーム"], WeatherType.雷 | WeatherType.雷雨, "ハニーワーム⇒銀魚HQ⇒"),
                new Fish("雷神魚", _fishGrounds["根渡り沼"], _fishingBates["カディスラーヴァ"], WeatherType.雷雨 | WeatherType.雷),
                new Fish("雷皇子", _fishGrounds["根渡り沼"], _fishingBates["カディスラーヴァ"], WeatherType.雷雨),
                new Fish("オオタキタロ", _fishGrounds["ウルズの恵み"], _fishingBates["グロウワーム"], WeatherType.雷 | WeatherType.雷雨),
                new Fish("ナミタロ", _fishGrounds["ウルズの恵み"], new[]{ _fishingBates["グロウワーム"], _fishingBates["トップウォーターフロッグ"] }, WeatherType.雷 | WeatherType.雷雨, "(要オオタキタロ×1、雷雨) グロウワーム⇒\n(天候不問) トップウォーターフロッグ⇒"),

                // 北部森林
                new Fish("ブルーウィドー", _fishGrounds["さざめき川"], _fishingBates["フローティングミノー"], 9, 14),
                new Fish("ジャッジレイ", _fishGrounds["フォールゴウド秋瓜湖畔"], _fishingBates["フェザントフライ"], 17, 21),
                new Fish("シャドーストリーク", _fishGrounds["プラウドクリーク"], _fishingBates["トップウォーターフロッグ"], 4, 10, WeatherType.霧),
                new Fish("コーネリア", _fishGrounds["タホトトル湖畔"], new[]{ _fishingBates["レインボースプーン"], _fishingBates["グロウワーム"] }, "(要ボクシングプレコ×5) グロウワーム→\nレインボースプーン→"),

                // ラベンダーベッド
                new Fish("スウィートニュート", _fishGrounds["ラベンダーベッド"], _fishingBates["ユスリカ"], 23, 4, WeatherType.霧, "ユスリカ⇒(プレ)グラディエーターペタHQ⇒(スト)"),

                // 西ザナラーン
                new Fish("銅鏡", _fishGrounds["ノフィカの井戸"], _fishingBates["バターワーム"], WeatherType.快晴 | WeatherType.晴れ),
                new Fish("マッドゴーレム", _fishGrounds["足跡の谷"], _fishingBates["バターワーム"], 21, 3),
                new Fish("リベットオイスター", _fishGrounds["ベスパーベイ"], _fishingBates["ヘヴィメタルジグ"]),
                new Fish("フィンガーズ", _fishGrounds["クレセントコーヴ"], _fishingBates["ポーラークリル"], 17, 18),
                new Fish("ダーティーヘリング", _fishGrounds["シルバーバザー"], _fishingBates["ポーラークリル"], 20, 22),
                new Fish("タイタニックソー", _fishGrounds["ウエストウインド岬"], _fishingBates["ラグワーム"], 9, 15, WeatherType.快晴 | WeatherType.晴れ, "ラグワーム⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)"),
                new Fish("パイレーツハンター", _fishGrounds["ウエストウインド岬"], _fishingBates["ラグワーム"], "(要ワフー×6) ラグワーム⇒(プレ)メルトールゴビーHQ⇒(スト)\nラグワーム⇒(プレ)メルトールゴビーHQ⇒(スト)ワフーHQ⇒(スト)"),
                new Fish("ンデンデキ", _fishGrounds["ムーンドリップ洞窟"], _fishingBates["ハニーワーム"], 18, 5, WeatherType.霧, "ハニーワーム⇒(プレ)銀魚HQ⇒(スト)アサシンペタHQ⇒(スト)"),
                new Fish("ヴァンパイアウィップ", _fishGrounds["パラタの墓所"], _fishingBates["ハニーワーム"], WeatherType.雨, WeatherType.快晴 | WeatherType.曇り | WeatherType.晴れ | WeatherType.霧, "ハニーワーム⇒(プレ)銀魚HQ⇒(スト)"),

                // 中央ザナラーン
                new Fish("ドリームゴビー", _fishGrounds["スートクリーク上流"], _fishingBates["ザリガニボール"], 17, 3),
                new Fish("ヌルヌルキング", _fishGrounds["スートクリーク下流"], _fishingBates["ブラッドワーム"], 19, 0),
                new Fish("オールドソフティ", _fishGrounds["クラッチ狭間"], _fishingBates["ブラッドワーム"], 17, 21),
                new Fish("ダークナイト", _fishGrounds["アンホーリーエアー"], _fishingBates["クロウフライ"], WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧),

                // 東ザナラーン
                new Fish("ジンコツシャブリ", _fishGrounds["ドライボーン北湧水地"], _fishingBates["ハナアブ"], 20, 3, WeatherType.雨 | WeatherType.暴雨),
                new Fish("マッドピルグリム", _fishGrounds["ドライボーン南湧水地"], _fishingBates["ユスリカ"], 17, 7, WeatherType.雨 | WeatherType.暴雨),
                new Fish("ナルザルイール", _fishGrounds["ユグラム川"], _fishingBates["ハニーワーム"], 17, 10),
                new Fish("ワーデンズワンド", _fishGrounds["ユグラム川"], _fishingBates["ハニーワーム"], 17, 20, WeatherType.快晴),
                new Fish("サンディスク", _fishGrounds["バーニングウォール"], _fishingBates["カディスラーヴァ"], 10, 15, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("サウザンドイヤー・イーチ", _fishGrounds["バーニングウォール"], _fishingBates["テッポウムシ"], WeatherType.霧),

                // 南ザナラーン
                new Fish("ホローアイズ", _fishGrounds["リザードクリーク"], _fishingBates["ユスリカ"], WeatherType.霧, "ユスリカ⇒(プレ)銅魚HQ⇒(スト)"),
                new Fish("パガルザンディスカス", _fishGrounds["ザハラクの湧水"], _fishingBates["グロウワーム"], WeatherType.灼熱波 | WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ディスコボルス", _fishGrounds["ザハラクの湧水"], _fishingBates["グロウワーム"], 12, 18, WeatherType.灼熱波),
                new Fish("アイアンヌース", _fishGrounds["忘れられたオアシス"], _fishingBates["スピナー"], WeatherType.霧),
                new Fish("ホーリーカーペット", _fishGrounds["サゴリー砂海"], _fishingBates["サンドリーチ"], 9, 16, WeatherType.灼熱波, "サンドリーチ⇒(スト)サンドストームライダーHQ⇒(スト)"),
                new Fish("ヘリコプリオン", _fishGrounds["サゴリー砂海"], _fishingBates["サンドリーチ"], 8, 20, WeatherType.曇り | WeatherType.霧, WeatherType.灼熱波, "サンドリーチ⇒(スト)サンドストームライダー⇒(スト)"),
                new Fish("オルゴイコルコイ", _fishGrounds["サゴリー砂丘"], _fishingBates["サンドリーチ"], WeatherType.灼熱波, "サンドリーチ⇒サンドストームライダーHQ⇒"),

                // 北ザナラーン
                new Fish("ハンニバル", _fishGrounds["ブルーフォグ湧水地"], _fishingBates["スピナーベイト"], 22, 4, WeatherType.霧, "スピナーベイト⇒(プレ)スカルピンHQ⇒(スト)"),
                new Fish("ウーツナイフ・ゼニス", _fishGrounds["青燐泉"], _fishingBates["ハニーワーム"], 1, 4, WeatherType.快晴 | WeatherType.晴れ, WeatherType.霧, "ハニーワーム⇒銀魚HQ⇒"),

                // ゴブレットビュート
                new Fish("スピアノーズ", _fishGrounds["ゴブレットビュート"], _fishingBates["カディスラーヴァ"], 21, 0, WeatherType.曇り | WeatherType.霧),

                // クルザス中央高地
                new Fish("ダナフェンズマーク", _fishGrounds["クルザス川"], _fishingBates["フェザントフライ"], WeatherType.吹雪),
                new Fish("カローンズランタン", _fishGrounds["聖ダナフェンの旅程"], _fishingBates["グロウワーム"], 0, 4, WeatherType.吹雪 | WeatherType.雪),
                new Fish("スターブライト", _fishGrounds["剣ヶ峰山麓"], _fishingBates["ハナアブ"], 21, 4, WeatherType.快晴 | WeatherType.晴れ, "ハナアブ⇒(プレ)アバラシアスメルトHQ⇒(スト)"),
                new Fish("ドーンメイデン", _fishGrounds["キャンプ・ドラゴンヘッド溜池"], _fishingBates["フェザントフライ"], 5, 7, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("メイトリアーク", _fishGrounds["調査隊の氷穴"], _fishingBates["ハナアブ"], "ハナアブ⇒(プレ)アバラシアスメルトHQ⇒(プレ)"),
                new Fish("ダークスター", _fishGrounds["聖ダナフェンの落涙"], new[]{ _fishingBates["ハニーワーム"], _fishingBates["チョコボフライ"] }, 19, 4, WeatherType.吹雪 | WeatherType.雪, "(要ランプマリモ×5) チョコボフライ⇒\nハニーワーム⇒(プレ)アバラシアスメルトHQ⇒(スト)"),
                new Fish("ブルーコープス", _fishGrounds["スノークローク大氷壁"], _fishingBates["カディスラーヴァ"], WeatherType.雪 | WeatherType.吹雪, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("アノマロカリス", _fishGrounds["イシュガルド大雲海"], _fishingBates["ホバーワーム"], 10, 15, WeatherType.快晴 | WeatherType.晴れ, "ホバーワーム⇒スプリッドクラウドHQ⇒"),
                new Fish("マハール", _fishGrounds["ウィッチドロップ"], _fishingBates["ホバーワーム"], WeatherType.快晴 | WeatherType.晴れ, WeatherType.吹雪, "ホバーワーム⇒(スト)スプリッドクラウドHQ⇒(スト)"),
                new Fish("ショニサウルス", _fishGrounds["ウィッチドロップ"], _fishingBates["ホバーワーム"], WeatherType.吹雪, "ホバーワーム⇒スプリッドクラウドHQ⇒マハールHQ⇒"),

                // クルザス西部高地
                new Fish("クルザスパファー", _fishGrounds["リバーズミート"], _fishingBates["ブルートリーチ"], WeatherType.雪 | WeatherType.吹雪),
                new Fish("ファットパース", _fishGrounds["リバーズミート"], _fishingBates["ブルートリーチ"]),
                new Fish("グレイシャーコア", _fishGrounds["グレイテール滝"], _fishingBates["ジャンボガガンボ"], WeatherType.雪 | WeatherType.吹雪),
                new Fish("ハイウィンドジェリー", _fishGrounds["グレイテール滝"], _fishingBates["ジャンボガガンボ"], WeatherType.晴れ | WeatherType.快晴, "(要フィッシュアイ) ジャンボガガンボ⇒スカイワームHQ⇒"),
                new Fish("アイスイーター", _fishGrounds["グレイテール滝"], _fishingBates["ジャンボガガンボ"], WeatherType.雪 | WeatherType.吹雪, "ジャンボガガンボ⇒グレイシャーコアHQ⇒\n"),
                new Fish("ヘイルイーター", _fishGrounds["グレイテール滝"], _fishingBates["ジャンボガガンボ"], WeatherType.吹雪, "ジャンボガガンボ⇒グレイシャーコアHQ⇒(プレ)\n"),
                new Fish("ソーサラーフィッシュ", _fishGrounds["クルザス不凍池"], _fishingBates["ゴブリンジグ"], 8, 20),
                new Fish("ホワイトオクトパス", _fishGrounds["クルザス不凍池"], _fishingBates["ブルートリーチ"], 8, 18),
                new Fish("クルザスクリオネ", _fishGrounds["クルザス不凍池"], _fishingBates["ブルートリーチ"], 0, 4, WeatherType.吹雪),
                new Fish("フレアフィッシュ", _fishGrounds["クルザス不凍池"], _fishingBates["ツチグモ"], 10, 16, WeatherType.吹雪, "ツチグモ⇒(プレ)ハルオネHQ⇒(スト)"),
                new Fish("ヒートロッド", _fishGrounds["クリアプール"], _fishingBates["ブルートリーチ"], WeatherType.吹雪 | WeatherType.雪),
                new Fish("カペリン", _fishGrounds["クリアプール"], _fishingBates["ブルートリーチ"], 0, 6, "(要フィッシュアイ)"),
                new Fish("プリーストフィッシュ", _fishGrounds["クリアプール"], _fishingBates["ブルートリーチ"], 0, 6, "(要フィッシュアイ)"),
                new Fish("ビショップフィッシュ", _fishGrounds["クリアプール"], _fishingBates["ブルートリーチ"], 10, 14, WeatherType.吹雪 | WeatherType.雪, WeatherType.快晴, "(要フィッシュアイ)"),
                new Fish("シャリベネ", _fishGrounds["クリアプール"], _fishingBates["ツチグモ"], 0, 3, WeatherType.吹雪, "(要ハルオネ×5) ツチグモ⇒\nツチグモ⇒ハルオネHQ⇒"),
                new Fish("イエティキラー", _fishGrounds["ドラゴンスピット"], _fishingBates["ブルートリーチ"], "(要引っ掛け釣り)"),
                new Fish("ベーンクラーケン", _fishGrounds["ベーンプール南"], _fishingBates["ブルートリーチ"], WeatherType.吹雪 | WeatherType.雪 | WeatherType.曇り | WeatherType.霧, "(要フィッシュアイ)"),
                new Fish("ネモ", _fishGrounds["ベーンプール南"], _fishingBates["ブルートリーチ"], WeatherType.雪, WeatherType.吹雪),
                new Fish("ラ・レアル", _fishGrounds["アッシュプール"], _fishingBates["ブルートリーチ"]),
                new Fish("氷神魚", _fishGrounds["ベーンプール西"], _fishingBates["ブルートリーチ"], WeatherType.雪 | WeatherType.吹雪),
                new Fish("雪乞魚", _fishGrounds["ベーンプール西"], _fishingBates["ブルートリーチ"], WeatherType.雪 | WeatherType.吹雪),
                new Fish("氷の巫女", _fishGrounds["ベーンプール西"], _fishingBates["カディスラーヴァ"], WeatherType.雪 | WeatherType.吹雪, WeatherType.吹雪, "(要フィッシュアイ)カディスラーヴァ⇒(プレ)ハルオネHQ⇒(スト)"),

                // モードゥナ
                new Fish("ヘリオバティス", _fishGrounds["銀泪湖北岸"], _fishingBates["カディスラーヴァ"], 17, 9),
                new Fish("エーテルラウス", _fishGrounds["銀泪湖北岸"], _fishingBates["グロウワーム"], 3, 13, WeatherType.妖霧),
                new Fish("インフェルノホーン", _fishGrounds["タングル湿林源流"], _fishingBates["グロウワーム"], WeatherType.妖霧, WeatherType.晴れ | WeatherType.快晴),
                new Fish("血紅龍", _fishGrounds["唄う裂谷"], _fishingBates["ハニーワーム"], 4, 12, WeatherType.快晴 | WeatherType.晴れ, WeatherType.霧, "ハニーワーム⇒(プレ)銀魚HQ⇒(スト)"),
                new Fish("ヴォイドバス", _fishGrounds["早霜峠"], _fishingBates["グロウワーム"], WeatherType.快晴 | WeatherType.晴れ, WeatherType.妖霧),
                new Fish("腐魚", _fishGrounds["タングル湿林"], _fishingBates["レインボースプーン"], WeatherType.妖霧),
                new Fish("ニンジャベタ", _fishGrounds["タングル湿林"], _fishingBates["ユスリカ"], 18, 9, WeatherType.妖霧, "ユスリカ⇒(プレ)グラディエーターベタHQ⇒(スト)アサシンベタHQ⇒(スト)"),
                new Fish("ジャノ", _fishGrounds["唄う裂谷北部"], _fishingBates["ハニーワーム"], 8, 18, WeatherType.妖霧, "ハニーワーム⇒(プレ)銀魚HQ⇒(プレ)金魚HQ⇒(スト)"),
                new Fish("クノ・ザ・キラー", _fishGrounds["唄う裂谷北部"], _fishingBates["ハニーワーム"], WeatherType.妖霧, "(要ジャノ×1) ハニーワーム⇒銀魚HQ⇒アサシンベタHQ⇒"),

                // アバラシア雲海
                new Fish("出目金", _fishGrounds["ヴール・シアンシラン"], _fishingBates["ブルートリーチ"], 9, 15),
                new Fish("カイマン", new[]{ _fishGrounds["ヴール・シアンシラン"], _fishGrounds["餌食の台地"] }, _fishingBates["ブレーデッドジグ"], 18, 21, "ブレーデッドジグ⇒ブルフロッグHQ⇒"),
                new Fish("水墨魚", _fishGrounds["ヴール・シアンシラン"], _fishingBates["ブルートリーチ"], 14, 16, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("バヌバヌヘッド", _fishGrounds["雲溜まり"], _fishingBates["ブレーデッドジグ"], WeatherType.晴れ | WeatherType.快晴),
                new Fish("ゲイラキラー", _fishGrounds["雲溜まり"], _fishingBates["ブルートリーチ"], WeatherType.晴れ | WeatherType.快晴),
                new Fish("パイッサキラー", _fishGrounds["雲溜まり"], _fishingBates["ブレーデッドジグ"], 8, 12, WeatherType.霧, WeatherType.快晴, "ブレーデッドジグ⇒ブルフロッグHQ⇒"),
                new Fish("スターフラワー", new [] { _fishGrounds["クラウドトップ"], _fishGrounds["モック・ウーグル島"] },_fishingBates["レッドバルーン"], WeatherType.快晴 | WeatherType.晴れ),
                new Fish("フリーシーモトロ", _fishGrounds["クラウドトップ"], _fishingBates["ジャンボガガンボ"], WeatherType.快晴 | WeatherType.晴れ),
                new Fish("シーロストラタスモトロ", _fishGrounds["クラウドトップ"], _fishingBates["ジャンボガガンボ"], 10, 13, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ストームコア", _fishGrounds["ブルーウィンドウ"], _fishingBates["レッドバルーン"], WeatherType.風 | WeatherType.曇り | WeatherType.霧),
                new Fish("ザ・セカンドワン", _fishGrounds["ブルーウィンドウ"], _fishingBates["ジャンボガガンボ"], WeatherType.風),
                new Fish("天空珊瑚", _fishGrounds["モック・ウーグル島"], _fishingBates["ジャンボガガンボ"], 0, 6, WeatherType.快晴 | WeatherType.晴れ, "(要引っ掛け釣り)"),
                new Fish("バスキングシャーク", _fishGrounds["モック・ウーグル島"], _fishingBates["ジャンボガガンボ"], WeatherType.霧, WeatherType.快晴, "ジャンボガガンボ⇒スカイフェアリー・セレネHQ⇒"),
                new Fish("クラウドバタフライ", _fishGrounds["モック・ウーグル島"], new[]{ _fishingBates["ジャンボガガンボ"], _fishingBates["レッドバルーン"] }, 5, 7, WeatherType.快晴, "(要スコーピオンフライ×3) ジャンボガガンボ⇒\n(スカイフェアリーセレネ×3) レッドバルーン⇒\nジャンボガガンボ⇒(プレ)"),

                // アジス・ラー
                new Fish("ブラッドスキッパー", new[]{ _fishGrounds["アルファ管区"], _fishGrounds["廃液溜まり"] }, _fishingBates["バイオレットワーム"], WeatherType.雷),
                new Fish("ハイアラガンクラブ改", _fishGrounds["アルファ管区"], _fishingBates["バイオレットワーム"], "バイオレットワーム⇒白金魚HQ⇒"),
                new Fish("魔科学物質666", _fishGrounds["廃液溜まり"], _fishingBates["バイオレットワーム"], WeatherType.晴れ | WeatherType.曇り | WeatherType.雷, "(要フィッシュアイ)バイオレットワーム⇒白金魚HQ⇒(プレ)"),
                new Fish("オイルイール", new []{ _fishGrounds["超星間交信塔"], _fishGrounds["アジス・ラー旗艦島"] }, _fishingBates["バイオレットワーム"], WeatherType.雷, "バイオレットワーム⇒白金魚HQ⇒"),
                new Fish("オリファントノーズ", _fishGrounds["超星間交信塔"], _fishingBates["バイオレットワーム"], 18, 0, WeatherType.雷),
                new Fish("セティ", _fishGrounds["超星間交信塔"], _fishingBates["バイオレットワーム"], 18, 22, WeatherType.曇り, WeatherType.雷, "(要フィッシュアイ)"),
                new Fish("バイオピラルク", _fishGrounds["デルタ管区"], _fishingBates["ブルートリーチ"], 18, 3, WeatherType.曇り),
                new Fish("バイオガピラルク", _fishGrounds["デルタ管区"], _fishingBates["ツチグモ"], 21, 2, WeatherType.曇り, "ツチグモ⇒(プレ)エーテルアイHQ⇒(スト)"),
                new Fish("プチアクソロトル", _fishGrounds["パプスの大樹"], _fishingBates["ブルートリーチ"], 21, 0),
                new Fish("肺魚", _fishGrounds["パプスの大樹"], _fishingBates["ブルートリーチ"], WeatherType.曇り),
                new Fish("ハンドレッドアイ", _fishGrounds["パプスの大樹"], _fishingBates["ツチグモ"], 6, 10, WeatherType.晴れ | WeatherType.曇り | WeatherType.雷, "ツチグモ⇒エーテルアイHQ⇒(プレ)"),
                new Fish("トゥプクスアラ", _fishGrounds["ハビスフィア"], _fishingBates["ジャンボガガンボ"], 15, 18, "ジャンボガガンボ⇒スカイハイフィッシュHQ⇒"),
                new Fish("スチュペンデミス", _fishGrounds["ハビスフィア"], _fishingBates["ジャンボガガンボ"], WeatherType.晴れ | WeatherType.曇り | WeatherType.雷),
                new Fish("クリスタルピジョン", _fishGrounds["ハビスフィア"], _fishingBates["ジャンボガガンボ"], WeatherType.晴れ, WeatherType.雷, "(要フィッシュアイ)ジャンボガガンボ⇒スカイハイフィッシュHQ⇒"),
                new Fish("ジュエリージェリー", _fishGrounds["アジス・ラー旗艦島"], _fishingBates["バイオレットワーム"], 20, 3),
                new Fish("バレルアイ", _fishGrounds["アジス・ラー旗艦島"], _fishingBates["バイオレットワーム"],  WeatherType.雷, "(要フィッシュアイ)バイオレットワーム⇒白金魚HQ⇒"),
                new Fish("オプロプケン", _fishGrounds["アジス・ラー旗艦島"], _fishingBates["バイオレットワーム"],  WeatherType.雷, "バイオレットワーム⇒白金魚HQ⇒"),
                new Fish("アラガンブレード・シャーク", _fishGrounds["アジス・ラー旗艦島"], _fishingBates["バイオレットワーム"], WeatherType.曇り, WeatherType.雷, "バイオレットワーム⇒白金魚HQ⇒"),
                new Fish("オパビニア", _fishGrounds["アジス・ラー旗艦島"], _fishingBates["バイオレットワーム"], WeatherType.雷, "(要 オプロプケン×3、要フィッシュアイ) バイオレットワーム⇒\nバイオレットワーム⇒オプロプケンHQ⇒白金魚HQ⇒"),

                // 高地ドラヴァニア
                new Fish("ピピラ・ピラ", new[]{ _fishGrounds["悲嘆の飛泉"], _fishGrounds["ウィロームリバー"],_fishGrounds["餌食の台地"] }, _fishingBates["ゴブリンジグ"], WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧),
                new Fish("草魚", _fishGrounds["悲嘆の飛泉"], _fishingBates["ゴブリンジグ"], WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧),
                new Fish("アカザ", new[]{ _fishGrounds["悲嘆の飛泉"], _fishGrounds["ウィロームリバー"], _fishGrounds["スモーキングウェイスト"] }, _fishingBates["ブルートリーチ"], WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧),
                new Fish("スケイルリッパー", _fishGrounds["悲嘆の飛泉"], _fishingBates["ブルートリーチ"], WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧),
                new Fish("ドラヴァニアンバス", _fishGrounds["ウィロームリバー"], _fishingBates["ブルートリーチ"], 0, 6, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧),
                new Fish("フォークタン", _fishGrounds["ウィロームリバー"], _fishingBates["ブルートリーチ"], 12, 16, WeatherType.砂塵 | WeatherType.曇り, WeatherType.快晴),
                new Fish("ポリプテルス", _fishGrounds["スモーキングウェイスト"], _fishingBates["ブルートリーチ"], 21, 3),
                new Fish("アクムノタネ", _fishGrounds["スモーキングウェイスト"], _fishingBates["ブルートリーチ"], 22, 2, WeatherType.砂塵 | WeatherType.霧 | WeatherType.曇り),
                new Fish("サンダーボルト", _fishGrounds["餌食の台地"], _fishingBates["ブレーデッドジグ"], 22, 4),
                new Fish("サンダースケイル", _fishGrounds["餌食の台地"], _fishingBates["ストーンラーヴァ"], 6, 8, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, WeatherType.雷, "ストーンラーヴァ⇒マクロブラキウムHQ⇒"),
                new Fish("アンバーサラマンダー", _fishGrounds["餌食の台地"], _fishingBates["ブルートリーチ"], 6, 12),
                new Fish("メテオトータス", _fishGrounds["モーン大岩窟"], _fishingBates["マグマワーム"], WeatherType.快晴 | WeatherType.晴れ, "マグマワーム⇒グラナイトクラブHQ⇒"),
                new Fish("聖竜の涙", _fishGrounds["モーン大岩窟西"], _fishingBates["マグマワーム"], 2, 6, "(要フィッシュアイ)"),
                new Fish("マグマラウス", _fishGrounds["アネス・ソー"], _fishingBates["マグマワーム"], 18, 6, "マグマワーム⇒グラナイトクラブHQ⇒"),
                new Fish("リドル", _fishGrounds["アネス・ソー"], _fishingBates["マグマワーム"], 8, 16, WeatherType.快晴 | WeatherType.晴れ, WeatherType.快晴, "(要フィッシュアイ) マグマワーム⇒グラナイトクラブHQ⇒"),
                new Fish("ラーヴァスネイル", _fishGrounds["光輪の祭壇"], _fishingBates["マグマワーム"], WeatherType.快晴 | WeatherType.晴れ),
                new Fish("溶岩王", _fishGrounds["光輪の祭壇"], _fishingBates["マグマワーム"], 10, 17, WeatherType.快晴 | WeatherType.晴れ, "(要フィッシュアイ)マグマワーム⇒グラナイトクラブHQ⇒"),
                new Fish("溶岩帝王", _fishGrounds["光輪の祭壇"], _fishingBates["マグマワーム"], 8, 16, WeatherType.砂塵 | WeatherType.曇り | WeatherType.霧, WeatherType.快晴 | WeatherType.晴れ, "(要フィッシュアイ)マグマワーム⇒グラナイトクラブHQ⇒(スト)"),
                new Fish("プロブレマティカス", _fishGrounds["光輪の祭壇"], _fishingBates["マグマワーム"], 10, 15, WeatherType.快晴 | WeatherType.晴れ, "(要フォッシルアロワナ×3、要フィッシュアイ、時間帯不問) マグマワーム⇒グラナイトクラブHQ⇒\n(要グラナイトクラブ×5) マグマワーム⇒\nマグマワーム⇒グラナイトクラブHQ⇒"),

                // 低地ドラヴァニア
                new Fish("水瓶王", _fishGrounds["サリャク河"], _fishingBates["ブレーデッドジグ"], "ブレーデッドジグ⇒香魚HQ⇒"),
                new Fish("マダムバタフライ", _fishGrounds["クイックスピル・デルタ"], _fishingBates["ツチグモ"], 21, 2, WeatherType.快晴, "ツチグモ⇒グリロタルパHQ⇒"),
                new Fish("フィロソファーアロワナ", _fishGrounds["サリャク河上流"], _fishingBates["ブルートリーチ"], 13, 20, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("アブトアード", _fishGrounds["サリャク河上流"], _fishingBates["ブルートリーチ"], WeatherType.霧 | WeatherType.曇り),
                new Fish("スピーカー", _fishGrounds["サリャク河上流"], _fishingBates["ストーンラーヴァ"], 16, 8, WeatherType.曇り | WeatherType.霧, WeatherType.暴雨, "ストーンラーヴァ⇒グリロタルパHQ"),
                new Fish("鎧魚", _fishGrounds["サリャク河上流"], _fishingBates["ストーンラーヴァ"], 1, 4, WeatherType.快晴, "(要グリロタルパ×6、天候/時間帯不問) ストーンラーヴァ⇒(プレ)\nストーンラーヴァ⇒(プレ)グリロタルパHQ⇒(プレ)"),
                new Fish("サリャクカイマン", _fishGrounds["サリャク河中州"], _fishingBates["ブレーデッドジグ"], 15, 18, "ブレーデッドジグ⇒ブルフロッグHQ⇒"),
                new Fish("バーサーカーベタ", _fishGrounds["サリャク河中州"], _fishingBates["ブルートリーチ"], WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ゴブリバス", _fishGrounds["サリャク河中州"], _fishingBates["ブルートリーチ"], 0, 6, WeatherType.曇り, WeatherType.霧),
                new Fish("万能のゴブリバス", _fishGrounds["サリャク河中州"], _fishingBates["ブルートリーチ"], 2, 6, WeatherType.雨, WeatherType.暴雨, "ブルートリーチ⇒香魚HQ⇒"),

                // ドラヴァニア雲海
                new Fish("キッシング・グラミー", _fishGrounds["エイル・トーム"], _fishingBates["ストーンラーヴァ"], 9, 0),
                new Fish("ヴィゾーヴニル", _fishGrounds["エイル・トーム"], _fishingBates["ブルートリーチ"], 8, 9),
                new Fish("モグルグポンポン", _fishGrounds["グリーンスウォード島"], _fishingBates["ブルートリーチ"], 10, 13, WeatherType.暴風, WeatherType.快晴, "※ポンポンポンが釣れたらトレードリリース"),
                new Fish("サカサナマズ", _fishGrounds["ウェストン・ウォーター"], _fishingBates["ストーンラーヴァ"], 16, 19),
                new Fish("アミア・カルヴァ", _fishGrounds["ウェストン・ウォーター"], _fishingBates["ブルートリーチ"], 8, 12),
                new Fish("ボサボサ", _fishGrounds["ウェストン・ウォーター"], _fishingBates["ブルートリーチ"], WeatherType.曇り, WeatherType.暴風, "(要フィッシュアイ)"),
                new Fish("サンセットセイル", _fishGrounds["ランドロード遺構"], _fishingBates["ジャンボガガンボ"], 15, 17, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ラタトスクソウル", _fishGrounds["ランドロード遺構"], _fishingBates["ジャンボガガンボ"], 4, 6, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("プテラノドン", _fishGrounds["ソーム・アル笠雲"], _fishingBates["ジャンボガガンボ"], 9, 17),
                new Fish("ディモルフォドン", _fishGrounds["ソーム・アル笠雲"], _fishingBates["ジャンボガガンボ"], WeatherType.快晴, WeatherType.暴風, "(要フィッシュアイ)ジャンボガガンボ⇒(スト)スカイハイフィッシュHQ⇒(スト)"),
                new Fish("マナセイル", _fishGrounds["サルウーム・カシュ"], _fishingBates["ジャンボガガンボ"], 10, 14, WeatherType.快晴 | WeatherType.晴れ, "ジャンボガガンボ⇒スカイフェアリー・セレネHQ⇒"),
                new Fish("ブラウンブーメラン", _fishGrounds["サルウーム・カシュ"], _fishingBates["レッドバルーン"], WeatherType.曇り),
                new Fish("ストームブラッドライダー", _fishGrounds["サルウーム・カシュ"], _fishingBates["ジャンボガガンボ"], WeatherType.快晴, WeatherType.暴風, "(要フィッシュアイ)"),
                new Fish("ランデロプテルス", _fishGrounds["サルウーム・カシュ"], _fishingBates["ジャンボガガンボ"], 5, 8, WeatherType.暴風, "(要スカイハイフィッシュ×5) ジャンボガガンボ⇒\nジャンボガガンボ⇒スカイハイフィッシュHQ⇒"),

                // ラールガーズリーチ
                new Fish("ミューヌフィッシュ", new []{ _fishGrounds["ミラージュクリーク上流"], _fishGrounds["ティモン川"], _fishGrounds["夜の森"] }, _fishingBates["赤虫"], WeatherType.曇り | WeatherType.霧, "赤虫⇒ギラバニアントラウトHQ⇒"),
                new Fish("フックスティーラー", _fishGrounds["ミラージュクリーク上流"], _fishingBates["赤虫"], "赤虫⇒ギラバニアントラウトHQ⇒"),
                new Fish("シデンナマズ", _fishGrounds["ラールガーズリーチ"], _fishingBates["赤虫"], WeatherType.雷, "赤虫⇒ギアラバニアントラウトHQ⇒"),
                new Fish("レッドテイル", _fishGrounds["星導山寺院入口"], _fishingBates["ドバミミズ"], WeatherType.曇り | WeatherType.霧, "ドバミミズ⇒バルーンフロッグHQ⇒(スト)"),
                new Fish("レッドテイルゾンビー", _fishGrounds["星導山寺院入口"], _fishingBates["ドバミミズ"], 8, 12, WeatherType.曇り, "ドバミミズ⇒バルーンフロッグHQ⇒(スト)"),

                // ギラバニア辺境地帯
                new Fish("サーメットヘッド", _fishGrounds["ティモン川"], _fishingBates["ザザムシ"], 16, 20),
                new Fish("クセナカンサス", _fishGrounds["ティモン川"], _fishingBates["ザザムシ"], 16, 20, "ザザムシ⇒サーメットヘッドHQ"),
                new Fish("レイスフィッシュ", _fishGrounds["夜の森"], _fishingBates["ザザムシ"], 0, 4, WeatherType.霧),
                new Fish("サファイアファン", _fishGrounds["夜の森"], _fishingBates["ザザムシ"], WeatherType.雷),
                new Fish("小流星", _fishGrounds["流星の尾"], _fishingBates["サスペンドミノー"], WeatherType.霧 | WeatherType.曇り),
                new Fish("ハンテンナマズ", _fishGrounds["流星の尾"], _fishingBates["イクラ"], 16, 19),
                new Fish("ニルヴァーナクラブ", _fishGrounds["流星の尾"], _fishingBates["サスペンドミノー"], WeatherType.霧 | WeatherType.曇り),
                new Fish("カーディナルフィッシュ", _fishGrounds["流星の尾"], _fishingBates["サスペンドミノー"], 19, 23, WeatherType.霧 | WeatherType.曇り),
                new Fish("アークビショップフィッシュ", _fishGrounds["流星の尾"], _fishingBates["サスペンドミノー"], 12, 16),
                new Fish("タニクダリ", _fishGrounds["ベロジナ川"], _fishingBates["ザザムシ"], WeatherType.霧),
                new Fish("ミラージュマヒ", _fishGrounds["ミラージュクリーク"], _fishingBates["サスペンドミノー"], 4, 8, WeatherType.晴れ | WeatherType.快晴),
                new Fish("コープスチャブ", _fishGrounds["ミラージュクリーク"], _fishingBates["サスペンドミノー"], 20, 0, WeatherType.快晴),


                // ギラバニア山岳地帯
                new Fish("ボンドスプリッター", _fishGrounds["夫婦池"], _fishingBates["サスペンドミノー"], WeatherType.砂塵),
                new Fish("ドレパナスピス", _fishGrounds["夫婦池"], _fishingBates["サスペンドミノー"], WeatherType.砂塵, "(要ボンドスプリッター×2) サスペンドミノー⇒\nサスペンドミノー⇒(スト)"),
                new Fish("ナガレクダリ", _fishGrounds["スロウウォッシュ"], _fishingBates["赤虫"], 8, 12, "赤虫⇒ギラバニアントラウトHQ"),
                new Fish("スティールシャーク", _fishGrounds["ヒース滝"], _fishingBates["ザザムシ"], WeatherType.快晴),
                new Fish("ラストティアー", _fishGrounds["ヒース滝"], _fishingBates["イクラ"], WeatherType.霧, WeatherType.晴れ),

                new Fish("瞑想魚", new [] { _fishGrounds["裁定者の像"], _fishGrounds["ブルズバス"] }, _fishingBates["ザザムシ"], WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵),
                new Fish("裁定魚", _fishGrounds["裁定者の像"], _fishingBates["ザザムシ"], WeatherType.風),
                new Fish("解脱魚", _fishGrounds["裁定者の像"], _fishingBates["サスペンドミノー"], WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵, WeatherType.快晴),
                new Fish("ブルズバイト", _fishGrounds["ブルズバス"], _fishingBates["サスペンドミノー"], WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵),
                new Fish("ワニガメ", _fishGrounds["ブルズバス"], _fishingBates["サスペンドミノー"], WeatherType.晴れ | WeatherType.快晴),
                new Fish("ヘモン", _fishGrounds["ブルズバス"], _fishingBates["サスペンドミノー"], 16, 20, WeatherType.曇り),
                new Fish("イースタンパイク", _fishGrounds["アームズ・オブ・ミード"], _fishingBates["活海老"], WeatherType.晴れ | WeatherType.快晴),
                new Fish("ロックフィッシュ", _fishGrounds["アームズ・オブ・ミード"], _fishingBates["サスペンドミノー"], 12, 16, WeatherType.曇り | WeatherType.風 | WeatherType.霧 | WeatherType.砂塵),
                new Fish("アラミガンベール", _fishGrounds["アームズ・オブ・ミード"], _fishingBates["サスペンドミノー"], WeatherType.快晴, WeatherType.晴れ),

                // ギラバニア湖畔地帯
                new Fish("ソルトミル", _fishGrounds["ロッホ・セル湖"], _fishingBates["蚕蛹"], WeatherType.曇り | WeatherType.霧, "蚕蛹⇒ロックソルトフィッシュHQ⇒"),
                new Fish("スカルプター", _fishGrounds["ロッホ・セル湖"], _fishingBates["蚕蛹"], 12, 18, WeatherType.雷雨, "蚕蛹⇒ロックソルトフィッシュHQ⇒"),
                new Fish("ダイヤモンドアイ", _fishGrounds["ロッホ・セル湖"], _fishingBates["蚕蛹"], WeatherType.快晴, "(要フィッシュアイ)蚕蛹⇒ロックソルトフィッシュHQ⇒"),
                new Fish("ステタカントゥス", _fishGrounds["ロッホ・セル湖"], _fishingBates["蚕蛹"], 16, 18, WeatherType.雷雨, "(要スカルプター×2、要フィッシュアイ) 蚕蛹⇒ロックソルトフィッシュHQ⇒\n(天候不問、ET16～18、フィッシュアイ不要)蚕蛹⇒ロックソルトフィッシュHQ⇒"),

                // 紅玉海
                new Fish("クアル", _fishGrounds["紅玉台場近海"], _fishingBates["アオイソメ"], 0, 8, WeatherType.雷, WeatherType.曇り),
                new Fish("紅龍", _fishGrounds["紅玉台場近海"], _fishingBates["アオイソメ"], 4, 8, WeatherType.雷, WeatherType.曇り, "アオイソメ⇒クアルHQ⇒(スト)"),
                new Fish("鰭竜", _fishGrounds["獄之蓋近海"], _fishingBates["活海老"], WeatherType.雷, "活海老⇒紅玉海老HQ⇒"),
                new Fish("ウキキ", _fishGrounds["獄之蓋近海"], _fishingBates["アオイソメ"], 8, 12, WeatherType.風),
                new Fish("菜食王", _fishGrounds["獄之蓋近海"], _fishingBates["活海老"], 20, 0, WeatherType.雷, "活海老⇒紅玉海老HQ⇒(スト)"),
                new Fish("オオテンジクザメ", _fishGrounds["ベッコウ島近海"], _fishingBates["アオイソメ"], 10, 18, WeatherType.快晴),
                new Fish("ナナツボシ", _fishGrounds["ベッコウ島近海"], _fishingBates["アオイソメ"], 10, 18, WeatherType.雷, WeatherType.晴れ),
                new Fish("春不知", _fishGrounds["沖之岩近海"], _fishingBates["アオイソメ"], 16, 20),
                new Fish("メカジキ", _fishGrounds["オノコロ島近海"], _fishingBates["アオイソメ"], 8, 12, WeatherType.風 | WeatherType.曇り, "(要フィッシュアイ)"),
                new Fish("ウミダイジャ", _fishGrounds["オノコロ島近海"], _fishingBates["活海老"], WeatherType.風, "活海老⇒紅玉海老HQ"),
                new Fish("ギマ", _fishGrounds["イサリ村沿岸"], _fishingBates["アオイソメ"], 5, 7),
                new Fish("バリマンボン", _fishGrounds["イサリ村沿岸"], _fishingBates["活海老"], 16, 0, WeatherType.快晴 | WeatherType.晴れ, WeatherType.雷),
                new Fish("ソクシツキ", _fishGrounds["ゼッキ島近海"], _fishingBates["活海老"], WeatherType.雷, "活海老⇒紅玉海老HQ"),

                // ヤンサ
                new Fish("ザクロウミ", _fishGrounds["アオサギ池"], _fishingBates["ドバミミズ"], WeatherType.雨),
                new Fish("オニニラミ", _fishGrounds["アオサギ川"], _fishingBates["サスペンドミノー"], WeatherType.晴れ, WeatherType.快晴),
                new Fish("仙寿の翁", _fishGrounds["ナマイ村溜池"], _fishingBates["ザザムシ"], 20, 0, WeatherType.快晴, "※ノゴイが釣れたらトレードリリース"),
                new Fish("シャジクノミ", _fishGrounds["無二江東"], _fishingBates["ザザムシ"], 0, 6, WeatherType.曇り, WeatherType.晴れ),
                new Fish("天女魚", _fishGrounds["無二江西"], _fishingBates["ザザムシ"], 16, 0),
                new Fish("羽衣美女", _fishGrounds["無二江西"], _fishingBates["ザザムシ"], WeatherType.曇り, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("パンダ蝶尾", _fishGrounds["梅泉郷"], _fishingBates["赤虫"], 10, 18, "(要フィッシュアイ)"),
                new Fish("絹鯉", _fishGrounds["梅泉郷"], _fishingBates["ザザムシ"], 4, 8, WeatherType.雨, "(要フィッシュアイ)"),
                new Fish("羽衣鯉", _fishGrounds["梅泉郷"], _fishingBates["ザザムシ"], WeatherType.霧, "(要フィッシュアイ)"),
                new Fish("ドマウナギ", _fishGrounds["七彩渓谷"], _fishingBates["赤虫"], 17, 10),
                new Fish("ニジノヒトスジ", _fishGrounds["七彩渓谷"], _fishingBates["サスペンドミノー"], 12, 16, WeatherType.霧),
                new Fish("紫彩魚", _fishGrounds["七彩溝"], _fishingBates["ザザムシ"], 0, 4),
                new Fish("赤彩魚", _fishGrounds["七彩溝"], _fishingBates["ザザムシ"], 4, 8),
                new Fish("橙彩魚", _fishGrounds["七彩溝"], _fishingBates["ザザムシ"], 4, 8, "ザザムシ⇒赤彩魚HQ⇒"),
                new Fish("藍彩魚", _fishGrounds["七彩溝"], _fishingBates["ザザムシ"], 0, 4, "ザザムシ⇒紫彩魚HQ⇒"),
                new Fish("緑彩魚", _fishGrounds["七彩溝"], _fishingBates["ザザムシ"], 0, 16, WeatherType.晴れ, WeatherType.晴れ | WeatherType.快晴),
                new Fish("七彩天主", _fishGrounds["七彩溝"], _fishingBates["ザザムシ"], 0, 16, WeatherType.晴れ, WeatherType.晴れ | WeatherType.快晴, "(要 藍彩魚×3、ET 00:00～03:59) ザザムシ⇒紫彩魚HQ⇒藍彩魚\n(要 橙彩魚×3, ET 4:00-7:59) ザザムシ⇒赤彩魚HQ⇒橙彩魚\n(要 緑彩魚×5、ET 00:00～15:59、晴れ⇒晴れ/快晴) ザザムシ⇒緑彩魚\n(天候/時間 制限なし) ザザムシ⇒七彩天主\n※ET 00時から藍彩魚・橙彩魚・緑彩魚を釣っておく"),
                new Fish("無二草魚", _fishGrounds["城下船場"], _fishingBates["赤虫"], 20, 4),
                new Fish("水天一碧", _fishGrounds["城下船場"], _fishingBates["ザザムシ"], 16, 0, WeatherType.暴雨),
                new Fish("雷遁魚", _fishGrounds["ドマ城前"], _fishingBates["ザザムシ"], WeatherType.雨 | WeatherType.暴雨),
                new Fish("ボクデン", _fishGrounds["ドマ城前"], _fishingBates["ザザムシ"], 12, 14),

                // アジムステップ
                new Fish("メダカ", new[]{ _fishGrounds["ネム・カール"], _fishGrounds["シロガネ水路"] }, _fishingBates["ザザムシ"], WeatherType.快晴),
                new Fish("ベジースキッパー", _fishGrounds["ネム・カール"], _fishingBates["赤虫"], 8, 12, WeatherType.晴れ, WeatherType.快晴, "(プレ)"),
                new Fish("ハクビターリング", _fishGrounds["ハク・カール"], _fishingBates["ザザムシ"], 0, 4, WeatherType.雨),
                new Fish("明けの旗魚", _fishGrounds["ハク・カール"], _fishingBates["ドバミミズ"], 0, 8, WeatherType.晴れ, WeatherType.霧, "ドバミミズ⇒ザガスHQ⇒(スト)"),
                new Fish("ブレードスキッパー", _fishGrounds["ヤト・カール上流"], _fishingBates["サスペンドミノー"], 4, 8, WeatherType.霧),
                new Fish("グッピー", _fishGrounds["アジム・カート"], _fishingBates["ザザムシ"], 16, 20, WeatherType.晴れ, WeatherType.快晴),
                new Fish("暮れの魚", _fishGrounds["アジム・カート"], _fishingBates["ドバミミズ"], 8, 16, WeatherType.雨 | WeatherType.暴風, WeatherType.曇り, "ドバミミズ⇒ザガスHQ⇒"),
                new Fish("川の長老", _fishGrounds["タオ・カール"], _fishingBates["ドバミミズ"], WeatherType.曇り, WeatherType.風 | WeatherType.霧, "ドバミミス⇒ザガスHQ⇒"),
                new Fish("シンタクヤブリ", _fishGrounds["タオ・カール"], _fishingBates["ザザムシ"], 20, 0),
                new Fish("ヤトカガン", _fishGrounds["ヤト・カール下流"], _fishingBates["ザザムシ"], WeatherType.風),
                new Fish("ナーマの愛籠", _fishGrounds["ドタール・カー"], _fishingBates["サスペンドミノー"], 4, 8, WeatherType.雨, WeatherType.晴れ | WeatherType.快晴),
                new Fish("神々の愛", _fishGrounds["ドタール・カー"], _fishingBates["ザザムシ"], 5, 7, WeatherType.雨, WeatherType.快晴, "(プレ)\n※ナーマの恵みが釣れたらトレードリリース"),

                // クガネ
                new Fish("ノボリリュウ", _fishGrounds["波止場全体"], _fishingBates["活海老"], "活海老⇒紅玉海老HQ"),

                // シロガネ
                new Fish("バクチウチ", _fishGrounds["シロガネ"], _fishingBates["アオイソメ"]),
                new Fish("ヒメダカ", _fishGrounds["シロガネ水路"], _fishingBates["ドバミミズ"], 4, 6),

                // クリスタリウム
                new Fish("ペンダントヘッド", _fishGrounds["クリスタリウム居室"], _fishingBates["蟲箱"], 18, 22, "(プレ)"),

                // ユールモア
                new Fish("グランドデイムバタフライ", _fishGrounds["廃船街"], _fishingBates["イカの切り身"], 12, 19, WeatherType.快晴),

                // レイクランド
                new Fish("プラチナグッピー", _fishGrounds["錆ついた貯水池"], _fishingBates["蟲箱"], WeatherType.快晴),
                new Fish("アンフォーギブン・クラブ", _fishGrounds["始まりの湖"], _fishingBates["蟲箱"], WeatherType.霧),
                new Fish("イモータルジョー", _fishGrounds["ケンの島 (釣り)"], _fishingBates["蟲箱"], 16, 0, WeatherType.晴れ | WeatherType.快晴, WeatherType.曇り | WeatherType.霧),

                // コルシア島
                new Fish("ホワイトロンゾ", _fishGrounds["ワッツリバー下流"], _fishingBates["蟲箱"], 0, 2, "プレ"),
                new Fish("ブロンズソール", _fishGrounds["シャープタンの泉"], _fishingBates["マーブルラーヴァ"], WeatherType.雨),
                new Fish("ヘノドゥス", _fishGrounds["コルシア島沿岸東"], _fishingBates["ショートビルミノー"], 16, 0, WeatherType.曇り | WeatherType.霧, "ショートビルミノー⇒スピアヘッドHQ⇒プレ"),

                // アム・アレーン
                new Fish("カンムリカブリ", _fishGrounds["砂の川"], _fishingBates["オヴィムジャーキー"], 0, 6, WeatherType.砂塵, "オヴィムジャーキー⇒ツノカブリHQ"),
                new Fish("トゲトカゲ", _fishGrounds["アンバーヒル"], _fishingBates["オヴィムジャーキー"], 10, 18, WeatherType.晴れ | WeatherType.快晴 | WeatherType.灼熱波, "デザートフロッグ⇒ミズカキスナヤモリHQ"),
                new Fish("クギトカゲ", _fishGrounds["アンバーヒル"], _fishingBates["デザートフロッグ"], 12, 16, WeatherType.快晴, "デザートフロッグ⇒ミズカキスナヤモリHQ"),

                // イル・メグ
                new Fish("フューリィベタ", _fishGrounds["姿見の湖"], _fishingBates["蟲箱"], 20, 0, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("ピクシーレインボー", _fishGrounds["中の子らの流れ"], _fishingBates["マーブルラーヴァ"], WeatherType.晴れ | WeatherType.快晴, WeatherType.霧, "(プレ)"),
                new Fish("水泡眼", _fishGrounds["コラードの排水溝"], _fishingBates["蟲箱"], WeatherType.快晴),

                // ラケティカ大森林
                new Fish("ロックワの衛士", _fishGrounds["トゥシ・メキタ湖"], _fishingBates["ロバーボール"], 10, 12, "ロバーボール⇒クラウンテトラHQ⇒エリオプスHQ⇒"),
                new Fish("ダイヤモンドピピラ", _fishGrounds["血の酒坏"], _fishingBates["ロバーボール"], 12, 20),
                new Fish("ブラックジェットストリーム", _fishGrounds["ロツァトル川"], _fishingBates["ロバーボール"], 2, 12, WeatherType.曇り, WeatherType.晴れ, "(プレ)"),
                new Fish("常闇魚", _fishGrounds["ウォーヴンオウス"], _fishingBates["ロバーボール"], 0, 8, "(要フィッシュアイ)"),

                // テンペスト
                new Fish("オンドの溜息", _fishGrounds["フラウンダーの穴蔵"], _fishingBates["イカの切り身"], 12, 14, WeatherType.快晴 | WeatherType.晴れ),
                new Fish("アーポアク", _fishGrounds["キャリバンの古巣穴西"], _fishingBates["イカの切り身"], 12, 16, WeatherType.快晴, "(要フィッシュアイ)イカの切り身⇒エンシェントシュリンプHQ⇒(プレ)"),
                new Fish("フードウィンカー", new[] { _fishGrounds["キャリバン海底谷北西"], _fishGrounds["キャリバンの古巣穴東"] }, _fishingBates["ショートビルミノー"], WeatherType.晴れ),
                new Fish("スターチェイサー", _fishGrounds["プルプラ洞"], _fishingBates["イカの切り身"], 6, 10, WeatherType.曇り, "(要フィッシュアイ)"),
            })
            {
                _fishes.Add(fish.Name, fish);
            }
            var maxDifficultyOfFish = _fishes.Max(fish => fish.DifficultyValue);
            var minDifficultyOfFish = _fishes.Min(fish => fish.DifficultyValue);
            var log_maxDifficultyOfFish = Math.Log(maxDifficultyOfFish);
            var log_minDifficultyOfFish = Math.Log(minDifficultyOfFish);
            var width = (log_maxDifficultyOfFish - log_minDifficultyOfFish) / 6;
            foreach (var fish in _fishes)
            {
                if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width)
                    fish.DifficultySymbol = DifficultySymbol.E;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 2)
                    fish.DifficultySymbol = DifficultySymbol.D;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 3)
                    fish.DifficultySymbol = DifficultySymbol.C;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 4)
                    fish.DifficultySymbol = DifficultySymbol.B;
                else if (Math.Log(fish.DifficultyValue) < log_minDifficultyOfFish + width * 5)
                    fish.DifficultySymbol = DifficultySymbol.A;
                else
                    fish.DifficultySymbol = DifficultySymbol.S;
            }
        }

        bool ISettingProvider.GetIsExpandedAreaGroupOnForecastWeather(string areaGroupName)
        {
            return _expandedAreaGroupNames.ContainsKey(areaGroupName);
        }

        bool ISettingProvider.SetIsExpandedAreaGroupOnForecastWeather(string areaGroupName, bool value)
        {
            if (value)
            {
                if (_expandedAreaGroupNames.ContainsKey(areaGroupName))
                    return false;
                else
                    _expandedAreaGroupNames[areaGroupName] = "*";
            }
            else
            {
                if (_expandedAreaGroupNames.ContainsKey(areaGroupName))
                    _expandedAreaGroupNames.Remove(areaGroupName);
                else
                    return false;
            }
            Properties.Settings.Default.ExpandedAreaGroupNames = string.Join("\n", _expandedAreaGroupNames.Keys);
            Properties.Settings.Default.Save();
            return true;
        }

        bool ISettingProvider.GetIsEnabledFishFilter(string fishName)
        {
            return _filteredfishNames.ContainsKey(fishName);
        }

        bool ISettingProvider.SetIsEnabledFishFilter(string fishName, bool value)
        {
            if (value)
            {
                if (_filteredfishNames.ContainsKey(fishName))
                    return false;
                else
                    _filteredfishNames[fishName] = "*";
            }
            else
            {
                if (_filteredfishNames.ContainsKey(fishName))
                    _filteredfishNames.Remove(fishName);
                else
                    return false;
            }
            Properties.Settings.Default.FilteredFishNames = string.Join("\n", _filteredfishNames.Keys);
            Properties.Settings.Default.Save();
            return true;
        }

        bool ISettingProvider.GetIsSelectedMainWindowTab(MainWindowTabType tab)
        {
            return _selectedTabNames.ContainsKey(tab.ToString());
        }

        bool ISettingProvider.SetIsSelectedMainWindowTab(MainWindowTabType tab, bool value)
        {
            var tabName = tab.ToString();
            if (value)
            {
                if (_selectedTabNames.ContainsKey(tabName))
                    return false;
                else
                    _selectedTabNames[tabName] = "*";
            }
            else
            {
                if (_selectedTabNames.ContainsKey(tabName))
                    _selectedTabNames.Remove(tabName);
                else
                    return false;
            }
            Properties.Settings.Default.SelectedTabNames = string.Join("\n", _selectedTabNames.Keys);
            Properties.Settings.Default.Save();
            return true;
        }

        string ISettingProvider.GetFishMemo(string fishName)
        {
            string value;
            if (_fishMemoList.TryGetValue(fishName, out value))
                return value;
            return _fishes[fishName].Memo;
        }

        bool ISettingProvider.SetFishMemo(string fishName, string text)
        {
            if (text == null || _fishes[fishName].Memo == text)
            {
                if (!_fishMemoList.Remove(fishName))
                    return false;
            }
            else
            {
                string currentText;
                if (_fishMemoList.TryGetValue(fishName, out currentText) && currentText == text)
                    return false;
                _fishMemoList[fishName] = text;
            }
            Properties.Settings.Default.FishMemoList = string.Join("\n", _fishMemoList.Select(item => string.Format("{0}\t{1}", item.Key.SimpleEncode(), item.Value.SimpleEncode())));
            Properties.Settings.Default.Save();
            return true;
        }
    }
}
