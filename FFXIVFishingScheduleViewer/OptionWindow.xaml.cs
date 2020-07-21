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
    public partial class OptionWindow : Window
    {
        private DataContext _dataContext;
        private ICollection<Action> _removeEventActions;
        private SelectionChangedEventHandler _userLanguageChangedEventHandler;
        private SelectionChangedEventHandler _forecastPeriodDaysChangedEventHandler;

        public OptionWindow()
        {
            InitializeComponent();

            _dataContext = null;
            DataContext = _dataContext;

            _removeEventActions = new List<Action>();
            _userLanguageChangedEventHandler = null;
            _forecastPeriodDaysChangedEventHandler = null;

            SetDataContext(DataContext);
            if (_dataContext != null)
            {
                UpdateWindowTitle();
                UpdateUserLanguageView();
                UpdateForecastPeriodDaysView();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var removeEventAction in _removeEventActions)
                removeEventAction();
            if (_dataContext != null)
                _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
            base.OnClosed(e);
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetDataContext(e.NewValue);
            if (_dataContext != null)
            {
                UpdateWindowTitle();
                UpdateUserLanguageView();
                UpdateForecastPeriodDaysView();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void _dataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_dataContext.GUIText):
                    UpdateWindowTitle();
                    UpdateUserLanguageView();
                    UpdateForecastPeriodDaysView();
                    break;
                default:
                    break;
            }
        }

        private void SetDataContext(object o)
        {
            if (_dataContext != null)
                _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
            if (o != null && o is DataContext)
            {
                _dataContext = (DataContext)o;
                _dataContext.PropertyChanged += _dataContext_PropertyChanged;
            }
            else
                _dataContext = null;
        }

        private void UpdateWindowTitle()
        {
            Title = string.Format("{0} - {1}", _dataContext.GUIText["Title.Option"], Owner.Title);
        }

        private void UpdateUserLanguageView()
        {
            if (_userLanguageChangedEventHandler != null)
                UserLanguage.SelectionChanged -= _userLanguageChangedEventHandler;
            var languagePeriodSelectionItems = new[]
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

            UserLanguage.ItemsSource = languagePeriodSelectionItems;
            UserLanguage.DisplayMemberPath = "Text";
            UserLanguage.SelectedValuePath = "Value";
            var found =
                Enumerable.Range(0, languagePeriodSelectionItems.Length)
                    .Select(index => new { index, text_value = languagePeriodSelectionItems[index] })
                    .Where(item => _dataContext == null ? item.index == 0 : item.text_value.Value == _dataContext.UserLanguage)
                    .SingleOrDefault();
            UserLanguage.SelectedIndex = found == null ? 0 : found.index;
            _userLanguageChangedEventHandler = new SelectionChangedEventHandler((s, e) =>
            {
                try
                {
                    _dataContext.UserLanguage = languagePeriodSelectionItems[UserLanguage.SelectedIndex].Value;
                }
                catch (ArgumentOutOfRangeException)
                {
                    UserLanguage.SelectedIndex = 0;
                    _dataContext.UserLanguage = "en";
                }
            });
            UserLanguage.SelectionChanged += _userLanguageChangedEventHandler;
        }

        private void UpdateForecastPeriodDaysView()
        {
            if (_forecastPeriodDaysChangedEventHandler != null)
                ForecastPeriodDays.SelectionChanged -= _forecastPeriodDaysChangedEventHandler;
            var forecastPeriodSelectionItems = new[]
            {
                new { Text = _dataContext.GUIText["Label.1Days"], Value = 1 },
                new { Text = _dataContext.GUIText["Label.3Days"], Value = 3 },
                new { Text = _dataContext.GUIText["Label.7Days"], Value = 7 },
            };

            ForecastPeriodDays.ItemsSource = forecastPeriodSelectionItems;
            ForecastPeriodDays.DisplayMemberPath = "Text";
            ForecastPeriodDays.SelectedValuePath = "Value";
            var found =
                Enumerable.Range(0, forecastPeriodSelectionItems.Length)
                    .Select(index => new { index, text_value = forecastPeriodSelectionItems[index] })
                    .Where(item => item.text_value.Value == _dataContext.ForecastWeatherDays)
                    .SingleOrDefault();
            ForecastPeriodDays.SelectedIndex = found == null ? 0 : found.index;
            _forecastPeriodDaysChangedEventHandler = new SelectionChangedEventHandler((s, e) =>
            {
                try
                {
                    _dataContext.ForecastWeatherDays = forecastPeriodSelectionItems[ForecastPeriodDays.SelectedIndex].Value;
                }
                catch (ArgumentOutOfRangeException)
                {
                    ForecastPeriodDays.SelectedIndex = 0;
                    _dataContext.ForecastWeatherDays = 1;
                }
            });
            ForecastPeriodDays.SelectionChanged += _forecastPeriodDaysChangedEventHandler;
        }
    }
}
