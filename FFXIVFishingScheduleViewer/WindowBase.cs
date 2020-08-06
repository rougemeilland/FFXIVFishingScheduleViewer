using System;
using System.ComponentModel;
using System.Windows;

namespace FFXIVFishingScheduleViewer
{
    public class WindowBase
        : Window
    {
        private ViewModel _viewModelCache;

        public WindowBase()
        {
            _viewModelCache = null;
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
            var position = WindowPositionInSettings;
            var size = WindowSizeInSettings;
            var state = WindowStateInSettings;
            if (position.HasValue && size.HasValue)
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
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }
        }

        protected virtual void ViewModelChanged(object sender, EventArgs e)
        {
        }

        protected virtual void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        internal ViewModel ViewModel
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

        private void WindowBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewModel)
                ViewModel = (ViewModel)e.NewValue;
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
