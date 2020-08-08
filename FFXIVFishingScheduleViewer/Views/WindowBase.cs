using FFXIVFishingScheduleViewer.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace FFXIVFishingScheduleViewer.Views
{
    public class WindowBase
        : Window
    {
        private WindowViewModel _viewModelCache;
        private bool _located;

        public WindowBase()
        {
            _viewModelCache = null;
            _located = false;
            DataContextChanged += WindowBase_DataContextChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowBounds();
            if (_viewModelCache != null)
            {
                _viewModelCache.PropertyChanged -= ViewModelPropertyChanged;
                _viewModelCache.Dispose();
                _viewModelCache = null;
            }
            DataContextChanged -= WindowBase_DataContextChanged;
            base.OnClosing(e);
        }

        protected virtual Point? WindowPositionInSettings { get; set; }
        protected virtual Size? WindowSizeInSettings { get; set; }
        protected virtual WindowState WindowStateInSettings { get; set; }

        protected virtual void SaveWindowSettings()
        {
        }

        

        protected void RecoverWindowBounds()
        {
            if (!_located)
            {
                var position = WindowPositionInSettings;
                var size = WindowSizeInSettings;
                var minWidth = ViewModel?.MinWidth ?? double.NaN;
                var minHeight = ViewModel?.MinHeight ?? double.NaN;
                if (!size.HasValue && !double.IsNaN(minWidth) && !double.IsNaN(minHeight))
                    size = new Size(minWidth, minHeight);
                var state = WindowStateInSettings;
                if (size.HasValue)
                {
                    if (position.HasValue)
                    {
                        if (position.Value.X >= 0 &&
                            (position.Value.X + size.Value.Width) < SystemParameters.VirtualScreenWidth)
                        {
                            Left = position.Value.X;
                        }
                        if (position.Value.Y >= 0 &&
                            (position.Value.Y + size.Value.Height) < SystemParameters.VirtualScreenHeight)
                        {
                            Top = position.Value.Y;
                        }
                    }
                    if (size.Value.Width > 0 &&
                        size.Value.Width <= SystemParameters.WorkArea.Width)
                    {
                        Width = size.Value.Width;
                    }
                    if (size.Value.Height > 0 &&
                        size.Value.Height <= SystemParameters.WorkArea.Height)
                    {
                        Height = size.Value.Height;
                    }
                }
                if (state == WindowState.Maximized)
                {
                    Loaded += (o, e) =>
                    {
                        _located = true;
                        WindowState = WindowState.Maximized;
                    };
                }
            }
        }

        protected virtual void ViewModelChanged(object sender, EventArgs e)
        {
            RecoverWindowBounds();
        }

        protected virtual void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.MinWidth):
                case nameof(ViewModel.MinHeight):
                    RecoverWindowBounds();
                    break;
                default:
                    break;
            }
        }

        internal WindowViewModel ViewModel
        {
            get
            {
                if (_viewModelCache != null)
                {
                    return _viewModelCache;
                }
                else if (DataContext is WindowViewModel)
                {
                    _viewModelCache = (WindowViewModel)DataContext;
                    _viewModelCache.PropertyChanged += ViewModelPropertyChanged;
                    return _viewModelCache;
                }
                else
                    return null;
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

        private void WindowBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is WindowViewModel)
                ViewModel = (WindowViewModel)e.NewValue;
            else
                throw new Exception();
        }

        private void SaveWindowBounds()
        {

            WindowState = WindowState.Normal;
            WindowStateInSettings = WindowState;
            WindowPositionInSettings = new Point(Left, Top);
            WindowSizeInSettings = new Size(Width, Height);
            SaveWindowSettings();
        }
    }
}
