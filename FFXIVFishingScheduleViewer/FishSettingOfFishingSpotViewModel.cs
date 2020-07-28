using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FFXIVFishingScheduleViewer
{
    class FishSettingOfFishingSpotViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private FishingSpot _fishingSpot;
        private ISettingProvider _settingProvider;
        private ICollection<MenuItemViewModel> _menuItems;

        public FishSettingOfFishingSpotViewModel(FishingSpot fishingSpot, IEnumerable<FishSettingViewModel> fishes, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _fishingSpot = fishingSpot;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            HasManyFish = fishes.Take(2).Count() > 1;
            Fishes = fishes.ToArray();
            _menuItems = new List<MenuItemViewModel>();
            _menuItems.Add(MenuItemViewModel.CreateShowSpotInCBHMenuItem(fishingSpot));
            _menuItems.Add(MenuItemViewModel.CreateSeparatorMenuItem());
            _menuItems.Add(MenuItemViewModel.CreateCancelMenuItem());
            GUIText = GUITextTranslate.Instance;
            CheckAllFishCommand = new SimpleCommand(p =>
            {
                settingProvider.SetIsEnabledFishFilter(
                    Fishes
                        .Select(fish => fish.Fish),
                    true);
            });
            UncheckAllFishCommand = new SimpleCommand(p =>
            {
                settingProvider.SetIsEnabledFishFilter(
                    Fishes
                        .Select(fish => fish.Fish),
                    false);
            });
        }

        public string FishingSpotName => _fishingSpot.Name;
        public bool HasManyFish { get; }
        public IEnumerable<FishSettingViewModel> Fishes { get; }
        public IEnumerable<MenuItemViewModel> ContextMenuItems => _menuItems;
        public string CheckAllFishButtonLabel => string.Format(GUIText["ButtonText.CheckAllFishOf"], _fishingSpot.Name);
        public string UncheckAllFishButtonLabel => string.Format(GUIText["ButtonText.UncheckAllFishOf"], _fishingSpot.Name);
        public GUITextTranslate GUIText { get; }
        public ICommand CheckAllFishCommand { get; }
        public ICommand UncheckAllFishCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var item in Fishes)
                    item.Dispose();
                foreach (var item in _menuItems)
                    item.Dispose();
                _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, System.EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishingSpotName));
            RaisePropertyChangedEvent(nameof(ContextMenuItems));
            RaisePropertyChangedEvent(nameof(CheckAllFishButtonLabel));
            RaisePropertyChangedEvent(nameof(UncheckAllFishButtonLabel));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}
