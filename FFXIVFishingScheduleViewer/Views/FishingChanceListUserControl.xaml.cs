using FFXIVFishingScheduleViewer.Models;
using FFXIVFishingScheduleViewer.Strings;
using FFXIVFishingScheduleViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FFXIVFishingScheduleViewer.Views
{
    /// <summary>
    /// FishingChanceListUserControl.xaml の相互作用ロジック
    /// </summary>
    internal partial class FishingChanceListUserControl
        : UserControlBase, IDisposable
    {
        private bool _isDisposed;
        private Grid _currentTimeIndicatorGrid;
        private IList<Action> _viewModelEventHandlerRemovers;
        private bool _isPendedUpdatingIndicator;
        private bool _isPendedUpdatingChanceListView;
        private BrushConverter _brushConverter;
        private Brush _borderColor;
        private Brush _headerCackgroundColor;
        private Brush _currentTimeIndicatorColor;
        private Brush _whiteBrush;
        private Brush _transparentBrush;
        private Color _whiteColor;

        public FishingChanceListUserControl()
        {
            _isDisposed = false;
            _currentTimeIndicatorGrid = null;
            _viewModelEventHandlerRemovers = new List<Action>();
            _isPendedUpdatingIndicator = false;
            _isPendedUpdatingChanceListView = false;

            InitializeComponent();
            InitializeUserControl();

            _brushConverter = new BrushConverter();
            _borderColor = (Brush)_brushConverter.ConvertFromString("#222222");
            _borderColor.Freeze();
            _headerCackgroundColor = (Brush)_brushConverter.ConvertFromString("#888888");
            _headerCackgroundColor.Freeze();
            _currentTimeIndicatorColor = (Brush)_brushConverter.ConvertFromString("springgreen");
            _currentTimeIndicatorColor.Freeze();
            _whiteBrush = (Brush)_brushConverter.ConvertFromString("white");
            _whiteBrush.Freeze();
            _transparentBrush = new SolidColorBrush(Colors.Transparent);
            _transparentBrush.Freeze();
            _whiteColor = (Color)ColorConverter.ConvertFromString("white");
            UpdateLateFishingChanceListView();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }
                foreach (var action in _viewModelEventHandlerRemovers)
                    action();
                _viewModelEventHandlerRemovers.Clear();
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected override void ViewModelChanged(object sender, EventArgs e)
        {
            UpdateLateFishingChanceListView();
            base.ViewModelChanged(sender, e);
        }

        protected override void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TypedViewModel.CurrentTime):
                    UpdateLateCurrentTimeIndicator();
                    break;
                case nameof(TypedViewModel.FishingChanceList):
                case nameof(TypedViewModel.FishingChanceTimeList):
                case nameof(TypedViewModel.FishingChanceListTextEffect):
                case nameof(TypedViewModel.GUIText):
                    UpdateLateFishingChanceListView();
                    break;
                default:
                    break;
            }
            base.ViewModelPropertyChanged(sender, e);
        }

        internal FishingChanceListViewModel TypedViewModel
        {
            get => (FishingChanceListViewModel)ViewModel;
            set => ViewModel = value;
        }

        private void UpdateLateFishingChanceListView()
        {
            _isPendedUpdatingChanceListView = true;
            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_isPendedUpdatingChanceListView)
                    {
                        UpdateFishingChanceListView();
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

        private void UpdateFishingChanceListView()
        {
            foreach (var action in _viewModelEventHandlerRemovers)
                action();
            _viewModelEventHandlerRemovers.Clear();

            FishChanceGrid.Children.Clear();
            FishChanceGrid.ColumnDefinitions.Clear();
            FishChanceGrid.RowDefinitions.Clear();

            if (!TypedViewModel.FishingChanceList.Any())
            {
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                FishChanceGrid.RowDefinitions.Add(new RowDefinition());
                FishChanceGrid.Children.Add(new TextBlock
                {
                    Text = TypedViewModel.GUIText["Label.NoCheckedFishes"],
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                });
            }
            else
            {
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                FishChanceGrid.ColumnDefinitions.Add(new ColumnDefinition());
                var dataColumnCount = TypedViewModel.ForecastWeatherDays * 24 + 8;
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
                            BorderBrush = _borderColor,
                            BorderThickness = new Thickness(2, 2, 1, 2),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = _headerCackgroundColor,
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
                            BorderBrush = _borderColor,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            BorderThickness = new Thickness(0, 2, 2, 1),
                            Background = _headerCackgroundColor,
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
                            BorderBrush = _borderColor,
                            BorderThickness = new Thickness(0, 0, 2, 2),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = _headerCackgroundColor,
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
                            BorderBrush = _borderColor,
                            BorderThickness = new Thickness(0, 2, 2, 1),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = _headerCackgroundColor,
                        };
                        Grid.SetColumn(c, 2);
                        Grid.SetColumnSpan(c, FishChanceGrid.ColumnDefinitions.Count - 2);
                        Grid.SetRow(c, 0);
                        FishChanceGrid.Children.Add(c);
                    }
                    var currentCulumnIndex = 2;
                    foreach (var time in TypedViewModel.FishingChanceTimeList)
                    {
                        Brush backgroundColor = GetBackgroundColorOfTime(time);
                        {
                            var borderThickness =
                                currentCulumnIndex == FishChanceGrid.ColumnDefinitions.Count - 1
                                    ? new Thickness(0, 0, 1, 2)
                                    : new Thickness(0, 0, 0, 2);
                            var c = new Border
                            {
                                BorderBrush = _borderColor,
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
                            var c = CreateTextBoxWithEffect(time.Hour.ToString(), HorizontalAlignment.Center, TextAlignment.Center);
                            c.HorizontalAlignment = HorizontalAlignment.Stretch;
                            c.VerticalAlignment = VerticalAlignment.Center;
                            c.Background = _transparentBrush;
                            c.Margin = new Thickness(2);
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
                        var time = TypedViewModel.FishingChanceTimeList.Last() + EorzeaTimeSpan.FromHours(1);
                        var c = CreateTextBoxWithEffect(time.Hour.ToString(), HorizontalAlignment.Center, TextAlignment.Center);
                        c.HorizontalAlignment = HorizontalAlignment.Left;
                        c.VerticalAlignment = VerticalAlignment.Center;
                        c.Background = _transparentBrush;
                        c.Margin = new Thickness(2);
                        Grid.SetColumn(c, currentCulumnIndex - 1);
                        Grid.SetRow(c, 1);
                        FishChanceGrid.Children.Add(c);
                    }
                }
                var currentRowIndex = 2;
                foreach (var chance in TypedViewModel.FishingChanceList)
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
                            BorderBrush = _borderColor,
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
                            BorderBrush = _borderColor,
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
                            BorderBrush = _borderColor,
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
                    foreach (var time in TypedViewModel.FishingChanceTimeList)
                    {
                        UIElement c;
                        if (chance.Regions.Contains(time))
                        {
                            c = new Border
                            {
                                BorderBrush = _borderColor,
                                BorderThickness = new Thickness(0, 0, 0, 0),
                                Background = backgroundColorOfFish,
                            };
                        }
                        else
                        {
                            c = new Border
                            {
                                BorderBrush = _borderColor,
                                BorderThickness = new Thickness(0, 0, 0, 0),
                                Background = GetBackgroundColorOfTime(time),
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
                        Background = _transparentBrush,
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
                        BorderBrush = _currentTimeIndicatorColor,
                        BorderThickness = new Thickness(2, 2, 2, 2),
                        Background = _currentTimeIndicatorColor,
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
                foreach (var chance in TypedViewModel.FishingChanceList)
                {
                    var contextMenu = BuildFishContextMenu(chance);
                    var baseDateTime = TypedViewModel.FishingChanceTimeList.Skip(8).First();
                    var forecastWeatherRegion = new EorzeaDateTimeRegion(baseDateTime, EorzeaTimeSpan.FromDays(TypedViewModel.ForecastWeatherDays));
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
                        var eorzeaTimeRegion = firstRegionOfChance.FormatEorzeaTimeRegion(forecastWeatherRegion, TypedViewModel.CurrentTime);
                        var localTimeRegion = firstRegionOfChance.FormatLocalTimeRegion(forecastWeatherRegion, TypedViewModel.CurrentTime);
                        var fishMemo = TypedViewModel.GetFishMemo(chance.FishingCondition.Fish, chance.FishingCondition.FishingSpot);
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
                                : string.Format("\n{0}\n{1}", GUITextTranslate.Instance["Label.Memo"], fishMemo));
                    };
                    var detailTextBlockContainer = CreateTextBoxWithEffect(detailTextBlockFormatter());
                    detailTextBlockContainer.HorizontalAlignment = HorizontalAlignment.Left;
                    detailTextBlockContainer.VerticalAlignment = VerticalAlignment.Center;
                    detailTextBlockContainer.Background = _transparentBrush;
                    detailTextBlockContainer.Margin = new Thickness(5);
                    EventHandler<FishMemoChangedEventArgs> fishMemoChangedEventHandler = new EventHandler<FishMemoChangedEventArgs>((s, e) =>
                    {
                        if (e.Fish == chance.FishingCondition.Fish && e.FishingSpot == chance.FishingCondition.FishingSpot)
                        {
                            var text = detailTextBlockFormatter();
                            foreach (var control in detailTextBlockContainer.Children)
                            {
                                if (control is TextBlock)
                                    ((TextBlock)control).Text = text;
                            }
                        }
                    });
                    TypedViewModel.FishMemoChanged += fishMemoChangedEventHandler;
                    _viewModelEventHandlerRemovers.Insert(0, () => TypedViewModel.FishMemoChanged -= fishMemoChangedEventHandler);
                    var c = new Border
                    {
                        Child = detailTextBlockContainer,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        BorderBrush = _borderColor,
                        BorderThickness = new Thickness(0, 0, 2, 2),
                        Background = _transparentBrush,
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
                foreach (var chance in TypedViewModel.FishingChanceList)
                {
                    var contextMenu = BuildFishContextMenu(chance);
                    var backgroundColorOfFish = chance.FishingCondition.Fish.DifficultySymbol.GetBackgroundColor();
                    var wholeRegion =
                        new EorzeaDateTimeRegion(
                            TypedViewModel.FishingChanceTimeList.First(),
                            EorzeaTimeSpan.FromDays(TypedViewModel.ForecastWeatherDays));
                    foreach (var region in chance.Regions.DateTimeRegions)
                    {
                        var startColumnIndex = (int)(region.Begin - wholeRegion.Begin).EorzeaTimeHours + 2;
                        var columnSpan = (int)region.Span.EorzeaTimeHours;
                        var eorzeaTimeRegion = region.FormatEorzeaTimeRegion(wholeRegion, TypedViewModel.CurrentTime);
                        var localTimeRegion = region.FormatLocalTimeRegion(wholeRegion, TypedViewModel.CurrentTime);
                        var toolTipText =
                            eorzeaTimeRegion != "" && localTimeRegion != ""
                            ? string.Format(
                                "{0}\n{1} {2}\n{3} {4}",
                                chance.FishingCondition.Fish.Name,
                                Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ET.Short")],
                                region.FormatEorzeaTimeRegion(wholeRegion, TypedViewModel.CurrentTime),
                                Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "LT.Short")],
                                region.FormatLocalTimeRegion(wholeRegion, TypedViewModel.CurrentTime))
                            : null;
                        var c = new Border
                        {
                            ToolTip = toolTipText,
                            BorderBrush = _transparentBrush,
                            BorderThickness = new Thickness(0, 0, 0, 0),
                            Background = _transparentBrush,
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

        private Panel CreateTextBoxWithEffect(string text, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left, TextAlignment textAlignment = TextAlignment.Left)
        {
            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
            };
            switch (TypedViewModel.FishingChanceListTextEffect)
            {
                case FishingChanceListTextEffectType.Effect1:
                    return CreateTextBoxWithEffect1(text, horizontalAlignment, textAlignment);
                case FishingChanceListTextEffectType.Effect2:
                    return CreateTextBoxWithEffect2(text, horizontalAlignment, textAlignment);
                case FishingChanceListTextEffectType.Normal:
                default:
                    return CreateTextBoxWithEffectNormal(text, horizontalAlignment, textAlignment);
            }
        }


        private Panel CreateTextBoxWithEffectNormal(string text, HorizontalAlignment horizontalAlignment, TextAlignment textAlignment)
        {
            return
                new Grid { HorizontalAlignment = horizontalAlignment }
                .AddToChildren(
                    new[]
                    {
                        new TextBlock
                        {
                            Text = text,
                            HorizontalAlignment = horizontalAlignment,
                            TextAlignment = textAlignment,
                            Background = _transparentBrush,
                        },
                    });
        }

        private Panel CreateTextBoxWithEffect1(string text, HorizontalAlignment horizontalAlignment, TextAlignment textAlignment)
        {
            var opacity = 0.5;
            var maxCount = 3;
            var scale = 1.0;
            return 
                new Grid { HorizontalAlignment = horizontalAlignment }
                .AddToChildren(
                    new[]
                    {
                        new Grid { HorizontalAlignment = horizontalAlignment, Opacity = opacity }
                            .AddToChildren(
                                Enumerable.Range(-maxCount, 2 * maxCount + 1)
                                .SelectMany(xOffset => Enumerable.Range(-maxCount, 2 * maxCount + 1), (xOffset, yOffset) => new { xOffset, yOffset })
                                .Where(item => item.xOffset != 0 || item.yOffset != 0)
                                .Select(item => new TextBlock
                                {
                                    Text = text,
                                    HorizontalAlignment = horizontalAlignment,
                                    TextAlignment = textAlignment,
                                    Background = _transparentBrush,
                                    Foreground = _whiteBrush,
                                    RenderTransform = new TranslateTransform { X = item.xOffset * scale, Y = item.yOffset * scale },
                                })) as UIElement,
                        new TextBlock
                        {
                            Text = text,
                            HorizontalAlignment = horizontalAlignment,
                            TextAlignment = textAlignment,
                            Background = _transparentBrush,
                        }
                    });
        }

        private Panel CreateTextBoxWithEffect2(string text, HorizontalAlignment horizontalAlignment, TextAlignment textAlignment)
        {
            var opacity = 0.5;
            var padding = 5.0;
            return
                new Grid { HorizontalAlignment = horizontalAlignment }
                .AddToChildren(
                    new[]
                    {
                        new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = horizontalAlignment, Opacity = opacity }
                            .AddToChildren(
                                text.Split('\n')
                                .Select(line => new Border
                                {
                                    Padding = new Thickness(padding),
                                    Margin = new Thickness(-padding),
                                    Background = _whiteBrush,
                                    HorizontalAlignment = horizontalAlignment,
                                    CornerRadius = new CornerRadius(padding),
                                    Child = new TextBlock
                                    {
                                        Text = line,
                                        HorizontalAlignment = horizontalAlignment,
                                        TextAlignment = textAlignment,
                                        Foreground = _whiteBrush,
                                    }
                                })),
                        new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = horizontalAlignment }
                            .AddToChildren(
                                text.Split('\n')
                                .Select(line => new TextBlock
                                {
                                    Text = line,
                                    HorizontalAlignment = horizontalAlignment,
                                    TextAlignment = textAlignment,
                                    Background = _transparentBrush,
                                }))
                    });
        }

        private void UpdateCurrentTimeIndicator()
        {
            if (_currentTimeIndicatorGrid != null)
            {
                var eorzeaTimeNow = TypedViewModel.CurrentTime.ToEorzeaDateTime();
                var firstTime = TypedViewModel.FishingChanceTimeList.First();
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
                    var dialog = new FishDetailWindow
                    {
                        Owner = Window.GetWindow(this),
                        TypedViewModel =
                            TypedViewModel.GetDetailViewModel(
                                chance.FishingCondition.Fish,
                                chance.FishingCondition.FishingSpot)
                    };
                    dialog.TypedViewModel.OKCommand = new SimpleCommand(p =>
                    {
                        foreach (var fishingSpotModel in dialog.TypedViewModel.FishingSpots)
                            TypedViewModel.SetFishMemo(fishingSpotModel.Fish, fishingSpotModel.FishingSpot, fishingSpotModel.Memo.Replace("=>", "⇒"));
                        dialog.Close();
                    });
                    dialog.TypedViewModel.CancelCommand = new SimpleCommand(p =>
                    {
                        dialog.Close();
                    });
                    dialog.ShowDialog();
                });
            contextMenu.Items.Add(new Separator());
            AddContextMenuItem(
                contextMenu,
                string.Format(GUITextTranslate.Instance["Menu.DontShowFish"], chance.FishingCondition.Fish.Name),
                true,
                () =>
                {
                    TypedViewModel.SetFishFilter(chance.FishingCondition.Fish, false);
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

        private Brush GetBackgroundColorOfTime(EorzeaDateTime time)
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
            var backgroundColor = (Brush)_brushConverter.ConvertFromString(backgroundColorName);
            backgroundColor.Freeze();
            return backgroundColor;
        }
    }
}
