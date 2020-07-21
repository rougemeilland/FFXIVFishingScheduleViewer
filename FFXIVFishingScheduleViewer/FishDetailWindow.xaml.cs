using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
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

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// FishDetailWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FishDetailWindow : Window
    {
        private FishDetailViewModel _dataContext;

        public FishDetailWindow()
        {
            InitializeComponent();

            _dataContext = null;
            DataContext = _dataContext;

            SetDataContext(DataContext);
            if (_dataContext != null)
                UpdateWindowTitle();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_dataContext != null)
                _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
            base.OnClosed(e);
        }

        private void _dataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_dataContext.GUIText):
                    UpdateWindowTitle();
                    break;
                default:
                    break;
            }
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetDataContext(e.NewValue);
            if (_dataContext != null)
            {
                UpdateWindowTitle();
            }
        }
        private void UpdateWindowTitle()
        {
            Title = string.Format("{0} - {1}", _dataContext.Title, Owner.Title);
        }

        private void SetDataContext(object o)
        {
            if (_dataContext != null)
                _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
            if (o != null && o is FishDetailViewModel)
            {
                _dataContext = (FishDetailViewModel)o;
                _dataContext.PropertyChanged += _dataContext_PropertyChanged;
            }
            else
                _dataContext = null;
        }
    }
}
