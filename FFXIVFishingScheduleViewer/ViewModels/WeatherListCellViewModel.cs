using FFXIVFishingScheduleViewer.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class WeatherListCellViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private Func<string> _textFormatter;
        private ISettingProvider _settingProvider;

        public WeatherListCellViewModel()
            : this(null, null)
        {
        }

        public WeatherListCellViewModel(Func<string> textFormatter, ISettingProvider settingProvider)
        {
            _textFormatter = textFormatter;
            _settingProvider = settingProvider;
            if (_settingProvider != null)
                _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            Foreground = Brushes.Black;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Center;
            CellPositionType = "";
        }

        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public string CellPositionType { get; set; }

        public string Text => _textFormatter != null ? _textFormatter() : null;

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (_settingProvider != null)
                    _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(Text));
        }
    }
}
