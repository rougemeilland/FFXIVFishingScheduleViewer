using System;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer.Models
{
    interface ISettingProvider
    {
        int SelectedMainViewTabIndex { get; set; }
        event EventHandler SelectedMainViewTabIndexChanged;

        int SelectedOptionCategoryTabIndex { get; set; }
        event EventHandler SelectedOptionCategoryTabIndexChanged;

        int SelectedOptionAreaGroupTabIndex { get; set; }
        event EventHandler SelectedOptionAreaGroupTabIndexChanged;

        bool GetIsExpandedAreaGroupOnForecastWeather(AreaGroup areaGroup);
        void SetIsExpandedAreaGroupOnForecastWeather(AreaGroup areaGroup, bool value);
        event EventHandler<AreaGroup> AreaGroupOnForecastWeatherExpanded;
        event EventHandler<AreaGroup> AreaGroupOnForecastWeatherContracted;

        bool GetIsEnabledFishFilter(Fish fish);
        void SetIsEnabledFishFilter(Fish fish, bool value);
        void SetIsEnabledFishFilter(IEnumerable<Fish> fishes, bool value);
        event EventHandler<Fish> FishFilterChanded;

        string GetFishMemo(Fish fish, FishingSpot fishingSpot);
        void SetFishMemo(Fish fish, FishingSpot fishingSpot, string text);
        event EventHandler<FishMemoChangedEventArgs> FishMemoChanged;

        int ForecastWeatherDays { get; set; }
        event EventHandler ForecastWeatherDaysChanged;

        string UserLanguage { get; set; }
        event EventHandler UserLanguageChanged;

        bool IsEnabledToCheckNewVersionReleased { get; set; }
        event EventHandler IsEnabledToCheckNewVersionReleasedChanged;

        FishingChanceListTextEffectType FishingChanceListTextEffect { get; set; }
        event EventHandler FishingChanceListTextEffectChanged;

        bool RequestedToClearSettings { get; set; }
        event EventHandler RequestedToClearSettingsChanged;
    }
}
