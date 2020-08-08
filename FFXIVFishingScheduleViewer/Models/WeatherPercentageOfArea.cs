namespace FFXIVFishingScheduleViewer.Models
{
    class WeatherPercentageOfArea
    {
        public WeatherPercentageOfArea(WeatherType weather, int percentage)
        {
            Weather = weather;
            Percentage = percentage;
        }

        public WeatherType Weather { get; }
        public int Percentage { get; }
    }
}
