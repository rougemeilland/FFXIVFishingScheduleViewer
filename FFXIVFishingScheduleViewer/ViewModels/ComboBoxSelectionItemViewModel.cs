using FFXIVFishingScheduleViewer.Models;
using System;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class ComboBoxSelectionItemViewModel<VALUE_T>
        : ViewModel
    {
        private bool _isDisposed;

        public ComboBoxSelectionItemViewModel(int index, Func<string> textFormatter, VALUE_T value, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            Index = index;
            Content = new ComboBoxSelectionItemContentViewModel(textFormatter, settingProvider);
            Value = value;
        }

        public int Index { get; }
        public ComboBoxSelectionItemContentViewModel Content { get; }
        public VALUE_T Value { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                Content.Dispose();
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
