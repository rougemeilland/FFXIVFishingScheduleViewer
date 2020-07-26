using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FFXIVFishingScheduleViewer
{
    class FishSettingOfAreaGroupViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private AreaGroup _areaGroup;
        private ISettingProvider _settingProvider;

        public FishSettingOfAreaGroupViewModel(AreaGroup areaGroup, IEnumerable<FishSettingOfAreaViewModel> areas, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _areaGroup = areaGroup;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            HasManyFish = areas.SelectMany(area => area.FishingSpots.SelectMany(fishingSpot => fishingSpot.Fishes)).Take(2).Count() > 1;
            Areas = areas.ToArray();
            GUIText = GUITextTranslate.Instance;
            CheckAllFishCommand = new SimpleCommand(p =>
            {
                settingProvider.SetIsEnabledFishFilter(
                    Areas
                        .SelectMany(area => area.FishingSpots)
                        .SelectMany(fishingSpot => fishingSpot.Fishes)
                        .Select(fish => fish.Fish),
                    true);
            });
            UncheckAllFishCommand = new SimpleCommand(p =>
            {
                settingProvider.SetIsEnabledFishFilter(
                    Areas
                        .SelectMany(area => area.FishingSpots)
                        .SelectMany(fishingSpot => fishingSpot.Fishes)
                        .Select(fish => fish.Fish),
                    false);
            });
        }

        public string AreaGroupName => _areaGroup.Name;
        public string CheckAllFishButtonLabel => string.Format(GUIText["ButtonText.CheckAllFishOf"], _areaGroup.Name);
        public string UncheckAllFishButtonLabel => string.Format(GUIText["ButtonText.UncheckAllFishOf"], _areaGroup.Name);
        public bool HasManyFish { get; }
        public IEnumerable<FishSettingOfAreaViewModel> Areas { get; }
        public GUITextTranslate GUIText { get; }

        public ICommand CheckAllFishCommand { get; }
        public ICommand UncheckAllFishCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var area in Areas)
                    area.Dispose();
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, System.EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(AreaGroupName));
            RaisePropertyChangedEvent(nameof(CheckAllFishButtonLabel));
            RaisePropertyChangedEvent(nameof(UncheckAllFishButtonLabel));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}
