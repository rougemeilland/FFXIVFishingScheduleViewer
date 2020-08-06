using System;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVFishingScheduleViewer
{
    class AboutViewModel
        : ViewModel
    {
        private static Assembly _mainAssembly;
        private bool _isDisposed;
        private string _parentWindowTitle;
        private ISettingProvider _settingProvider;

        static AboutViewModel()
        {
            _mainAssembly = typeof(MainWindow).Assembly;
        }

        public AboutViewModel(string parentWindowTitle, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _parentWindowTitle = parentWindowTitle;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            Product = _settingProvider.ProductName;
            Version = _settingProvider.CurrentVersionOfApplication;
            Company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(_mainAssembly, typeof(AssemblyCompanyAttribute))).Company;
            Copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(_mainAssembly, typeof(AssemblyCopyrightAttribute))).Copyright;
        }

        public string Product { get; }
        public string Version { get; }
        public string Company { get; }
        public string Copyright { get; }
        public string WindowTitleText => string.Format(GUITextTranslate.Instance["Title.AboutWindow"], Product, _parentWindowTitle);
        public string ProductVersionText => string.Format(Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ProductVersion")], Product, Version);
        public string FFXIVLicenseText => Translate.Instance[new TranslationTextId(TranslationCategory.License, "FFXIVLicense")];
        public GUITextTranslate GUIText => GUITextTranslate.Instance;

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
            RaisePropertyChangedEvent(nameof(WindowTitleText));
            RaisePropertyChangedEvent(nameof(ProductVersionText));
            RaisePropertyChangedEvent(nameof(FFXIVLicenseText));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}
