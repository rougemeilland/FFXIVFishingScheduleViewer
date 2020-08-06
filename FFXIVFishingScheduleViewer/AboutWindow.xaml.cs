using System.Windows;

namespace FFXIVFishingScheduleViewer
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

        internal AboutViewModel TypedViewModel
        {
            get => (AboutViewModel)ViewModel;
            set => ViewModel = value;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
