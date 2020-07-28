using System.Collections.Generic;
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
        private ICollection<MenuItemViewModel> _menuItems;

        public FishDetailViewModel(Fish fish)
        {
            _isDisposed = false;
            _fish = fish;
            IsOK = false;
            var brushConverer = new BrushConverter();
            Background = _fish.DifficultySymbol.GetBackgroundColor();
            _menuItems = new List<MenuItemViewModel>();
            _menuItems.Add(MenuItemViewModel.CreateShowFishInCBHMenuItem(_fish));
            foreach (var spot in _fish.FishingSpots.OrderBy(spot => spot.Area.AreaGroup.Order).ThenBy(spot => spot.Area.Order).ThenBy(spot => spot.Order))
                _menuItems.Add(MenuItemViewModel.CreateShowSpotInCBHMenuItem(spot));
            foreach (var bait in _fish.FishingBaits.OrderBy(bait => bait.Order))
                _menuItems.Add(MenuItemViewModel.CreateShowBaitInCBHMenuItem(bait));
            _menuItems.Add(MenuItemViewModel.CreateSeparatorMenuItem());
            _menuItems.Add(MenuItemViewModel.CreateShowFishInEDBMenuItem(_fish));
            foreach (var bait in _fish.FishingBaits.OrderBy(bait => bait.Order))
                _menuItems.Add(MenuItemViewModel.CreateShowBaitInEDBMenuItem(bait));
            _menuItems.Add(MenuItemViewModel.CreateSeparatorMenuItem());
            _menuItems.Add(MenuItemViewModel.CreateCancelMenuItem());
            GUIText = GUITextTranslate.Instance;
            ResetCommand = new SimpleCommand(p =>
            {
                Memo = _fish.DefaultMemoText.Replace("⇒", "=>");
            });
        }

        public bool IsOK { get; }

        public string Title => string.Format(GUIText["Title.FishDetailWindow"], _fish.Name);
        public string FishingSpots => string.Join(", ", _fish.FishingSpots.Select(spot => spot.Name));
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

        public IEnumerable<MenuItemViewModel> ContextMenuItems => _menuItems;
        public GUITextTranslate GUIText { get; }
        public Brush Background { get; }
        public ICommand OKCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ResetCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var menuItem in _menuItems)
                    menuItem.Dispose();
                _menuItems.Clear();
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}