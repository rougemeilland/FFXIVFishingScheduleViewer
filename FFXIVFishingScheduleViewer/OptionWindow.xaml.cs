using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FFXIVFishingScheduleViewer
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow
        : WindowBase
    {
        private ICollection<Action> _removeEventActions;
        private SelectionChangedEventHandler _userLanguageChangedEventHandler;
        private SelectionChangedEventHandler _forecastPeriodDaysChangedEventHandler;
        private SelectionChangedEventHandler _fishingChanceListTextEffectChangedEventHandler;

        public OptionWindow()
        {
            InitializeComponent();

            _removeEventActions = new List<Action>();
            _userLanguageChangedEventHandler = null;
            _forecastPeriodDaysChangedEventHandler = null;
            _fishingChanceListTextEffectChangedEventHandler = null;
            RecoverWindowBounds();
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var removeEventAction in _removeEventActions)
            {
                try
                {
                    removeEventAction();
                }
                catch (Exception)
                {
                }
            }
            base.OnClosed(e);
        }

        protected override Point? WindowPositionInSettings
        {
            get
            {
                try
                {
                    return Point.Parse(Properties.Settings.Default.OptionWindowPosition);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.OptionWindowPosition = value?.ToString() ?? "";
        }

        protected override Size? WindowSizeInSettings
        {
            get
            {
                try
                {
                    return Size.Parse(Properties.Settings.Default.OptionWindowSize);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            set => Properties.Settings.Default.OptionWindowSize = value?.ToString() ?? "";
        }

        protected override WindowState WindowStateInSettings
        {
            get => Properties.Settings.Default.OptionWindwIsMaximized ? WindowState.Maximized : WindowState.Normal;
            set => Properties.Settings.Default.OptionWindwIsMaximized = value == WindowState.Maximized;
        }

        protected override void SaveWindowSettings()
        {
            Properties.Settings.Default.Save();
        }

        protected override void ViewModelChanged(object sender, EventArgs e)
        {
            UpdateUserLanguageView();
            UpdateForecastPeriodDaysView();
            UpdateFishingChanceListTextEffectView();
            base.ViewModelChanged(sender, e);
        }

        protected override void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TypedViewModel.GUIText):
                    UpdateUserLanguageView();
                    UpdateForecastPeriodDaysView();
                    UpdateFishingChanceListTextEffectView();
                    break;
                default:
                    break;
            }
            base.ViewModelPropertyChanged(sender, e);
        }

        internal OptionViewModel TypedViewModel
        {
            get => (OptionViewModel)ViewModel;
            set => ViewModel = value;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void UpdateUserLanguageView()
        {
            if (_userLanguageChangedEventHandler != null)
                UserLanguageSelectionControl.SelectionChanged -= _userLanguageChangedEventHandler;
            var selectionItems = new[]
            {
                    new
                    {
                        Text = string.Format(
                            "{0} ({1})",
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "日本語"), "ja"],
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "日本語")]),
                        Value = "ja",
                    },
                    new
                    {
                        Text = string.Format(
                            "{0} ({1})",
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "英語"), "en"],
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "英語")]),
                        Value = "en",
                    },
                    new
                    {
                        Text = string.Format(
                            "{0} ({1})",
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "フランス語"), "fr"],
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "フランス語")]),
                        Value = "fr",
                    },
                    new
                    {
                        Text = string.Format(
                            "{0} ({1})",
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "ドイツ語"), "de"],
                            Translate.Instance[new TranslationTextId(TranslationCategory.Language, "ドイツ語")]),
                        Value = "de",
                    },
                };

            UserLanguageSelectionControl.ItemsSource = selectionItems;
            UserLanguageSelectionControl.DisplayMemberPath = "Text";
            UserLanguageSelectionControl.SelectedValuePath = "Value";
            var found =
                Enumerable.Range(0, selectionItems.Length)
                    .Select(index => new { index, text_value = selectionItems[index] })
                    .Where(item => ViewModel == null ? item.index == 0 : item.text_value.Value == TypedViewModel.UserLanguage)
                    .SingleOrDefault();
            UserLanguageSelectionControl.SelectedIndex = found == null ? 0 : found.index;
            _userLanguageChangedEventHandler = new SelectionChangedEventHandler((s, e) =>
            {
                try
                {
                    TypedViewModel.UserLanguage = selectionItems[UserLanguageSelectionControl.SelectedIndex].Value;
                }
                catch (ArgumentOutOfRangeException)
                {
                    UserLanguageSelectionControl.SelectedIndex = 0;
                    TypedViewModel.UserLanguage = "en";
                }
            });
            UserLanguageSelectionControl.SelectionChanged += _userLanguageChangedEventHandler;
        }

        private void UpdateForecastPeriodDaysView()
        {
            if (_forecastPeriodDaysChangedEventHandler != null)
                ForecastPeriodDaysSelectionControl.SelectionChanged -= _forecastPeriodDaysChangedEventHandler;
            var selectionItems = new[]
            {
                new { Text = TypedViewModel.GUIText["Label.1Days"], Value = 1 },
                new { Text = TypedViewModel.GUIText["Label.3Days"], Value = 3 },
                new { Text = TypedViewModel.GUIText["Label.7Days"], Value = 7 },
            };

            ForecastPeriodDaysSelectionControl.ItemsSource = selectionItems;
            ForecastPeriodDaysSelectionControl.DisplayMemberPath = "Text";
            ForecastPeriodDaysSelectionControl.SelectedValuePath = "Value";
            var found =
                Enumerable.Range(0, selectionItems.Length)
                    .Select(index => new { index, text_value = selectionItems[index] })
                    .Where(item => item.text_value.Value == TypedViewModel.ForecastWeatherDays)
                    .SingleOrDefault();
            ForecastPeriodDaysSelectionControl.SelectedIndex = found == null ? 0 : found.index;
            _forecastPeriodDaysChangedEventHandler = new SelectionChangedEventHandler((s, e) =>
            {
                try
                {
                    TypedViewModel.ForecastWeatherDays = selectionItems[ForecastPeriodDaysSelectionControl.SelectedIndex].Value;
                }
                catch (ArgumentOutOfRangeException)
                {
                    ForecastPeriodDaysSelectionControl.SelectedIndex = 0;
                    TypedViewModel.ForecastWeatherDays = 1;
                }
            });
            ForecastPeriodDaysSelectionControl.SelectionChanged += _forecastPeriodDaysChangedEventHandler;
        }

        private void UpdateFishingChanceListTextEffectView()
        {
            if (_fishingChanceListTextEffectChangedEventHandler != null)
                FishingChanceListTextEffectSelectionControl.SelectionChanged -= _fishingChanceListTextEffectChangedEventHandler;
            var selectionItems = new[]
            {
                new { Text = TypedViewModel.GUIText["Label.EffectNormal"], Value = FishingChanceListTextEffectType.Normal },
                new { Text = TypedViewModel.GUIText["Label.Effect1"], Value = FishingChanceListTextEffectType.Effect1 },
                new { Text = TypedViewModel.GUIText["Label.Effect2"], Value = FishingChanceListTextEffectType.Effect2 },
            };

            FishingChanceListTextEffectSelectionControl.ItemsSource = selectionItems;
            FishingChanceListTextEffectSelectionControl.DisplayMemberPath = "Text";
            FishingChanceListTextEffectSelectionControl.SelectedValuePath = "Value";
            var found =
                Enumerable.Range(0, selectionItems.Length)
                    .Select(index => new { index, text_value = selectionItems[index] })
                    .Where(item => item.text_value.Value == TypedViewModel.FishingChanceListTextEffect)
                    .SingleOrDefault();
            FishingChanceListTextEffectSelectionControl.SelectedIndex = found == null ? 0 : found.index;
            _fishingChanceListTextEffectChangedEventHandler = new SelectionChangedEventHandler((s, e) =>
            {
                try
                {
                    TypedViewModel.FishingChanceListTextEffect = selectionItems[FishingChanceListTextEffectSelectionControl.SelectedIndex].Value;
                }
                catch (ArgumentOutOfRangeException)
                {
                    FishingChanceListTextEffectSelectionControl.SelectedIndex = 0;
                    TypedViewModel.FishingChanceListTextEffect = FishingChanceListTextEffectType.Normal;
                }
            });
            FishingChanceListTextEffectSelectionControl.SelectionChanged += _fishingChanceListTextEffectChangedEventHandler;
        }
    }
}
