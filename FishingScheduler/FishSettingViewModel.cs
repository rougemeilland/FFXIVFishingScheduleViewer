﻿using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FishingScheduler
{
    class FishSettingViewModel
        : ViewModel
    {
        private bool _isDisposed;
        private FishingSpot _fishingSpot;
        private ISettingProvider _settingProvider;
        private bool _isEditMode;
        private string _edittingMemo;

        public FishSettingViewModel(Fish fish, FishingSpot fishingSpot, string cellPositionType, ISettingProvider settingProvider)
        {
            _isDisposed = false;
            _fishingSpot = fishingSpot;
            _settingProvider = settingProvider;
            Fish = fish;
            _settingProvider.FishFilterChanded += _settingProvider_FishFilterChanded;
            _settingProvider.FishMemoChanged += _settingProvider_FishMemoChanged;
            _settingProvider.UserLanguageChanged += _settingProvider_UserLanguageChanged;
            var brushConverer = new BrushConverter();
            Background = Fish.DifficultySymbol.GetBackgroundColor();
            CellPositionType = cellPositionType;
            GUIText = GUITextTranslate.Instance;
            _isEditMode = false;
            EditMemoCommand = new SimpleCommand(p =>
            {
                EdittingMemo = Memo;
                IsEditMode = true;
            });
            SaveMemoCommand = new SimpleCommand(p =>
            {
                Memo = EdittingMemo;
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

        public Fish Fish { get; }

        public string FishName => Fish.Name;

        public bool IsEnabledFilter
        {
            get => _settingProvider.GetIsEnabledFishFilter(Fish);
            set => _settingProvider.SetIsEnabledFishFilter(Fish, value);
        }

        public string Memo
        {
            get => _settingProvider.GetFishMemo(Fish);
            set => _settingProvider.SetFishMemo(Fish, value);
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

        public string DiscoveryDifficulty => Fish.DifficultySymbol.GetShortText();
        public string TimeCondition =>
            string.Join(
                ", ",
                Fish.FishingConditions
                    .Where(condition => condition.FishingSpot == _fishingSpot)
                    .SelectMany(condition => condition.ConditionElements)
                    .Where(element => element is ITimeFishingConditionElement)
                    .Select(element => element.Description)
                    .Distinct());
        public string WeatherCondition =>
            string.Join(
                ", ",
                Fish.FishingConditions
                    .Where(condition => condition.FishingSpot == _fishingSpot)
                    .SelectMany(condition => condition.ConditionElements)
                    .Where(element => element is IWeatherFishingConditionElement)
                    .Select(element => element.Description)
                    .Distinct());
        public Brush Background { get; }
        public string CellPositionType { get; }
        public GUITextTranslate GUIText { get; }
        public ICommand EditMemoCommand { get; }
        public ICommand SaveMemoCommand { get; }
        public ICommand CancelMemoCommand { get; }
        public ICommand ResetMemoCommand { get; }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _settingProvider.FishFilterChanded -= _settingProvider_FishFilterChanded;
                _settingProvider.FishMemoChanged -= _settingProvider_FishMemoChanged;
                _settingProvider.UserLanguageChanged -= _settingProvider_UserLanguageChanged;
                _isDisposed = true;
                base.Dispose(disposing);
            }
        }

        private void _settingProvider_FishFilterChanded(object sender, Fish e)
        {
            if (e == Fish)
                RaisePropertyChangedEvent(nameof(IsEnabledFilter));
        }

        private void _settingProvider_FishMemoChanged(object sender, Fish e)
        {
            if (e == Fish)
                RaisePropertyChangedEvent(nameof(Memo));
        }

        private void _settingProvider_UserLanguageChanged(object sender, EventArgs e)
        {
            RaisePropertyChangedEvent(nameof(FishName));
            RaisePropertyChangedEvent(nameof(Memo));
            RaisePropertyChangedEvent(nameof(GUIText));
            RaisePropertyChangedEvent(nameof(DiscoveryDifficulty));
            RaisePropertyChangedEvent(nameof(TimeCondition));
            RaisePropertyChangedEvent(nameof(WeatherCondition));
        }
    }
}