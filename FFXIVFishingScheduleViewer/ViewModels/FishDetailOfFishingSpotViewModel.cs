using FFXIVFishingScheduleViewer.Models;
using FFXIVFishingScheduleViewer.Strings;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FFXIVFishingScheduleViewer.ViewModels
{
    class FishDetailOfFishingSpotViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private ISettingProvider _settingProvider;
        private Func<FishingSpot, string> _memoGetter;
        private string _memo;

        public FishDetailOfFishingSpotViewModel(Fish fish, FishingSpot fishingSpot, Func<FishingSpot, string> memoGetter, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _settingProvider = settingProvider;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            _memoGetter = memoGetter;
            _memo = _memoGetter(fishingSpot);
            Fish = fish;
            FishingSpot = fishingSpot;
            Background = fish.DifficultySymbol.GetBackgroundColor();
            ResetCommand = new SimpleCommand(p =>
            {
                Memo = fish.FishingConditions.Where(condition => condition.FishingSpot == fishingSpot).Single().DefaultMemoText.Replace("⇒", "=>");
            });
        }

        public Fish Fish { get; }
        public FishingSpot FishingSpot { get; }

        public Brush Background { get; }
        public string FishingSpotName => FishingSpot.Name;

        public string RequiredFishingBaits =>
            string.Join(
                " , ",
                Fish.FishingConditions
                .Where(condition => condition.FishingSpot == FishingSpot)
                .Single()
                    .FishingBaits
                        .Select(bait => bait.Name));

        public string Memo
        {
            get => _memo;

            set
            {
                if (value != _memo)
                {
                    _memo = value;
                    RaisePropertyChangedEvent(nameof(Memo));
                }
            }
        }

        public string DiscoveryDifficulty => Fish.DifficultySymbol.GetShortText();
        public string TimeCondition
        {
            get
            {
                var text =
                    Fish.FishingConditions
                    .Where(condition => condition.FishingSpot == FishingSpot)
                    .Single()
                    .ConditionElements
                        .Where(element => element is ITimeFishingConditionElement)
                        .Select(element => element.Description)
                        .Where(description => description != null)
                        .Distinct()
                        .SingleOrDefault();
                return text ?? "";
            }
        }

        public string WeatherCondition
        {
            get
            {
                var text =
                    Fish.FishingConditions
                    .Where(condition => condition.FishingSpot == FishingSpot)
                    .Single()
                    .ConditionElements
                        .Where(element => element is IWeatherFishingConditionElement)
                        .Select(element => element.Description)
                        .Where(description => description != null)
                        .Distinct()
                        .SingleOrDefault();
                return text ?? "";
            }
        }

        public ICommand ResetCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishingSpotName));
            RaisePropertyChangedEvent(nameof(GUIText));
        }
    }
}