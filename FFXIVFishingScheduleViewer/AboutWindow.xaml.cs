using System;
using System.ComponentModel;
using System.Windows;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// AboutWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutWindow : Window
    {
        private AboutViewModel _dataContext;

        public AboutWindow()
        {
            InitializeComponent();

            _dataContext = null;
            DataContext = _dataContext;

            SetDataContext(DataContext);
            if (_dataContext != null)
            {
                UpdateWindowTitle();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_dataContext != null)
                _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
            base.OnClosed(e);
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetDataContext(e.NewValue);
            if (_dataContext != null)
            {
                UpdateWindowTitle();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void _dataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_dataContext.AboutWindowTitleText):
                    UpdateWindowTitle();
                    break;
                default:
                    break;
            }
        }

        private void SetDataContext(object o)
        {
            if (_dataContext != null)
                _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
            if (o != null && o is AboutViewModel)
            {
                _dataContext = (AboutViewModel)o;
                _dataContext.PropertyChanged += _dataContext_PropertyChanged;
            }
            else
                _dataContext = null;
        }

        private void UpdateWindowTitle()
        {
            Title = string.Format("{0} - {1}", _dataContext.AboutWindowTitleText, Owner.Title);
        }

    }
}
