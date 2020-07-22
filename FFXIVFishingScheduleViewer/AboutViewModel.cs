using System;
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
        private bool _isDisposed;
        private ISettingProvider _settingProvider;

        public AboutViewModel(ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            var assembly = typeof(MainWindow).Assembly;
            Product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product;
            Version = assembly.GetName().Version.ToString();
            Company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute))).Company;
            Copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute))).Copyright;
            AboutMenuCommand = null;
        }

        public string Product { get; }
        public string Version { get; }
        public string Company { get; }
        public string Copyright { get; }
        public string AboutMenuText => string.Format(GUITextTranslate.Instance["Menu.About"], Product);
        public string AboutWindowTitleText => string.Format(GUITextTranslate.Instance["Title.About"], Product);
        public string ProductVersionText => string.Format(Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "ProductVersion")], Product, Version);
        public string FFXIVLicenseText => Translate.Instance[new TranslationTextId(TranslationCategory.Generic, "FFXIVLicense")];
        public GUITextTranslate GUIText => GUITextTranslate.Instance;
        public ICommand AboutMenuCommand { get; set; }
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
            RaisePropertyChangedEvent(nameof(AboutMenuText));
            RaisePropertyChangedEvent(nameof(AboutWindowTitleText));
            RaisePropertyChangedEvent(nameof(ProductVersionText));
            RaisePropertyChangedEvent(nameof(FFXIVLicenseText));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}
