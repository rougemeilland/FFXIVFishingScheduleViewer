using FFXIVFishingScheduleViewer.Models;
using System;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class ComboBoxSelectionItemContentViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private Func<string> _textFormatter;
        private ISettingProvider _settingProvider;

        public ComboBoxSelectionItemContentViewModel(Func<string> textFormatter, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _textFormatter = textFormatter;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
        }

        public string Text => _textFormatter();

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
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
