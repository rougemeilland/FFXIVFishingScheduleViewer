using FFXIVFishingScheduleViewer.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FFXIVFishingScheduleViewer.Views
{
    public class UserControlBase
        : UserControl
    {
        private ViewModel _viewModelCache;

        public UserControlBase()
        {
            _viewModelCache = null;
        }

        protected void InitializeUserControl()
        {
            Loaded += (s1, e1) =>
            {
                DataContextChanged += UserControlBase_DataContextChanged;
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Closing += (s2, e2) =>
                    {
                        if (_viewModelCache != null)
                        {
                            _viewModelCache.PropertyChanged -= ViewModelPropertyChanged;
                            _viewModelCache.Dispose();
                            _viewModelCache = null;
                        }
                        DataContextChanged -= UserControlBase_DataContextChanged;
                    };
                }
            };
        }

        protected virtual void ViewModelChanged(object sender, EventArgs e)
        {
        }

        protected virtual void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected ViewModel ViewModel
        {
            get
            {
                if (_viewModelCache != null)
                {
                }
                else if (DataContext is ViewModel)
                {
                    _viewModelCache = (ViewModel)DataContext;
                    _viewModelCache.PropertyChanged += ViewModelPropertyChanged;
                }
                else
                    throw new Exception();
                return _viewModelCache;
            }

            set
            {
                if (!ReferenceEquals(_viewModelCache, value))
                {
                    if (_viewModelCache != null)
                    {
                        _viewModelCache.PropertyChanged -= ViewModelPropertyChanged;
                        _viewModelCache.Dispose();
                        _viewModelCache = null;
                    }
                    if (value != null)
                    {
                        _viewModelCache = value;
                        _viewModelCache.PropertyChanged += ViewModelPropertyChanged;
                    }
                    else
                        throw new Exception();
                    DataContext = _viewModelCache;
                    try
                    {
                        ViewModelChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void UserControlBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewModel)
                ViewModel = (ViewModel)e.NewValue;
            else
                throw new Exception();
        }
    }
}
