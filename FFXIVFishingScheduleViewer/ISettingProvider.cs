using System;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    interface ISettingProvider
    {
        bool GetIsSelectedMainWindowTab(MainWindowTabType tab);
        void SetIsSelectedMainWindowTab(MainWindowTabType tab, bool value);
        event EventHandler<MainWindowTabType> MainWindowTabSelected;
        event EventHandler<MainWindowTabType> MainWindowTabUnselected;

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

        void CheckNewVersionReleased();
        string NewVersionOfApplication { get; }
        string CurrentVersionOfApplication { get; }
        event EventHandler NewVersionOfApplicationChanged;

        string UrlOfDownloadPage { get; }
    }
}
