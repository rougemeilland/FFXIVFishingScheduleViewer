namespace FishingScheduler
{
    class WeatherViewModel
        : ViewModel
    {
        public WeatherViewModel(string text, System.Windows.Media.Brush foreground, System.Windows.Media.Brush background)
        {
            Text = text;
            Foreground = foreground;
            Background = background;
        }

        public string Text { get; }
        public System.Windows.Media.Brush Foreground { get; }
        public System.Windows.Media.Brush Background { get; }
    }
}
