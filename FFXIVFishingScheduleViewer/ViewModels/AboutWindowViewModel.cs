using FFXIVFishingScheduleViewer.Models;
using FFXIVFishingScheduleViewer.Strings;
using System;
using System.Reflection;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class AboutWindowViewModel
        : WindowViewModel
    {
        private static Assembly _mainAssembly;
        private bool _isDisposed;
        private GameData _gameData;

        static AboutWindowViewModel()
        {
            _mainAssembly = typeof(AboutWindowViewModel).Assembly;
        }

        public AboutWindowViewModel(GameData gameData)
        {
            _isDisposed = false;
            _gameData = gameData;
            _gameData.SettingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            Product = _gameData.ProductName;
            Version = _gameData.CurrentVersionOfApplication;
            Company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(_mainAssembly, typeof(AssemblyCompanyAttribute))).Company;
            Copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(_mainAssembly, typeof(AssemblyCopyrightAttribute))).Copyright;
            MaxWidth = 480;
        }

        public string Product { get; }
        public string Version { get; }
        public string Company { get; }
        public string Copyright { get; }
        public override string WindowTitleText => string.Format(GUITextTranslate.Instance["Title.AboutWindow"], Product);
        public string ProductVersionText => string.Format(Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ProductVersion")], Product, Version);
        public string FFXIVLicenseText => Translate.Instance[new TranslationTextId(TranslationCategory.License, "FFXIVLicense")];

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _gameData.SettingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(WindowTitleText));
            RaisePropertyChangedEvent(nameof(ProductVersionText));
            RaisePropertyChangedEvent(nameof(FFXIVLicenseText));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}
