using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FFXIVFishingScheduleViewer
{
    class FishDetailViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private Fish _fish;
        private string _memo;

        public FishDetailViewModel(Fish fish)
        {
            _isDisposed = false;
            _fish = fish;
            IsOK = false;
            var brushConverer = new BrushConverter();
            Background = _fish.DifficultySymbol.GetBackgroundColor();
            GUIText = GUITextTranslate.Instance;
            ResetCommand = new SimpleCommand(p =>
            {
                Memo = _fish.TranslatedMemo;
            });
        }

        public bool IsOK { get; }

        public string Title => string.Format(GUIText["Title.FishDetailWindow"], _fish.Name);
        public GUITextTranslate GUIText { get; }
        public string FishingSpots => string.Join(" / ", _fish.FishingSpots.Select(spot => spot.Name));
        public string RequiredFishingBaits => string.Join(" , ", _fish.FishingBaits.Select(bait => bait.Name));

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

        public string DiscoveryDifficulty => _fish.DifficultySymbol.GetShortText();
        public string TimeCondition
        {
            get
            {
                var query =
                    _fish.FishingConditions
                    .GroupBy(condition => condition.FishingSpot)
                    .Select(g => new
                    {
                        fishingSpot = g.Key,
                        timeCondition =
                            g
                            .SelectMany(condition => condition.ConditionElements)
                            .Where(element => element is ITimeFishingConditionElement)
                            .Select(element => element.Description)
                            .Distinct()
                            .Single()
                    })
                    .ToArray();
                if (query.Length == 0)
                    return "";
                else if (query.Length == 1)
                    return query[0].timeCondition;
                else
                    return string.Join(", ", query.Select(item => string.Format("{0} ({1})", item.timeCondition, item.fishingSpot)));
            }
        }

        public string WeatherCondition
        {
            get
            {
                var query =
                    _fish.FishingConditions
                    .GroupBy(condition => condition.FishingSpot)
                    .Select(g => new
                    {
                        fishingSpot = g.Key,
                        weatherCondition =
                            g
                            .SelectMany(condition => condition.ConditionElements)
                            .Where(element => element is IWeatherFishingConditionElement)
                            .Select(element => element.Description)
                            .Distinct()
                            .Single()
                    })
                    .ToArray();
                if (query.Length == 0)
                    return "";
                else if (query.Length == 1)
                    return query[0].weatherCondition;
                else
                    return string.Join(", ", query.Select(item => string.Format("{0} ({1})", item.weatherCondition, item.fishingSpot)));
            }
        }

        public Brush Background { get; }
        public ICommand OKCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ResetCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}