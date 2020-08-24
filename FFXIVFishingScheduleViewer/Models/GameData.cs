using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVFishingScheduleViewer.Models
{
    partial class GameData
    {
        private static Regex _locationOfReleasePattern = new Regex(@"^*./releases/tag/v(?<version>[0-9\.]+)$", RegexOptions.Compiled);
        private static Regex _versionTextPattern = new Regex(@"^(?<version>[0-9\.]+)$", RegexOptions.Compiled);
        private string _newVersionOfApplication;

        public GameData(string[] args)
        {
            _newVersionOfApplication = null;
            var m = _versionTextPattern.Match(GetType().Assembly.GetName().Version.ToString());
            CurrentVersionOfApplication = m.Success ? m.Groups["version"].Value : null;
            AreaGroups = new AreaGroupCollection();
            FishingSpots = new FishingSpotCollection();
            FishingBaits = new FishingBaitCollection();
            Fishes = new FishCollection();
            ProductName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(GetType().Assembly, typeof(AssemblyProductAttribute))).Product;
            IsDeveloperMode = args.Any(arg => arg == "--developerMode");

            InitializeData();

            SettingProvider = new SettingProvider(Fishes);
#if DEBUG
            SelfCheck();
#endif
        }

        public string ProductName { get; }

        public void CheckNewVersionReleased()
        {
            if (Properties.Settings.Default.IsEnabledToCheckNewVersionReleased && CurrentVersionOfApplication != null)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        using (var client = new HttpClient())
                        using (var res = await client.GetAsync(Properties.Settings.Default.UrlOfDownloadPage))
                        {
                            res.EnsureSuccessStatusCode();
                            var content = await res.Content.ReadAsStringAsync();
                            var url = res.RequestMessage.RequestUri;
                            var m = _locationOfReleasePattern.Match(url.AbsoluteUri);
                            if (m.Success)
                            {
                                var newVersionText = m.Groups["version"].Value;
                                NewVersionOfApplication = newVersionText.CompareVersionString(CurrentVersionOfApplication) > 0 ? newVersionText : null;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }

        public string NewVersionOfApplication
        {
            get => _newVersionOfApplication;

            private set
            {
                if (value != _newVersionOfApplication)
                {
                    _newVersionOfApplication = value;
                    try
                    {
                        NewVersionOfApplicationChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public string CurrentVersionOfApplication { get; }
        public event EventHandler NewVersionOfApplicationChanged;
        public string UrlOfDownloadPage => Properties.Settings.Default.UrlOfDownloadPage;
        public AreaGroupCollection AreaGroups { get; }
        public FishingSpotCollection FishingSpots { get; }
        public FishingBaitCollection FishingBaits { get; }
        public FishCollection Fishes { get; }
        public ISettingProvider SettingProvider { get; }
        public bool IsDeveloperMode { get; }

#if DEBUG
        private void SelfCheck()
        {
            var result =
                Fishes
                    .SelectMany(fish => fish.CheckTranslation())
                    .Concat(
                        AreaGroups
                            .SelectMany(areaGroup => areaGroup.CheckTranslation()))
                    .Concat(
                        AreaGroups
                            .SelectMany(areaGroup => areaGroup.Areas)
                            .SelectMany(area => area.CheckTranslation()))
                    .Concat(
                        AreaGroups
                            .SelectMany(areaGroup => areaGroup.Areas)
                            .SelectMany(area => area.FishingSpots)
                            .SelectMany(spot => spot.CheckTranslation()))
                    .Concat(
                        Fishes
                            .SelectMany(fish => fish.FishingConditions.Select(c => c.FishingSpot))
                            .SelectMany(fishingSpot => fishingSpot.CheckTranslation()))
                    .Concat(
                        FishingBaits
                            .SelectMany(bait => bait.CheckTranslation()))
                    .Concat(
                        Enum.GetValues(typeof(WeatherType))
                        .Cast<WeatherType>()
                        .Where(weather => weather != WeatherType.None)
                        .SelectMany(weather => weather.CheckTranslation()))
                    .ToArray();
            if (result.Any())
                throw new Exception(string.Format("Can't translate: {0}", string.Join(", ", result.Select(s => string.Format("'{0}'", s)))));
            Task.Run(() => new FishDataVerifier().GenerateCode(FishingSpots));
        }
#endif
    }
}
