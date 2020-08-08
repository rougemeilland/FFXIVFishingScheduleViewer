using FFXIVFishingScheduleViewer.ViewModels;
using System;
using System.Windows;

namespace FFXIVFishingScheduleViewer.Views
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow
        : WindowBase
    {
        public OptionWindow()
        {
            InitializeComponent();

            RecoverWindowBounds();
        }

        protected override Point? WindowPositionInSettings
        {
            get
            {
                try
                {
                    return Point.Parse(Properties.Settings.Default.OptionWindowPosition);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.OptionWindowPosition = value?.ToString() ?? "";
        }

        protected override Size? WindowSizeInSettings
        {
            get
            {
                try
                {
                    return Size.Parse(Properties.Settings.Default.OptionWindowSize);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.OptionWindowSize = value?.ToString() ?? "";
        }

        protected override WindowState WindowStateInSettings
        {
            get => Properties.Settings.Default.OptionWindwIsMaximized ? WindowState.Maximized : WindowState.Normal;
            set => Properties.Settings.Default.OptionWindwIsMaximized = value == WindowState.Maximized;
        }

        protected override void SaveWindowSettings()
        {
            Properties.Settings.Default.Save();
        }

        internal OptionWindowViewModel TypedViewModel
        {
            get => (OptionWindowViewModel)ViewModel;
            set => ViewModel = value;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
