using FFXIVFishingScheduleViewer.Models;
using FFXIVFishingScheduleViewer.Strings;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class OptionWindowViewModel
        : WindowViewModel
    {
        private bool _isDisposed;
        private GameData _gameData;
        private ComboBoxSelectionItemViewModel<int>[] _forecastWeatherDaysSelectionItems;
        private ComboBoxSelectionItemViewModel<string>[] _userLanguageSelectionItems;
        private ComboBoxSelectionItemViewModel<FishingChanceListTextEffectType>[] _fishingChanceListTextEffectSelectionItems;

        public OptionWindowViewModel(GameData gameData)
        {
            _isDisposed = false;
            _gameData = gameData;
            _gameData.SettingProvider.SelectedOptionCategoryTabIndexChanged += SettingProvider_SelectedOptionCategoryTabIndexChanged;
            _gameData.SettingProvider.SelectedOptionAreaGroupTabIndexChanged += SettingProvider_SelectedOptionAreaGroupTabIndexChanged;
            _gameData.SettingProvider.ForecastWeatherDaysChanged += SettingProvider_ForecastWeatherDaysChanged;
            _gameData.SettingProvider.UserLanguageChanged += SettingProvider_UserLanguageChanged;
            _gameData.SettingProvider.FishingChanceListTextEffectChanged += SettingProvider_FishingChanceListTextEffectChanged;
            _gameData.SettingProvider.IsEnabledToCheckNewVersionReleasedChanged += SettingProvider_IsEnabledToCheckNewVersionReleasedChanged;
            _gameData.SettingProvider.RequestedToClearSettingsChanged += SettingProvider_RequestedToClearSettingsChanged;
            _forecastWeatherDaysSelectionItems = GetForecastPeriodDaysSelectionItemsSource().ToArray();
            _userLanguageSelectionItems = GetUserLanguageSelectionItemsSource().ToArray();
            _fishingChanceListTextEffectSelectionItems = GetFishingChanceListTextEffectSelectionItemsSource().ToArray();
            var source =
                    _gameData.Fishes
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
                                                            _gameData.SettingProvider);
                                                }),
                                            _gameData.SettingProvider);
                                }),
                            _gameData.SettingProvider)),
                    _gameData.SettingProvider))
                .ToArray();

            MinWidth = 640;
            MinHeight = 480;
        }

        public override string WindowTitleText => GUIText["Title.OptionWindow"];

        public int SelectedOptionCategoryTabIndex
        {
            get => _gameData.SettingProvider.SelectedOptionCategoryTabIndex;
            set => _gameData.SettingProvider.SelectedOptionCategoryTabIndex = value;
        }

        public IEnumerable<ComboBoxSelectionItemViewModel<int>> ForecastWeatherDaysSelectionItems => _forecastWeatherDaysSelectionItems;

        public int SelectedIndexOfForecastWeatherDaysSelectionItems
        {
            get
            {
                var currentValue = _gameData.SettingProvider.ForecastWeatherDays;
                var index= _forecastWeatherDaysSelectionItems.IndexOf(item => item.Value == currentValue);
                return index >= 0 ? index : 0;
            }

            set
            {
                var newSettingValue = _forecastWeatherDaysSelectionItems[value >= 0 && value < _forecastWeatherDaysSelectionItems.Length ? value : 0].Value;
                if (newSettingValue != _gameData.SettingProvider.ForecastWeatherDays)
                    _gameData.SettingProvider.ForecastWeatherDays = newSettingValue;
            }
        }

        public IEnumerable<ComboBoxSelectionItemViewModel<string>> UserLanguageSelectionItems => _userLanguageSelectionItems;

        public int SelectedIndexOfUserLanguageSelectionItems
        {
            get
            {
                var currentValue = _gameData.SettingProvider.UserLanguage;
                int index;
                if ((index = _userLanguageSelectionItems.IndexOf(item => item.Value == currentValue)) >= 0)
                    return index;
                if ((index = _userLanguageSelectionItems.IndexOf(item => item.Value == "en")) >= 0)
                    return index;
                throw new Exception();
            }

            set
            {
                var newSettingValue = value >= 0 && value < _userLanguageSelectionItems.Length ? _userLanguageSelectionItems[value].Value : "en";
                if (newSettingValue != _gameData.SettingProvider.UserLanguage)
                    _gameData.SettingProvider.UserLanguage = newSettingValue;
            }
        }


        public IEnumerable<ComboBoxSelectionItemViewModel<FishingChanceListTextEffectType>> FishingChanceListTextEffectSelectionItems => _fishingChanceListTextEffectSelectionItems;

        public int SelectedIndexOfFishingChanceListTextEffectSelectionItems
        {
            get
            {
                var currentValue = _gameData.SettingProvider.FishingChanceListTextEffect;
                var index = _fishingChanceListTextEffectSelectionItems.IndexOf(item => item.Value == currentValue);
                return index >= 0 ? index : 0;
            }

            set
            {
                var newSettingValue = _fishingChanceListTextEffectSelectionItems[value >= 0 && value < _fishingChanceListTextEffectSelectionItems.Length ? value : 0].Value;
                if (newSettingValue != _gameData.SettingProvider.FishingChanceListTextEffect)
                    _gameData.SettingProvider.FishingChanceListTextEffect = newSettingValue;
            }
        }

        public int SelectedOptionAreaGroupTabIndex
        {
            get => _gameData.SettingProvider.SelectedOptionAreaGroupTabIndex;
            set => _gameData.SettingProvider.SelectedOptionAreaGroupTabIndex = value;
        }

        public IEnumerable<FishSettingOfAreaGroupViewModel> FishSettingOfWorld { get; }

        public bool IsEnabledToCheckNewVersionReleased
        {
            get => _gameData.SettingProvider.IsEnabledToCheckNewVersionReleased;
            set => _gameData.SettingProvider.IsEnabledToCheckNewVersionReleased = value;
        }


        public bool RequestedToClearSettings
        {
            get => _gameData.SettingProvider.RequestedToClearSettings;
            set => _gameData.SettingProvider.RequestedToClearSettings = value;
        }

        public bool IsDeveloperMode
        {
            get
            {
                System.Diagnostics.Debug.WriteLine(string.Format("'{0}'={1}", "IsDeveloperMode", _gameData.IsDeveloperMode));
                return _gameData.IsDeveloperMode;
            }
        }

        public ICommand CloseWindowCommand { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var item in FishSettingOfWorld)
                    item?.Dispose();
                _gameData.SettingProvider.SelectedOptionCategoryTabIndexChanged -= SettingProvider_SelectedOptionCategoryTabIndexChanged;
                _gameData.SettingProvider.SelectedOptionAreaGroupTabIndexChanged -= SettingProvider_SelectedOptionAreaGroupTabIndexChanged;
                _gameData.SettingProvider.ForecastWeatherDaysChanged -= SettingProvider_ForecastWeatherDaysChanged;
                _gameData.SettingProvider.UserLanguageChanged -= SettingProvider_UserLanguageChanged;
                _gameData.SettingProvider.FishingChanceListTextEffectChanged -= SettingProvider_FishingChanceListTextEffectChanged;
                _gameData.SettingProvider.IsEnabledToCheckNewVersionReleasedChanged -= SettingProvider_IsEnabledToCheckNewVersionReleasedChanged;
                _gameData.SettingProvider.RequestedToClearSettingsChanged -= SettingProvider_RequestedToClearSettingsChanged;
                foreach (var item in _forecastWeatherDaysSelectionItems)
                    item?.Dispose();
                foreach (var item in _userLanguageSelectionItems)
                    item?.Dispose();
                foreach (var item in _fishingChanceListTextEffectSelectionItems)
                    item?.Dispose();
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void SettingProvider_SelectedOptionCategoryTabIndexChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedOptionCategoryTabIndex));
        }

        private void SettingProvider_SelectedOptionAreaGroupTabIndexChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedOptionAreaGroupTabIndex));
        }

        private void SettingProvider_ForecastWeatherDaysChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedIndexOfForecastWeatherDaysSelectionItems));
        }

        private void SettingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedIndexOfFishingChanceListTextEffectSelectionItems));
            RaisePropertyChangedEvent(nameof(WindowTitleText));
            RaisePropertyChangedEvent(nameof(GUIText));
        }

        private void SettingProvider_FishingChanceListTextEffectChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(SelectedIndexOfFishingChanceListTextEffectSelectionItems));
        }

        private void SettingProvider_IsEnabledToCheckNewVersionReleasedChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(IsEnabledToCheckNewVersionReleased));
        }

        private void SettingProvider_RequestedToClearSettingsChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(RequestedToClearSettings));
        }

        private IEnumerable<ComboBoxSelectionItemViewModel<int>> GetForecastPeriodDaysSelectionItemsSource()
        {
            return new[]
            {
                new { formatter = new Func<string>(() => GUIText["Label.1Days"]), value = 1 },
                new { formatter = new Func<string>(() => GUIText["Label.3Days"]), value = 3 },
                new { formatter = new Func<string>(() => GUIText["Label.7Days"]), value = 7 },
            }
            .Select((item, index) => new ComboBoxSelectionItemViewModel<int>(index, item.formatter, item.value, _gameData.SettingProvider));
        }

        private IEnumerable<ComboBoxSelectionItemViewModel<string>> GetUserLanguageSelectionItemsSource()
        {
            return new[]
            {
                 new
                 {
                     formatter = new Func<string>(
                         () =>
                            string.Format(
                                "{0} ({1})",
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "日本語"), "ja"],
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "日本語")])),
                     value = "ja",
                 },
                 new
                 {
                     formatter = new Func<string>(
                         () =>
                            string.Format(
                                "{0} ({1})",
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "英語"), "en"],
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "英語")])),
                     value = "en",
                 },
                 new
                 {
                     formatter = new Func<string>(
                         () =>
                            string.Format(
                                "{0} ({1})",
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "フランス語"), "fr"],
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "フランス語")])),
                     value = "fr",
                 },
                 new {
                     formatter = new Func<string>(
                         () =>
                            string.Format(
                                "{0} ({1})",
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "ドイツ語"), "de"],
                                Translate.Instance[new TranslationTextId(TranslationCategory.Language, "ドイツ語")])),
                     value = "de",
                 },
            }
            .Select((item, index) => new ComboBoxSelectionItemViewModel<string>(index, item.formatter, item.value, _gameData.SettingProvider));
        }

        private IEnumerable<ComboBoxSelectionItemViewModel<FishingChanceListTextEffectType>> GetFishingChanceListTextEffectSelectionItemsSource()
        {
            return new[]
            {
                new { formatter = new Func<string>(() => GUIText["Label.EffectNormal"]), value = FishingChanceListTextEffectType.Normal },
                new { formatter = new Func<string>(() => GUIText["Label.Effect1"]), value = FishingChanceListTextEffectType.Effect1 },
                new { formatter = new Func<string>(() => GUIText["Label.Effect2"]), value = FishingChanceListTextEffectType.Effect2 },
            }
            .Select((item, index) => new ComboBoxSelectionItemViewModel<FishingChanceListTextEffectType>(index, item.formatter, item.value, _gameData.SettingProvider));
        }
    }
}
