using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FFXIVFishingScheduleViewer
{
    class FishSettingViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private FishingSpot _fishingSpot;
        private ISettingProvider _settingProvider;
        private bool _isEditMode;
        private string _edittingMemo;
        private ICollection<MenuItemViewModel> _menuItems;

        public FishSettingViewModel(FishingCondition condition, string cellPositionType, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _fishingSpot = condition.FishingSpot;
            _settingProvider = settingProvider;
            Condition = condition;
            _settingProvider.FishFilterChanded += _settingProvider_FishFilterChanded;
            _settingProvider.FishMemoChanged += _settingProvider_FishMemoChanged;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            var brushConverer = new BrushConverter();
            Background = condition.Fish.DifficultySymbol.GetBackgroundColor();
            CellPositionType = cellPositionType;
            _menuItems = new List<MenuItemViewModel>();
            _menuItems.Add(MenuItemViewModel.CreateShowFishInCBHMenuItem(condition.Fish));
            _menuItems.Add(MenuItemViewModel.CreateShowSpotInCBHMenuItem(condition.FishingSpot));
            foreach (var bait in condition.FishingBaits.OrderBy(bait => bait.Order))
                _menuItems.Add(MenuItemViewModel.CreateShowBaitInCBHMenuItem(bait));
            _menuItems.Add(MenuItemViewModel.CreateSeparatorMenuItem());
            _menuItems.Add(MenuItemViewModel.CreateShowFishInEDBMenuItem(condition.Fish));
            foreach (var bait in condition.FishingBaits.OrderBy(bait => bait.Order))
                _menuItems.Add(MenuItemViewModel.CreateShowBaitInEDBMenuItem(bait));
            _menuItems.Add(MenuItemViewModel.CreateSeparatorMenuItem());
            _menuItems.Add(MenuItemViewModel.CreateCancelMenuItem());
            GUIText = GUITextTranslate.Instance;
            _isEditMode = false;
            EditMemoCommand = new SimpleCommand(p =>
            {
                EdittingMemo = Memo.Replace("⇒", "=>");
                IsEditMode = true;
            });
            SaveMemoCommand = new SimpleCommand(p =>
            {
                Memo = EdittingMemo.Replace("=>", "⇒");
                IsEditMode = false;
            });
            CancelMemoCommand = new SimpleCommand(p =>
            {
                IsEditMode = false;
            });
            ResetMemoCommand = new SimpleCommand(p =>
           {
               Memo = null;
               IsEditMode = false;
           });
        }

        public FishingCondition Condition { get; }

        public string FishName => Condition.Fish.Name;

        public bool IsEnabledFilter
        {
            get => _settingProvider.GetIsEnabledFishFilter(Condition.Fish);
            set => _settingProvider.SetIsEnabledFishFilter(Condition.Fish, value);
        }

        public string Memo
        {
            get => _settingProvider.GetFishMemo(Condition.Fish, Condition.FishingSpot);
            set => _settingProvider.SetFishMemo(Condition.Fish, Condition.FishingSpot, value);
        }

        public string EdittingMemo
        {
            get => _edittingMemo;

            set
            {
                if (value != _edittingMemo)
                {
                    _edittingMemo = value;
                    RaisePropertyChangedEvent(nameof(EdittingMemo));
                }
            }
        }

        public bool IsEditMode
        {
            get
            {
                return _isEditMode;
            }

            set
            {
                if (value != _isEditMode)
                {
                    _isEditMode = value;
                    RaisePropertyChangedEvent(nameof(IsEditMode));
                }
            }
        }

        public string DiscoveryDifficulty => Condition.Fish.DifficultySymbol.GetShortText();
        public string TimeCondition =>
            string.Join(
                ", ",
                Condition.ConditionElements
                    .Where(element => element is ITimeFishingConditionElement)
                    .Select(element => element.Description)
                    .Distinct());
        public string WeatherCondition =>
            string.Join(
                ", ",
                Condition.ConditionElements
                    .Where(element => element is IWeatherFishingConditionElement)
                    .Select(element => element.Description)
                    .Distinct());
        public Brush Background { get; }
        public string CellPositionType { get; }
        public IEnumerable<MenuItemViewModel> ContextMenuItems => _menuItems;
        public GUITextTranslate GUIText { get; }
        public ICommand EditMemoCommand { get; }
        public ICommand SaveMemoCommand { get; }
        public ICommand CancelMemoCommand { get; }
        public ICommand ResetMemoCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var menuItem in _menuItems)
                    menuItem.Dispose();
                _settingProvider.FishFilterChanded -= _settingProvider_FishFilterChanded;
                _settingProvider.FishMemoChanged -= _settingProvider_FishMemoChanged;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_FishFilterChanded(object sender, Fish e)
        {
            if (e == Condition.Fish)
                RaisePropertyChangedEvent(nameof(IsEnabledFilter));
        }

        private void _settingProvider_FishMemoChanged(object sender, FishMemoChangedEventArgs e)
        {
            if (e.Fish == Condition.Fish && e.FishingSpot == Condition.FishingSpot)
                RaisePropertyChangedEvent(nameof(Memo));
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishName));
            RaisePropertyChangedEvent(nameof(Memo));
            RaisePropertyChangedEvent(nameof(ContextMenuItems));
            RaisePropertyChangedEvent(nameof(GUIText));
            RaisePropertyChangedEvent(nameof(DiscoveryDifficulty));
            RaisePropertyChangedEvent(nameof(TimeCondition));
            RaisePropertyChangedEvent(nameof(WeatherCondition));
        }
    }
}