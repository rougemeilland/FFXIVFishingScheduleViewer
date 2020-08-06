using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FFXIVFishingScheduleViewer
{
    class OptionViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private string _parentWindowTitle;
        private ISettingProvider _settingProvider;

        public OptionViewModel(string parentWindowTitle, FishCollection fishes, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _parentWindowTitle = parentWindowTitle;
            _settingProvider = settingProvider;
            _settingProvider.SelectedOptionCategoryTabIndexChanged += _settingProvider_SelectedOptionCategoryTabIndexChanged;
            _settingProvider.SelectedOptionAreaGroupTabIndexChanged += _settingProvider_SelectedOptionAreaGroupTabIndexChanged;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            _settingProvider.FishingChanceListTextEffectChanged += _settingProvider_FishingChanceListTextEffectChanged;
            _settingProvider.IsEnabledToCheckNewVersionReleasedChanged += _settingProvider_IsEnabledToCheckNewVersionReleasedChanged;
            GUIText = GUITextTranslate.Instance;
            var source =
                fishes
                .SelectMany(fish => fish.FishingConditions)
                .Select(item => new { fish = item.Fish, fishingSpot = item.FishingSpot, area = item.FishingSpot.Area, areaGroup = item.FishingSpot.Area.AreaGroup });
            var areaList =
                source
                .GroupBy(item => item.area)
                .Select(g => new { area = g.Key, areaGroup = g.Key.AreaGroup, fishCount = g.Count() })
                .GroupBy(item => item.areaGroup)
                .Select(g => new
                {
                    areaGroup = g.Key,
                    areas = g
                        .Select(item => new { item.area, item.fishCount })
                        .OrderBy(item => item.area.Order)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.areaGroup, item => item.areas);
            var fishingSpotList =
                source
                .GroupBy(item => item.fishingSpot)
                .Select(g => new { fishingSpot = g.Key, fishCount = g.Count() })
                .GroupBy(item => item.fishingSpot.Area)
                .Select(g => new
                {
                    area = g.Key,
                    fishingSpots = g
                        .Select(item => new { item.fishingSpot, item.fishCount })
                        .OrderBy(item => item.fishingSpot.Order)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.area, item => item.fishingSpots);
            var fishListOfFishingSpot =
                source
                .GroupBy(item => item.fishingSpot)
                .Select(g => new
                {
                    fishingSpot = g.Key,
                    fishes = g
                        .Select(item => item.fish)
                        .OrderBy(item => item.DifficultyValue)
                        .ThenBy(item => item.Order)
                        .ToArray()
                        .AsEnumerable()
                })
                .ToDictionary(item => item.fishingSpot, item => item.fishes);
            FishSettingOfWorld =
                source
                .GroupBy(item => item.areaGroup)
                .Select(g => new { areaGroup = g.Key, fishCount = g.Count() })
                .OrderBy(areaGroupInfo => areaGroupInfo.areaGroup.Order)
                .Select(areaGroupInfo => new FishSettingOfAreaGroupViewModel(
                    areaGroupInfo.areaGroup,
                    areaList[areaGroupInfo.areaGroup]
                        .Select(areaInfo => new FishSettingOfAreaViewModel(
                            areaInfo.area,
                            fishingSpotList[areaInfo.area]
                                .Select(fishingSpotInfo =>
                                {
                                    var fishesOfSpot = fishListOfFishingSpot[fishingSpotInfo.fishingSpot];
                                    var firstFish = fishesOfSpot.First();
                                    var lastFish = fishesOfSpot.Last();
                                    return
                                        new FishSettingOfFishingSpotViewModel(
                                            fishingSpotInfo.fishingSpot,
                                            fishesOfSpot
                                                .Select(fish =>
                                                {
                                                    string cellPositionType;
                                                    if (fish == firstFish)
                                                        cellPositionType = "Top";
                                                    else if (fish == lastFish)
                                                        cellPositionType = "Bottom";
                                                    else
                                                        cellPositionType = "";
                                                    return
                                                        new FishSettingViewModel(
                                                            fish.FishingConditions.Where(c => c.FishingSpot == fishingSpotInfo.fishingSpot).Single(),
                                                            cellPositionType,
                                                            _settingProvider);
                                                }),
                                            _settingProvider);
                                }),
                            _settingProvider)),
                    _settingProvider))
                .ToArray();
        }

        public GUITextTranslate GUIText { get; }
        public string WindowTitleText => string.Format(GUIText["Title.OptionWindow"], _parentWindowTitle);

        public int SelectedOptionCategoryTabIndex
        {
            get => _settingProvider.SelectedOptionCategoryTabIndex;
            set => _settingProvider.SelectedOptionCategoryTabIndex = value;
        }

        public int SelectedOptionAreaGroupTabIndex
        {
            get => _settingProvider.SelectedOptionAreaGroupTabIndex;
            set => _settingProvider.SelectedOptionAreaGroupTabIndex = value;
        }

        public FishingChanceListTextEffectType FishingChanceListTextEffect
        {
            get => _settingProvider.FishingChanceListTextEffect;
            set => _settingProvider.FishingChanceListTextEffect = value;
        }

        public bool IsEnabledToCheckNewVersionReleased
        {
            get => _settingProvider.IsEnabledToCheckNewVersionReleased;
            set => _settingProvider.IsEnabledToCheckNewVersionReleased = value;
        }

        public IEnumerable<FishSettingOfAreaGroupViewModel> FishSettingOfWorld { get; }

        public string UserLanguage
        {
            get => _settingProvider.UserLanguage;
            set => _settingProvider.UserLanguage = value;
        }

        public int ForecastWeatherDays
        {
            get => _settingProvider.ForecastWeatherDays;
            set => _settingProvider.ForecastWeatherDays = value;
        }

        public ICommand CloseWindowCommand { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var item in FishSettingOfWorld)
                    item?.Dispose();
                _settingProvider.SelectedOptionCategoryTabIndexChanged -= _settingProvider_SelectedOptionCategoryTabIndexChanged;
                _settingProvider.SelectedOptionAreaGroupTabIndexChanged -= _settingProvider_SelectedOptionAreaGroupTabIndexChanged;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _settingProvider.FishingChanceListTextEffectChanged -= _settingProvider_FishingChanceListTextEffectChanged;
                _settingProvider.IsEnabledToCheckNewVersionReleasedChanged -= _settingProvider_IsEnabledToCheckNewVersionReleasedChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_SelectedOptionCategoryTabIndexChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedOptionCategoryTabIndex));
        }

        private void _settingProvider_SelectedOptionAreaGroupTabIndexChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedOptionAreaGroupTabIndex));
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(WindowTitleText));
            RaisePropertyChangedEvent(nameof(FishingChanceListTextEffect));
            RaisePropertyChangedEvent(nameof(GUIText));
        }

        private void _settingProvider_IsEnabledToCheckNewVersionReleasedChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(IsEnabledToCheckNewVersionReleased));
        }

        private void _settingProvider_FishingChanceListTextEffectChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishingChanceListTextEffect));
        }

    }
}
