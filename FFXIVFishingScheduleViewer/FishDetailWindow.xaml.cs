using System;
using System.Windows;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// FishDetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FishDetailWindow
        : WindowBase
    {
        public FishDetailWindow()
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
                    return Point.Parse(Properties.Settings.Default.DetailWindowPosition);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.DetailWindowPosition = value?.ToString() ?? "";
        }

        protected override Size? WindowSizeInSettings
        {
            get
            {
                try
                {
                    return Size.Parse(Properties.Settings.Default.DetailWindowSize);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.DetailWindowSize = value?.ToString() ?? "";
        }

        protected override WindowState WindowStateInSettings
        {
            get => Properties.Settings.Default.DetailWindwIsMaximized ? WindowState.Maximized : WindowState.Normal;
            set => Properties.Settings.Default.DetailWindwIsMaximized = value == WindowState.Maximized;
        }

        protected override void SaveWindowSettings()
        {
            Properties.Settings.Default.Save();
        }

        internal FishDetailViewModel TypedViewModel
        {
            get => (FishDetailViewModel)ViewModel;
            set => ViewModel = value;
        }


    }
}
