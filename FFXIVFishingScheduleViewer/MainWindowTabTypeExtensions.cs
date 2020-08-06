namespace FFXIVFishingScheduleViewer
{
    static class MainWindowTabTypeExtensions
    {
        public static string ToInternalId(this MainWindowTabType value, string defaultValue)
        {
            switch (value)
            {
                case MainWindowTabType.ForecastWeather:
                    return "forecastWeather";
                case MainWindowTabType.Chance:
                    return "chance";
                default:
                    return defaultValue;
            }
        }

        public static MainWindowTabType ToMainWindowTab(this string value, MainWindowTabType defaultValue)
        {
            switch (value)
            {
                case "forecastWeather":
                    return MainWindowTabType.ForecastWeather;
                case "chance":
                    return MainWindowTabType.Chance;
                default:
                    return defaultValue;
            }
        }
    }
}
