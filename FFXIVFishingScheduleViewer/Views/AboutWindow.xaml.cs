using FFXIVFishingScheduleViewer.ViewModels;
using System.Windows;

namespace FFXIVFishingScheduleViewer.Views
{
    /// <summary>
    /// AboutWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutWindow
        : WindowBase
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        internal AboutWindowViewModel TypedViewModel
        {
            get => (AboutWindowViewModel)ViewModel;
            set => ViewModel = value;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
