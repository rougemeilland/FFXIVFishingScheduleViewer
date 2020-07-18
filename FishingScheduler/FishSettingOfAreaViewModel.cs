using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FishingScheduler
{
    class FishSettingOfAreaViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private Area _area;
        private ISettingProvider _settingProvider;
    
        public FishSettingOfAreaViewModel(Area area, IEnumerable<FishSettingOfFishingSpotViewModel> fishingSpots, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _area = area;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            HasManyFish = fishingSpots.SelectMany(fishingSpot => fishingSpot.Fishes).Take(2).Count() > 1;
            FishingSpots = fishingSpots.ToArray();
            GUIText = GUITextTranslate.Instance;
            CheckAllFishCommand = new SimpleCommand(p =>
            {
                settingProvider.SetIsEnabledFishFilter(
                    FishingSpots
                        .SelectMany(fishingSpot => fishingSpot.Fishes)
                        .Select(fish => fish.Fish),
                    true);
            });
            UncheckAllFishCommand = new SimpleCommand(p =>
            {
                settingProvider.SetIsEnabledFishFilter(
                    FishingSpots
                        .SelectMany(fishingSpot => fishingSpot.Fishes)
                        .Select(fish => fish.Fish),
                    false);
            });
        }

        public string AreaName => _area.Name;
        public string CheckAllFishButtonLabel => string.Format(GUIText["Label.CheckAllFishOf"], _area.Name);
        public string UncheckAllFishButtonLabel => string.Format(GUIText["Label.UncheckAllFishOf"], _area.Name);
        public bool HasManyFish { get; }
        public IEnumerable<FishSettingOfFishingSpotViewModel> FishingSpots { get; }
        public GUITextTranslate GUIText { get; }
        public ICommand CheckAllFishCommand { get; }
        public ICommand UncheckAllFishCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var fishingSpot in FishingSpots)
                    fishingSpot.Dispose();
                _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(AreaName));
            RaisePropertyChangedEvent(nameof(CheckAllFishButtonLabel));
            RaisePropertyChangedEvent(nameof(UncheckAllFishButtonLabel));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}