using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// FishChanceListUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FishChanceListUserControl
        : UserControl, IDisposable
    {
        private bool _isDisposed;
        private DataContext _dataContext;
        private Grid _currentTimeIndicatorGrid;
        private IList<Action> _dataContextEventHandlerRemovers;
        private bool _isPendedUpdatingIndicator;
        private bool _isPendedUpdatingChanceListView;

        public FishChanceListUserControl()
        {
            _isDisposed = false;
            _currentTimeIndicatorGrid = null;
            _dataContextEventHandlerRemovers = new List<Action>();
            _isPendedUpdatingIndicator = false;
            _isPendedUpdatingChanceListView = false;

            InitializeComponent();

            SetDataContext(DataContext);
            if (_dataContext != null)
                UpdateLateFishChanceListView();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }
                foreach (var action in _dataContextEventHandlerRemovers)
                    action();
                _dataContextEventHandlerRemovers.Clear();
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetDataContext(e.NewValue);
            if (_dataContext != null)
            {
                UpdateLateFishChanceListView();
            }
        }

        private void _dataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_dataContext.CurrentTime):
                    UpdateLateCurrentTimeIndicator();
                    break;
                case nameof(_dataContext.FishChanceList):
                case nameof(_dataContext.FishChanceTimeList):
                case nameof(_dataContext.GUIText):
                    UpdateLateFishChanceListView();
                    break;
                default:
                    break;
            }
        }

        private void SetDataContext(object o)
        {
            foreach (var action in _dataContextEventHandlerRemovers)
                action();
            _dataContextEventHandlerRemovers.Clear();
            if (o != null && o is DataContext)
            {
                if (_dataContext != null)
                    _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
                _dataContext = (DataContext)o;
                _dataContext.PropertyChanged += _dataContext_PropertyChanged;
            }
            else
                _dataContext = null;
        }

        private void UpdateLateFishChanceListView()
        {
            _isPendedUpdatingChanceListView = true;
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_isPendedUpdatingChanceListView)
                    {
                        UpdateFishChanceListView();
                        UpdateCurrentTimeIndicator();
                        _isPendedUpdatingChanceListView = false;
                    }
                });
            });
        }

        private void UpdateLateCurrentTimeIndicator()
        {
            _isPendedUpdatingIndicator = true;
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_isPendedUpdatingIndicator)
                    {
                        UpdateCurrentTimeIndicator();
                        _isPendedUpdatingIndicator = false;
                    }
                });
            });
        }

        private void UpdateFishChanceListView()
        {
            foreach (var action in _dataContextEventHandlerRemovers)
                action();
            _dataContextEventHandlerRemovers.Clear();

            FishChanceGrid.Children.Clear();
            FishChanceGrid.ColumnDefinitions.Clear();
            FishChanceGrid.RowDefinitions.Clear();

            if (!_dataContext.FishChanceList.Any())
            {
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                FishChanceGrid.Children.Add(new TextBlock
                {
                    Text = _dataContext.GUIText["Label.NoCheckedFishes"],
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                });
            }
            else
            {
                var converter = new BrushConverter();
                var borderColor = (Brush)converter.ConvertFromString("#222222");
                var headerCackgroundColor = (Brush)converter.ConvertFromString("#888888");
                var currentTimeIndicatorColor = (Brush)converter.ConvertFromString("springgreen");
                var transparentBrush = new SolidColorBrush(Colors.Transparent);
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                var dataColumnCount = _dataContext.ForecastWeatherDays * 24 + 8;
                for (var count = dataColumnCount; count > 0; --count)
                    FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) });
                FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                {
                    {
                        var c = new Border
                        {
                            Child = new TextBlock
                            {
                                Text = GUITextTranslate.Instance["Label.FishName"],
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
                            Child = new TextBlock
                            {
                                Text = GUITextTranslate.Instance["Label.FishingSpot"],
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
                            Child = new TextBlock
                            {
                                Text = GUITextTranslate.Instance["Label.RequiredFishingBaits"],
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
                        var c = new Border
                        {
                            Child = new TextBlock
                            {
                                Text = GUITextTranslate.Instance["Label.EorzeaTime"],
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
                            var borderThickness =
                                currentCulumnIndex == FishChanceGrid.ColumnDefinitions.Count - 1
                                    ? new Thickness(0, 0, 1, 2)
                                    : new Thickness(0, 0, 0, 2);
                            var c = new Border
                            {
                                BorderBrush = borderColor,
                                BorderThickness = borderThickness,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Background = backgroundColor,
                            };
                            Grid.SetColumn(c, currentCulumnIndex);
                            Grid.SetRow(c, 1);
                            FishChanceGrid.Children.Add(c);
                        }
                        if (time.Hour % 4 == 0)
                        {
                            var c = new TextBlock
                            {
                                Text = time.Hour.ToString(),
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center,
                                Background = transparentBrush,
                            };
                            if (currentCulumnIndex <= 2)
                            {
                                Grid.SetColumn(c, currentCulumnIndex);
                                Grid.SetRow(c, 1);
                            }
                            else
                            {
                                Grid.SetColumn(c, currentCulumnIndex - 1);
                                Grid.SetColumnSpan(c, 2);
                                Grid.SetRow(c, 1);
                            }
                            FishChanceGrid.Children.Add(c);
                        }
                        currentCulumnIndex += 1;
                    }
                    {
                        var time = _dataContext.FishChanceTimeList.Last() + EorzeaTimeSpan.FromHours(1);
                        var c = new TextBlock
                        {
                            Text = time.Hour.ToString(),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center,
                            Background = transparentBrush,
                        };
                        Grid.SetColumn(c, currentCulumnIndex - 1);
                        Grid.SetRow(c, 1);
                        FishChanceGrid.Children.Add(c);
                    }
                }
                var currentRowIndex = 2;
                foreach (var chance in _dataContext.FishChanceList)
                {
                    var contextMenu = BuildFishContextMenu(chance);
                    var backgroundColorOfFish = chance.FishingCondition.Fish.DifficultySymbol.GetBackgroundColor();
                    FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                    FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                    {
                        var vStack = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5),
                        };
                        var c = new Border
                        {
                            Child = vStack,
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
                        vStack.Children.Add(new TextBlock
                        {
                            Text = chance.FishingCondition.Fish.Name,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            ToolTip = new ToolTip { Content = GUITextTranslate.Instance["ToolTip.FishName"], },
                        });
                        var hStack = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5),
                            ToolTip = GUITextTranslate.Instance["ToolTip.DiscoveryDifficulty"],
                        };
                        vStack.Children.Add(hStack);
                        hStack.Children.Add(new TextBlock
                        {
                            Text = GUITextTranslate.Instance["Label.DiscoveryDifficulty"],
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        });
                        hStack.Children.Add(new TextBlock
                        {
                            Text = chance.FishingCondition.Fish.DifficultySymbol.GetShortText(),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5, 0, 0, 0),
                        });

                    }
                    {
                        var c = new Border
                        {
                            Child = new TextBlock
                            {
                                Text = string.Format(
                                    "{0} ({1})",
                                    chance.FishingCondition.FishingSpot.Name,
                                    chance.FishingCondition.FishingSpot.Area.Name),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(5),
                                ToolTip = GUITextTranslate.Instance["ToolTip.FishingSpot"],
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
                            Child = new TextBlock
                            {
                                Text = string.Join(", ", chance.FishingCondition.FishingBaits.Select(fishingBait => fishingBait.Name)),
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
                            ToolTip = GUITextTranslate.Instance["ToolTip.FishingBaits"],
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
                    _currentTimeIndicatorGrid = new Grid
                    {
                        Background = transparentBrush,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Visibility = Visibility.Collapsed,
                    };
                    _currentTimeIndicatorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    _currentTimeIndicatorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Pixel) });
                    _currentTimeIndicatorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    _currentTimeIndicatorGrid.RowDefinitions.Add(new RowDefinition());
                    var c = new Border
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
                    Grid.SetRowSpan(_currentTimeIndicatorGrid, FishChanceGrid.RowDefinitions.Count);
                    FishChanceGrid.Children.Add(_currentTimeIndicatorGrid);
                }
                currentRowIndex = 2;
                foreach (var chance in _dataContext.FishChanceList)
                {
                    var contextMenu = BuildFishContextMenu(chance);
                    var now = _dataContext.FishChanceTimeList.Skip(8).First();
                    var forecastWeatherRegion = new EorzeaDateTimeRegion(now, EorzeaTimeSpan.FromDays(_dataContext.ForecastWeatherDays));
                    var firstRegionOfChance =
                        chance.Regions
                            .Intersect(new EorzeaDateTimeHourRegions(new[] { forecastWeatherRegion }))
                            .DateTimeRegions
                            .First();
                    var conditionText =
                        string.Join(
                            ", ",
                            chance.FishingCondition.ConditionElements
                            .Select(element => element.Description)
                            .Where(text => !string.IsNullOrEmpty(text)));
                    Func<string> detailTextBlockFormatter = () =>
                    {
                        var eorzeaTimeRegion = firstRegionOfChance.FormatEorzeaTimeRegion(forecastWeatherRegion);
                        var localTimeRegion = firstRegionOfChance.FormatLocalTimeRegion(forecastWeatherRegion);
                        var fishMemo = _dataContext.GetFishMemo(chance.FishingCondition.Fish, chance.FishingCondition.FishingSpot);
                        return string.Format(
                            "{0}{1}: [{2}]{3}",
                            eorzeaTimeRegion != "" || localTimeRegion != ""
                                ? string.Format(
                                    "{0} {1} ( {2} {3} )\n",
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ET.Short")],
                                    eorzeaTimeRegion,
                                    Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "LT.Short")],
                                    localTimeRegion)
                                : "",
                            GUITextTranslate.Instance["Label.Conditions"],
                            string.IsNullOrEmpty(conditionText)
                                ? GUITextTranslate.Instance["Label.None"]
                                : conditionText,
                            string.IsNullOrEmpty(fishMemo)
                                ? ""
                                : string.Format("\n{0}\n{1}",
                                    GUITextTranslate.Instance["Label.Memo"],
#if true
                                    fishMemo
#else
                                    string.Join("\n",
                                        fishMemo.Split(
                                            "\n".ToCharArray(),
                                            StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => string.Format("- {0}", s)))
#endif
                                    ));
                    };
                    var detailTextBlock = new TextBlock
                    {
                        Text = detailTextBlockFormatter(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = transparentBrush,
                        Margin = new Thickness(5),
                    };
                    EventHandler<FishMemoChangedEventArgs> fishMemoChangedEventHandler = new EventHandler<FishMemoChangedEventArgs>((s, e) =>
                    {
                        if (e.Fish == chance.FishingCondition.Fish && e.FishingSpot == chance.FishingCondition.FishingSpot)
                        {
                            detailTextBlock.Text = detailTextBlockFormatter();
                        }
                    });
                    _dataContext.FishMemoChanged += fishMemoChangedEventHandler;
                    _dataContextEventHandlerRemovers.Insert(0, () => _dataContext.FishMemoChanged -= fishMemoChangedEventHandler);
                    var c = new Border
                    {
                        Child = detailTextBlock,
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
                    var contextMenu = BuildFishContextMenu(chance);
                    var backgroundColorOfFish = chance.FishingCondition.Fish.DifficultySymbol.GetBackgroundColor();
                    var wholeRegion =
                        new EorzeaDateTimeRegion(
                            _dataContext.FishChanceTimeList.First(),
                            EorzeaTimeSpan.FromDays(_dataContext.ForecastWeatherDays));
                    foreach (var region in chance.Regions.DateTimeRegions)
                    {
                        var startColumnIndex = (int)(region.Begin - wholeRegion.Begin).EorzeaTimeHours + 2;
                        var columnSpan = (int)region.Span.EorzeaTimeHours;
                        var eorzeaTimeRegion = region.FormatEorzeaTimeRegion(wholeRegion);
                        var localTimeRegion = region.FormatLocalTimeRegion(wholeRegion);
                        var toolTipText =
                            eorzeaTimeRegion != "" && localTimeRegion != ""
                            ? string.Format(
                                "{0}\nET {1}\nLT {2}",
                                chance.FishingCondition.Fish.Name,
                                region.FormatEorzeaTimeRegion(wholeRegion),
                                region.FormatLocalTimeRegion(wholeRegion))
                            : null;
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
            UpdateCurrentTimeIndicator();
        }

        private void UpdateCurrentTimeIndicator()
        {
            if (_currentTimeIndicatorGrid != null)
            {
                var eorzeaTimeNow = _dataContext.CurrentTime.ToEorzeaDateTime();
                var firstTime = _dataContext.FishChanceTimeList.First();
                var startOfHour = eorzeaTimeNow.GetStartOfHour();
                var columnIndex = (int)(eorzeaTimeNow - firstTime).EorzeaTimeHours;
                var leftWeight = (eorzeaTimeNow - startOfHour).EorzeaTimeSeconds;
                var rightWeight = (startOfHour + EorzeaTimeSpan.FromHours(1) - eorzeaTimeNow).EorzeaTimeSeconds;
                _currentTimeIndicatorGrid.ColumnDefinitions[0].Width = new GridLength(leftWeight, GridUnitType.Star);
                _currentTimeIndicatorGrid.ColumnDefinitions[2].Width = new GridLength(rightWeight, GridUnitType.Star);
                Grid.SetColumn(_currentTimeIndicatorGrid, columnIndex + 2);
                Grid.SetRow(_currentTimeIndicatorGrid, 0);
                _currentTimeIndicatorGrid.Visibility = Visibility.Visible;
            }
        }

        private ContextMenu BuildFishContextMenu(FishChanceTimeRegions chance)
        {
            var contextMenu = new ContextMenu();
            AddContextMenuItem(
                contextMenu,
                string.Format(GUITextTranslate.Instance["Menu.ShowFishDetail"], chance.FishingCondition.Fish.Name),
                true,
                () =>
                {
                    var viewModel =
                        _dataContext.GetDetailViewModel(
                            chance.FishingCondition.Fish,
                            chance.FishingCondition.FishingSpot);
                    var dialog = new FishDetailWindow();
                    viewModel.OKCommand = new SimpleCommand(p =>
                    {
                        foreach (var fishingSpotModel in viewModel.FishingSpots)
                            _dataContext.SetFishMemo(fishingSpotModel.Fish, fishingSpotModel.FishingSpot, fishingSpotModel.Memo.Replace("=>", "⇒"));
                        dialog.Close();
                    });
                    viewModel.CancelCommand = new SimpleCommand(p =>
                    {
                        dialog.Close();
                    });
                    dialog.Owner = Window.GetWindow(this);
                    dialog.DataContext = viewModel;
                    dialog.ShowDialog();
                });
            contextMenu.Items.Add(new Separator());
            AddContextMenuItem(
                contextMenu,
                string.Format(GUITextTranslate.Instance["Menu.DontShowFish"], chance.FishingCondition.Fish.Name),
                true,
                () =>
                {
                    _dataContext.SetFishFilter(chance.FishingCondition.Fish, false);
                });
            contextMenu.Items.Add(new Separator());
            {
                var urlOfFishPageOfCBH = chance.FishingCondition.Fish.GetCBHLink();
                AddContextMenuItem(
                    contextMenu,
                    string.Format(GUITextTranslate.Instance["Menu.ViewPageInCBH"], chance.FishingCondition.Fish.Name),
                    urlOfFishPageOfCBH != null,
                    () =>
                    {
                        System.Diagnostics.Process.Start(urlOfFishPageOfCBH);
                    });
                var urlOfSpotPageOfCBH = chance.FishingCondition.FishingSpot.GetCBHLink();
                AddContextMenuItem(
                    contextMenu,
                    string.Format(GUITextTranslate.Instance["Menu.ViewPageInCBH"], chance.FishingCondition.FishingSpot.Name),
                    urlOfSpotPageOfCBH != null,
                    () =>
                    {
                        System.Diagnostics.Process.Start(urlOfSpotPageOfCBH);
                    });
                foreach (var bait in chance.FishingCondition.FishingBaits)
                {
                    var urlOfBaitPageOfCBH = bait.GetCBHLink();
                    AddContextMenuItem(
                        contextMenu,
                        string.Format(GUITextTranslate.Instance["Menu.ViewPageInCBH"], bait.Name),
                        urlOfBaitPageOfCBH != null,
                        () =>
                        {
                            System.Diagnostics.Process.Start(urlOfBaitPageOfCBH);
                        });
                }
            }
            contextMenu.Items.Add(new Separator());
            {
                var urlOfFishPageOfEDB = chance.FishingCondition.Fish.GetEDBLink();
                AddContextMenuItem(
                    contextMenu,
                    string.Format(GUITextTranslate.Instance["Menu.ViewPageInEDB"], chance.FishingCondition.Fish.Name),
                    urlOfFishPageOfEDB != null,
                    () =>
                    {
                        System.Diagnostics.Process.Start(urlOfFishPageOfEDB);
                    });
                foreach (var bait in chance.FishingCondition.FishingBaits)
                {
                    var urlOfBaitPageOfEDB = bait.GetEDBLink();
                    AddContextMenuItem(
                        contextMenu,
                        string.Format(GUITextTranslate.Instance["Menu.ViewPageInEDB"], bait.Name),
                        urlOfBaitPageOfEDB != null,
                        () =>
                        {
                            System.Diagnostics.Process.Start(urlOfBaitPageOfEDB);
                        });
                }
            }
            contextMenu.Items.Add(new Separator());
            AddContextMenuItem(
                contextMenu,
                GUITextTranslate.Instance["Menu.Cancel"],
                true,
                () => { });
            return contextMenu;
        }

        private void AddContextMenuItem(ContextMenu contextMenu, string header, bool isEnabled, Action action)
        {
            var showDetailMenuItem = new MenuItem { Header = header, IsEnabled = isEnabled };
            contextMenu.Items.Add(showDetailMenuItem);
            showDetailMenuItem.Click += (s, e) => action();
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
    }
}
