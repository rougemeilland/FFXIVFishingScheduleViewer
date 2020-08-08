using FFXIVFishingScheduleViewer.Properties;
using FFXIVFishingScheduleViewer.ViewModels;
using FFXIVFishingScheduleViewer.Views;
using System.Windows;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            UpgradeSettings();
            var window = new MainWindow();
            var viewModel = new MainWindowViewModel(window, e.Args);
            window.DataContext = viewModel;
            window.Show();
        }
        private static void UpgradeSettings()
        {
            var settings = Settings.Default;
            if (!settings.IsUpgraded)
            {
                settings.Upgrade();
                settings.IsUpgraded = true;
                settings.Save();
            }
            if (settings.RequestedToClearSettings == true)
            {
                settings.Reset();
                settings.RequestedToClearSettings = false;
                settings.IsUpgraded = true;
                settings.Save();
            }
        }
    }
}
