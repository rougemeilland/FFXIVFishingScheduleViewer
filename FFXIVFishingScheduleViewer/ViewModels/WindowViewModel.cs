namespace FFXIVFishingScheduleViewer.ViewModels
{
    abstract class WindowViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private double _minWidth;
        private double _maxWidth;
        private double _minHeight;
        private double _maxHeight;

        public WindowViewModel()
        {
            _isDisposed = false;
            _minWidth = 0.0;
            _maxWidth = double.PositiveInfinity;
            _minHeight = 0.0;
            _maxHeight = double.PositiveInfinity;
        }

        public abstract string WindowTitleText { get; }

        public double MinWidth
        {
            get => _minWidth;

            set
            {
                if (value != _minWidth)
                {
                    _minWidth = value;
                    RaisePropertyChangedEvent(nameof(MinWidth));
                }
            }
        }

        public double MaxWidth
        {
            get => _maxWidth;

            set
            {
                if (value != _maxWidth)
                {
                    _maxWidth = value;
                    RaisePropertyChangedEvent(nameof(MaxWidth));
                }
            }
        }

        public double MinHeight
        {
            get => _minHeight;

            set
            {
                if (value != _minHeight)
                {
                    _minHeight = value;
                    RaisePropertyChangedEvent(nameof(MinHeight));
                }
            }
        }

        public double MaxHeight
        {
            get => _maxHeight;

            set
            {
                if (value != _maxHeight)
                {
                    _maxHeight = value;
                    RaisePropertyChangedEvent(nameof(MaxHeight));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
