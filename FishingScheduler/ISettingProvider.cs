namespace FishingScheduler
{
    interface ISettingProvider
    {
        bool GetIsExpandedAreaGroupOnForecastWeather(string areaGroupName);
        bool SetIsExpandedAreaGroupOnForecastWeather(string areaGroupName, bool value);
        bool GetIsEnabledFishFilter(string fishName);
        bool SetIsEnabledFishFilter(string fishName, bool value);
        bool GetIsSelectedMainWindowTab(MainWindowTabType tab);
        bool SetIsSelectedMainWindowTab(MainWindowTabType tab, bool value);
        string GetFishMemo(string fishName);
        bool SetFishMemo(string fishName, string text);
        int GetForecastWeatherDays();
        void SetForecastWeatherDays(int days);
    }
}
