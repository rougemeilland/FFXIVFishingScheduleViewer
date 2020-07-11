using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FishingScheduler
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window
    {
        private KeyValueCollection<string, AreaGroup> _areaGroups;
        private KeyValueCollection<string, Fish> _fishes;
        private ISettingProvider _settingProvider;

        // only for designer
        public OptionWindow()
            : this(new KeyValueCollection<string, AreaGroup>(), new KeyValueCollection<string, Fish>(), null)
        {
        }

        internal OptionWindow(KeyValueCollection<string, AreaGroup> areaGroups, KeyValueCollection<string, Fish> fishes, ISettingProvider settingProvider)
        {
            InitializeComponent();

            _areaGroups = areaGroups;

            {
                var forecastPeriodSelectionItems = new[]
                {
                    new { Text = "ET one day (LT 1:10)", Value = 1 },
                    new { Text = "ET 3 days (LT 3:30)", Value = 3 },
                    new { Text = "ET 7 days (LT 8:10)", Value = 7 },
                };

                ForecastPeriodDays.ItemsSource = forecastPeriodSelectionItems;
                ForecastPeriodDays.DisplayMemberPath = "Text";
                ForecastPeriodDays.SelectedValuePath = "Value";
                var found =
                    Enumerable.Range(0, forecastPeriodSelectionItems.Length)
                        .Select(index => new { index, text_value = forecastPeriodSelectionItems[index] })
                        .Where(item => item.text_value.Value == settingProvider.GetForecastWeatherDays())
                        .SingleOrDefault();
                ForecastPeriodDays.SelectedIndex = found == null ? 0 : found.index;
                ForecastPeriodDays.SelectionChanged += (s, e) =>
                {
                    try
                    {
                        settingProvider.SetForecastWeatherDays(forecastPeriodSelectionItems[ForecastPeriodDays.SelectedIndex].Value);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        ForecastPeriodDays.SelectedIndex = 0;
                        settingProvider.SetForecastWeatherDays(0);
                    }
                };
            }

            _fishes = fishes;
            _settingProvider = settingProvider;

            var converter = new BrushConverter();
            var borderColor = (Brush)converter.ConvertFromString("#222222");
            var labelBackgroundColor = (Brush)converter.ConvertFromString("#dddddd");

            var source =
                fishes
                .SelectMany(fish => fish.FishingGrounds, (fish, fishingGround) => new { fish, fishingGround })
                .Select(item => new { item.fish, item.fishingGround, area = item.fishingGround.Area, areaGroup = item.fishingGround.Area.AreaGroup });
            var areaGroupList =
                source
                .GroupBy(item => item.areaGroup)
                .Select(g => new { areaGroup = g.Key, fishCount = g.Count() })
                .OrderBy(item => item.areaGroup.Order)
                .ToArray();
            var areaList =
                source
                .GroupBy(item => item.area)
                .Select(g => new { area = g.Key, areaGroup = g.Key.AreaGroup, fishCount = g.Count() })
                .GroupBy(item => item.areaGroup)
                .Select(g => new
                {
                    areaGroup = g.Key,
                    areas = g
                        .Select(item => new { item.area, item.fishCount } )
                        .OrderBy(item => item.area.Order)
                        .ToArray()
                })
                .ToDictionary(item => item.areaGroup, item => item.areas);
            var fishingGroundList =
                source
                .GroupBy(item => item.fishingGround)
                .Select(g => new { fishingGround = g.Key, fishCount = g.Count() })
                .GroupBy(item => item.fishingGround.Area)
                .Select(g => new
                {
                    area = g.Key,
                    fishingGrounds = g
                        .Select(item => new { item.fishingGround, item.fishCount })
                        .OrderBy(item => item.fishingGround.Order)
                        .ToArray()
                })
                .ToDictionary(item => item.area, item => item.fishingGrounds);
            var fishListOfAreaGroup =
                source
                .GroupBy(item => item.areaGroup)
                .Distinct()
                .Select(g => new
                {
                    areaGroup = g.Key,
                    fishes = g
                        .Select(item => item.fish)
                        .OrderBy(item => item.Name)
                        .ToArray()
                })
                .ToDictionary(item => item.areaGroup, item => item.fishes);
            var fishListOfArea =
                source
                .GroupBy(item => item.area)
                .Select(g => new
                {
                    area = g.Key,
                    fishes = g
                        .Select(item => item.fish)
                        .OrderBy(item => item.Name)
                        .ToArray()
                })
                .ToDictionary(item => item.area, item => item.fishes);
            var fishListOffishingGround =
                source
                .GroupBy(item => item.fishingGround)
                .Select(g => new
                {
                    fishingGround = g.Key,
                    fishes = g
                        .Select(item => item.fish)
                        .OrderBy(item => item.DifficultyValue)
                        .ThenBy(item => item.Name)
                        .ToArray()
                })
                .ToDictionary(item => item.fishingGround, item => item.fishes);

            var fishCheckBoxList = new List<Tuple<Fish, CheckBox>>();
            IDictionary<Fish, CheckBox[]> checkBoxOfFish = null;
            FishListContainer.Items.Clear();
            foreach (var areaGroupInfo in areaGroupList)
            {
                var areaGroupContainer = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
                if (areaGroupInfo.fishCount > 1)
                {
                    var areaGroupHeaderContainer = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Margin = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    {
                        var setAllButton = new Button
                        {
                            Content = string.Format("Check all fish on '{0}'", areaGroupInfo.areaGroup.AreaGroupName),
                            Margin = new Thickness(5),
                            Padding = new Thickness(5),
                        };
                        setAllButton.Click += (s, e) =>
                        {
                            foreach (var fish in fishListOfAreaGroup[areaGroupInfo.areaGroup])
                            {
                                foreach (var checkBox in checkBoxOfFish[fish])
                                {
                                    if (checkBox.IsChecked != true)
                                        checkBox.IsChecked = true;
                                }
                            }
                        };
                        areaGroupHeaderContainer.Children.Add(setAllButton);
                        var clearAllButton = new Button
                        {
                            Content = string.Format("Unheck all fish on '{0}'", areaGroupInfo.areaGroup.AreaGroupName),
                            Margin = new Thickness(5),
                            Padding = new Thickness(5),
                        };
                        clearAllButton.Click += (s, e) =>
                        {
                            foreach (var fish in fishListOfAreaGroup[areaGroupInfo.areaGroup])
                            {
                                foreach (var checkBox in checkBoxOfFish[fish])
                                {
                                    if (checkBox.IsChecked != false)
                                        checkBox.IsChecked = false;
                                }
                            }
                        };
                        areaGroupHeaderContainer.Children.Add(clearAllButton);
                    }
                    areaGroupContainer.Children.Add(areaGroupHeaderContainer);
                }
                var areaGroupBodyContainer = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };
                areaGroupContainer.Children.Add(areaGroupBodyContainer);
                FishListContainer.Items.Add(new TabItem
                {
                    Header = areaGroupInfo.areaGroup.AreaGroupName,
                    Content = new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = areaGroupContainer,
                    },
                });
                foreach (var areaInfo in areaList[areaGroupInfo.areaGroup])
                {
                    var areaContainer = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };
                    if (areaInfo.fishCount > 1)
                    {
                        var buttonBarContainer = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left };
                        areaContainer.Children.Add(buttonBarContainer);
                        {
                            var setAllButton = new Button
                            {
                                Content = string.Format("Check all fish on '{0}'", areaInfo.area.AreaName),
                                Margin = new Thickness(5),
                                Padding = new Thickness(5),
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            setAllButton.Click += (s, e) =>
                            {
                                foreach (var fish in fishListOfArea[areaInfo.area])
                                {
                                    foreach (var checkBox in checkBoxOfFish[fish])
                                        checkBox.IsChecked = true;
                                }
                            };
                            buttonBarContainer.Children.Add(setAllButton);
                            var clearAllButton = new Button
                            {
                                Content = string.Format("Uncheck all fish on '{0}'", areaInfo.area.AreaName),
                                Margin = new Thickness(5),
                                Padding = new Thickness(5),
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            clearAllButton.Click += (s, e) =>
                            {
                                foreach (var fish in fishListOfArea[areaInfo.area])
                                {
                                    foreach (var checkBox in checkBoxOfFish[fish])
                                        checkBox.IsChecked = false;
                                }
                            };
                            buttonBarContainer.Children.Add(clearAllButton);
                        }
                    }
                    areaGroupBodyContainer.Children.Add(new GroupBox
                    {
                        Header = areaInfo.area.AreaName,
                        Content = areaContainer,
                        Margin = new Thickness(5),
                    });
                    foreach (var fishingGroundInfo in fishingGroundList[areaInfo.area])
                    {
                        var fishingGroundContainer = new StackPanel { Orientation = Orientation.Vertical};
                        areaContainer.Children.Add(new GroupBox
                        {
                            Header = fishingGroundInfo.fishingGround.FishingGroundName,
                            Content = fishingGroundContainer,
                            Margin = new Thickness(5),
                        });
                        if (fishingGroundInfo.fishCount > 1)
                        {
                            var buttonBarContainer = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left };
                            fishingGroundContainer.Children.Add(buttonBarContainer);
                            {
                                var setAllButton = new Button
                                {
                                    Content = string.Format("Check all fish on '{0}'", fishingGroundInfo.fishingGround.FishingGroundName),
                                    Margin = new Thickness(5),
                                    Padding = new Thickness(5),
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                setAllButton.Click += (s, e) =>
                                {
                                    foreach (var fish in fishListOffishingGround[fishingGroundInfo.fishingGround])
                                    {
                                        foreach (var checkBox in checkBoxOfFish[fish])
                                            checkBox.IsChecked = true;
                                    }
                                };
                                buttonBarContainer.Children.Add(setAllButton);
                                var clearAllButton = new Button
                                {
                                    Content = string.Format("Uncheck all fish on '{0}'", fishingGroundInfo.fishingGround.FishingGroundName),
                                    Margin = new Thickness(5),
                                    Padding = new Thickness(5),
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                clearAllButton.Click += (s, e) =>
                                {
                                    foreach (var fish in fishListOffishingGround[fishingGroundInfo.fishingGround])
                                    {
                                        foreach (var checkBox in checkBoxOfFish[fish])
                                            checkBox.IsChecked = false;
                                    }
                                };
                                buttonBarContainer.Children.Add(clearAllButton);
                            }
                        }
                        var fishingGroundBodyContainer = new Grid { Margin = new Thickness(5) };
                        fishingGroundContainer.Children.Add(fishingGroundBodyContainer);
                        var rowCount = 0;
                        fishingGroundBodyContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                        fishingGroundBodyContainer.ColumnDefinitions.Add(new ColumnDefinition());
                        foreach (var fish in fishListOffishingGround[fishingGroundInfo.fishingGround])
                        {
                            fishingGroundBodyContainer.RowDefinitions.Add(new RowDefinition());
                            fishingGroundBodyContainer.RowDefinitions.Add(new RowDefinition());
                            var fishNameLabel = new Border
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                BorderBrush = borderColor,
                                BorderThickness = rowCount == 0 ? new Thickness(1, 1, 1, 1) : new Thickness(1, 0, 1, 1),
                                Background = labelBackgroundColor,
                                Child = new TextBlock
                                {
                                    Text = fish.Name,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margin = new Thickness(5),
                                }

                            };
                            Grid.SetColumn(fishNameLabel, 0);
                            Grid.SetRow(fishNameLabel, rowCount);
                            Grid.SetRowSpan(fishNameLabel, 2);
                            fishingGroundBodyContainer.Children.Add(fishNameLabel);
                            var buttonPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                            };
                            var buttonPanelBorder = new Border
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                BorderBrush = borderColor,
                                BorderThickness = rowCount == 0 ? new Thickness(0, 1, 1, 1) : new Thickness(0, 0, 1, 1),
                                Child = buttonPanel
                            };
                            fishingGroundBodyContainer.Children.Add(buttonPanelBorder);
                            Grid.SetColumn(buttonPanelBorder, 1);
                            Grid.SetRow(buttonPanelBorder, rowCount);
                            var checkBox = new CheckBox
                            {
                                Content = "Display chance of this fish",
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(5),
                                IsChecked = settingProvider.GetIsEnabledFishFilter(fish.Name),
                            };
                            checkBox.Checked += (s, e) =>
                            {
                                if (settingProvider.SetIsEnabledFishFilter(fish.Name, true))
                                {
                                    foreach (var c in checkBoxOfFish[fish])
                                        c.IsChecked = true;
                                }
                            };
                            checkBox.Unchecked += (s, e) =>
                            {
                                if (settingProvider.SetIsEnabledFishFilter(fish.Name, false))
                                {
                                    foreach (var c in checkBoxOfFish[fish])
                                        c.IsChecked = false;
                                }
                            };
                            buttonPanel.Children.Add(checkBox);
                            var editMemoButton = new Button { Content = "Edit memo", Margin = new Thickness(5), Padding = new Thickness(5), Visibility = Visibility.Visible };
                            buttonPanel.Children.Add(editMemoButton);
                            var memoPanel = new StackPanel { Orientation = Orientation.Horizontal };
                            var memoPanelBorder = new Border
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                BorderBrush = borderColor,
                                BorderThickness = new Thickness(0, 0, 1, 1),
                                Child = memoPanel
                            };
                            Grid.SetColumn(memoPanelBorder, 1);
                            Grid.SetRow(memoPanelBorder, rowCount + 1);
                            fishingGroundBodyContainer.Children.Add(memoPanelBorder);
                            memoPanel.Children.Add(new TextBlock { Text = "Memo:", Margin = new Thickness(5) });
                            var memoTextPanel = new Grid();
                            memoPanel.Children.Add(memoTextPanel);
                            var memoDisplayPanel = new StackPanel { Orientation = Orientation.Vertical, Visibility = Visibility.Visible };
                            memoTextPanel.Children.Add(memoDisplayPanel);
                            var memoDisplayText = new TextBlock { Text = settingProvider.GetFishMemo(fish.Name), Margin = new Thickness(5) };
                            memoDisplayPanel.Children.Add(memoDisplayText);
                            var memoEditPanel = new StackPanel { Orientation = Orientation.Vertical, Visibility = Visibility.Collapsed };
                            memoTextPanel.Children.Add(memoEditPanel);
                            var memoEditText = new TextBox
                            {
                                Text = settingProvider.GetFishMemo(fish.Name),
                                AcceptsReturn = true,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Margin = new Thickness(5),
                                Padding = new Thickness(5)
                            };
                            memoEditPanel.Children.Add(memoEditText);
                            var memoEditButtonPanel = new StackPanel { Orientation = Orientation.Horizontal };
                            memoEditPanel.Children.Add(memoEditButtonPanel);
                            var memoEditOkButton = new Button { Content = "Save", Margin = new Thickness(5), Padding = new Thickness(5) };
                            memoEditButtonPanel.Children.Add(memoEditOkButton);
                            var memoEditCancelButton = new Button { Content = "Cancel", Margin = new Thickness(5), Padding = new Thickness(5) };
                            memoEditButtonPanel.Children.Add(memoEditCancelButton);
                            var memoEditResetButton = new Button { Content = "Reset", Margin = new Thickness(5), Padding = new Thickness(5) };
                            memoEditButtonPanel.Children.Add(memoEditResetButton);
                            editMemoButton.Click += (s, e) =>
                            {
                                editMemoButton.Visibility = Visibility.Collapsed;
                                memoDisplayPanel.Visibility = Visibility.Collapsed;
                                memoEditPanel.Visibility = Visibility.Visible;
                                memoEditText.Text = _settingProvider.GetFishMemo(fish.Name);
                            };
                            memoEditOkButton.Click += (s, e) =>
                            {
                                editMemoButton.Visibility = Visibility.Visible;
                                memoDisplayPanel.Visibility = Visibility.Visible;
                                memoEditPanel.Visibility = Visibility.Collapsed;
                                _settingProvider.SetFishMemo(fish.Name, memoEditText.Text.Trim());
                                memoDisplayText.Text = _settingProvider.GetFishMemo(fish.Name);
                            };
                            memoEditCancelButton.Click += (s, e) =>
                            {
                                editMemoButton.Visibility = Visibility.Visible;
                                memoDisplayPanel.Visibility = Visibility.Visible;
                                memoEditPanel.Visibility = Visibility.Collapsed;
                                memoDisplayText.Text = _settingProvider.GetFishMemo(fish.Name);
                            };
                            memoEditResetButton.Click += (s, e) =>
                            {
                                editMemoButton.Visibility = Visibility.Visible;
                                memoDisplayPanel.Visibility = Visibility.Visible;
                                memoEditPanel.Visibility = Visibility.Collapsed;
                                _settingProvider.SetFishMemo(fish.Name, null);
                                memoDisplayText.Text = _settingProvider.GetFishMemo(fish.Name);
                            };
                            fishCheckBoxList.Add(new Tuple<Fish, CheckBox>(fish, checkBox));
                            rowCount += 2;
                        }
                    }
                }
            }
            checkBoxOfFish = fishCheckBoxList.GroupBy(item => item.Item1).ToDictionary(g => g.Key, g => g.Select(item => item.Item2).ToArray());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
