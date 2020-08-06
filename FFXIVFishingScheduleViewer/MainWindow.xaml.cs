using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : WindowBase
    {
        private ISettingProvider _settingProvider;
        private AreaGroupCollection _areaGroups;
        private FishingSpotCollection _fishingSpots;
        private FishingBaitCollection _fishingBaites;
        private FishCollection _fishes;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            UpgradeSettings();

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            InitializeComponent();

            InitializeData();

            _settingProvider = new SettingProvider(_areaGroups, _fishes);
            TypedViewModel = new MainViewModel(Title, Dispatcher, _areaGroups, _fishes, _settingProvider);
            TypedViewModel.ShowDownloadPageCommand = new SimpleCommand(p =>
            {
                System.Diagnostics.Process.Start(TypedViewModel.UrlOfDownloadPage);
            });
            TypedViewModel.OptionMenuCommand = new SimpleCommand(p1 =>
            {
                var dialog = new OptionWindow
                {
                    Owner = this,
                    TypedViewModel = new OptionViewModel(Title, _fishes, _settingProvider)
                };
                dialog.TypedViewModel.CloseWindowCommand = new SimpleCommand(p2 =>
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                });
                dialog.ShowDialog();
            });
            TypedViewModel.ExitMenuCommand = new SimpleCommand(p => Close());
            TypedViewModel.ViewREADMEMenuCommand = new SimpleCommand(p =>
            {
                System.Diagnostics.Process.Start(Translate.Instance[new TranslationTextId(TranslationCategory.Url, "README")]);
            });
            TypedViewModel.AboutMenuCommand = new SimpleCommand(p =>
            {
                var dialog = new AboutWindow
                {
                    Owner = this,
                    ViewModel = new AboutViewModel(Title, _settingProvider)
                };
                dialog.ShowDialog();
            });
#if DEBUG
            SelfCheck();
#endif
            TypedViewModel.CurrentTime = DateTime.UtcNow;
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            UpdateTimer();

            RecoverWindowBounds();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_timer.IsEnabled)
                _timer.Stop();
            _timer.Tick -= _timer_Tick;
            base.OnClosing(e);
        }

        protected override Point? WindowPositionInSettings
        {
            get
            {
                try
                {
                    return Point.Parse(Properties.Settings.Default.MainWindowPosition);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.MainWindowPosition = value?.ToString() ?? "";
        }

        protected override Size? WindowSizeInSettings
        {
            get
            {
                try
                {
                    return Size.Parse(Properties.Settings.Default.MainWindowSize);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.MainWindowSize = value?.ToString() ?? "";
        }

        protected override WindowState WindowStateInSettings
        {
            get => Properties.Settings.Default.MainWindwIsMaximized ? WindowState.Maximized : WindowState.Normal;
            set => Properties.Settings.Default.MainWindwIsMaximized = value == WindowState.Maximized;
        }

        protected override void SaveWindowSettings()
        {
            Properties.Settings.Default.Save();
        }

        internal MainViewModel TypedViewModel
        {
            get => (MainViewModel)ViewModel;
            set => ViewModel = value;
        }

        private static void UpgradeSettings()
        {
            if (!Properties.Settings.Default.IsUpgraded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();
            }
        }

        private void UpdateTimer()
        {
            var nextTime = (TypedViewModel.CurrentTime.ToEorzeaDateTime().GetStartOfMinute() + EorzeaTimeSpan.FromMinutes(1)).ToEarthDateTime();
            var now = DateTime.UtcNow;
            while (nextTime - now <= TimeSpan.FromSeconds(1))
                nextTime = (nextTime.ToEorzeaDateTime() + EorzeaTimeSpan.FromMinutes(1)).ToEarthDateTime();
            var interval = nextTime - now;
            _timer.Interval = interval;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            TypedViewModel.CurrentTime = DateTime.UtcNow;
            UpdateTimer();
        }

#if DEBUG
        private void SelfCheck()
        {
            var result =
                _fishes
                    .SelectMany(fish => fish.CheckTranslation())
                    .Concat(
                        _areaGroups
                            .SelectMany(areaGroup => areaGroup.CheckTranslation()))
                    .Concat(
                        _areaGroups
                            .SelectMany(areaGroup => areaGroup.Areas)
                            .SelectMany(area => area.CheckTranslation()))
                    .Concat(
                        _areaGroups
                            .SelectMany(areaGroup => areaGroup.Areas)
                            .SelectMany(area => area.FishingSpots)
                            .SelectMany(spot => spot.CheckTranslation()))
                    .Concat(
                        _fishes
                            .SelectMany(fish => fish.FishingConditions.Select(c => c.FishingSpot))
                            .SelectMany(fishingSpot => fishingSpot.CheckTranslation()))
                    .Concat(
                        _fishingBaites
                            .SelectMany(bait => bait.CheckTranslation()))
                    .Concat(
                        Enum.GetValues(typeof(WeatherType))
                        .Cast<WeatherType>()
                        .Where(weather => weather != WeatherType.None)
                        .SelectMany(weather => weather.CheckTranslation()))
                    .ToArray();
            if (result.Any())
                throw new Exception(string.Format("Can't translate: {0}", string.Join(", ", result.Select(s => string.Format("'{0}'", s)))));
            System.Threading.Tasks.Task.Run(() => new FishDataVerifier().GenerateCode(_fishingSpots));
        }
#endif
    }
}
