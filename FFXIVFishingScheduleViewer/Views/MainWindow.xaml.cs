using FFXIVFishingScheduleViewer.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FFXIVFishingScheduleViewer.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : WindowBase
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;

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

        protected override void ViewModelChanged(object sender, EventArgs e)
        {
            UpdateTimer();
            base.ViewModelChanged(sender, e);
        }

        internal MainWindowViewModel TypedViewModel
        {
            get => (MainWindowViewModel)ViewModel;
            set => ViewModel = value;
        }


        private void UpdateTimer()
        {
            if (_timer.IsEnabled)
                _timer.Stop();
            TypedViewModel.CurrentTime = DateTime.UtcNow;
            _timer.Interval = TypedViewModel.GetTimerInterval();
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            UpdateTimer();
        }
    }
}
