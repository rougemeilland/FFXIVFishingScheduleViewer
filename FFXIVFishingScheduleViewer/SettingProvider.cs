using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    class SettingProvider
        : ISettingProvider
    {
        private static Regex _locationOfReleasePattern = new Regex(@"^*./releases/tag/v(?<version>[0-9\.]+)$", RegexOptions.Compiled);
        private static Regex _versionTextPattern = new Regex(@"^(?<version>[0-9\.]+)$", RegexOptions.Compiled);
        private event EventHandler<MainWindowTabType> _mainWindowTabSelected;
        private event EventHandler<MainWindowTabType> _mainWindowTabUnselected;
        private event EventHandler<AreaGroup> _areaGroupOnForecastWeatherExpanded;
        private event EventHandler<AreaGroup> _areaGroupOnForecastWeatherContracted;
        private event EventHandler<Fish> _fishFilterChanded;
        private event EventHandler<Fish> _fishMemoChanded;
        private event EventHandler _forecastWeatherDaysChanged;
        private event EventHandler _userLanguageChanged;
        private event EventHandler _newVersionOfApplicationChanged;
        private event EventHandler _isEnabledToCheckNewVersionReleasedChanged;

        private FishCollection _fishes;
        private IDictionary<string, string> _filteredfishNames;
        private IDictionary<string, string> _expandedAreaGroupNames;
        private IDictionary<string, string> _selectedTabNames;
        private IDictionary<string, string> _fishMemoList;
        private string _newVersionOfApplication;
        private string _currentVersionText;

        public SettingProvider(FishCollection fishes)
        {
            _fishes = fishes;
            _filteredfishNames =
                Properties.Settings.Default.FilteredFishNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");
            _expandedAreaGroupNames =
                Properties.Settings.Default.ExpandedAreaGroupNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");
            _selectedTabNames =
                Properties.Settings.Default.SelectedTabNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");
            if (!_selectedTabNames.Any())
                _selectedTabNames[MainWindowTabType.ForecastWeather.ToString()] = "*";
            _fishMemoList =
                Properties.Settings.Default.FishMemoList
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                .Where(columns => columns.Length == 2)
                .Select(columns => new { name = columns[0].SimpleDecode(), text = columns[1].SimpleDecode() })
                .ToDictionary(item => item.name, item => item.text);
            if (Properties.Settings.Default.UserLanguage == "*")
            {
                var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                switch (lang)
                {
                    case "ja":
                    case "en":
                    case "fr":
                    case "de":
                        break;
                    default:
                        lang = "en";
                        break;
                }
                Properties.Settings.Default.UserLanguage = lang;
                Properties.Settings.Default.Save();
            }
            Translate.Instance.SetLanguage(Properties.Settings.Default.UserLanguage);
            _newVersionOfApplication = null;
            var m = _versionTextPattern.Match(GetType().Assembly.GetName().Version.ToString());
            _currentVersionText = m.Success ? m.Groups["version"].Value : null;
        }

        bool ISettingProvider.GetIsSelectedMainWindowTab(MainWindowTabType tab)
        {
            return _selectedTabNames.ContainsKey(tab.ToString());
        }

        void ISettingProvider.SetIsSelectedMainWindowTab(MainWindowTabType tab, bool value)
        {
            var tabName = tab.ToString();
            if (value)
            {
                if (_selectedTabNames.ContainsKey(tabName))
                    return;
                _selectedTabNames[tabName] = "*";
            }
            else
            {
                if (!_selectedTabNames.ContainsKey(tabName))
                    return;
                _selectedTabNames.Remove(tabName);
            }
            Properties.Settings.Default.SelectedTabNames = string.Join("\n", _selectedTabNames.Keys);
            Properties.Settings.Default.Save();
            try
            {
                if (value)
                    _mainWindowTabSelected?.Invoke(this, tab);
                else
                    _mainWindowTabUnselected?.Invoke(this, tab);
            }
            catch (Exception)
            {
            }
        }

        event EventHandler<MainWindowTabType> ISettingProvider.MainWindowTabSelected
        {
            add
            {
                _mainWindowTabSelected += value;
            }

            remove
            {
                _mainWindowTabSelected -= value;
            }
        }

        event EventHandler<MainWindowTabType> ISettingProvider.MainWindowTabUnselected
        {
            add
            {
                _mainWindowTabUnselected += value;
            }

            remove
            {
                _mainWindowTabUnselected -= value;
            }
        }

        bool ISettingProvider.GetIsExpandedAreaGroupOnForecastWeather(AreaGroup areaGroup)
        {
            return _expandedAreaGroupNames.ContainsKey(areaGroup.Id.ToString());
        }

        void ISettingProvider.SetIsExpandedAreaGroupOnForecastWeather(AreaGroup areaGroup, bool value)
        {
            var areaGroupIdText = areaGroup.Id.ToString();
            if (value)
            {
                if (_expandedAreaGroupNames.ContainsKey(areaGroupIdText))
                    return;
                _expandedAreaGroupNames[areaGroupIdText] = "*";
            }
            else
            {
                if (!_expandedAreaGroupNames.ContainsKey(areaGroupIdText))
                    return;
                _expandedAreaGroupNames.Remove(areaGroupIdText);
            }
            Properties.Settings.Default.ExpandedAreaGroupNames = string.Join("\n", _expandedAreaGroupNames.Keys);
            Properties.Settings.Default.Save();
            try
            {
                if (value)
                    _areaGroupOnForecastWeatherExpanded(this, areaGroup);
                else
                    _areaGroupOnForecastWeatherContracted(this, areaGroup);
            }
            catch (Exception)
            {
            }
        }

        event EventHandler<AreaGroup> ISettingProvider.AreaGroupOnForecastWeatherExpanded
        {
            add
            {
                _areaGroupOnForecastWeatherExpanded += value;
            }

            remove
            {
                _areaGroupOnForecastWeatherExpanded -= value;
            }
        }

        event EventHandler<AreaGroup> ISettingProvider.AreaGroupOnForecastWeatherContracted
        {
            add
            {
                _areaGroupOnForecastWeatherContracted += value;
            }

            remove
            {
                _areaGroupOnForecastWeatherContracted -= value;
            }
        }

        bool ISettingProvider.GetIsEnabledFishFilter(Fish fish)
        {
            return _filteredfishNames.ContainsKey(fish.Id.ToString());
        }

        void ISettingProvider.SetIsEnabledFishFilter(Fish fish, bool value)
        {
            var fishIdText = fish.Id.ToString();
            if (value)
            {
                if (_filteredfishNames.ContainsKey(fishIdText))
                    return;
                _filteredfishNames[fishIdText] = "*";
            }
            else
            {
                if (!_filteredfishNames.ContainsKey(fishIdText))
                    return;
                _filteredfishNames.Remove(fishIdText);
            }
            Properties.Settings.Default.FilteredFishNames = string.Join("\n", _filteredfishNames.Keys);
            Properties.Settings.Default.Save();
            try
            {
                _fishFilterChanded(this, fish);
            }
            catch (Exception)
            {
            }
        }

        void ISettingProvider.SetIsEnabledFishFilter(IEnumerable<Fish> fishes, bool value)
        {
            var indexedFishes = fishes.Distinct().ToDictionary(fish => fish.Id.ToString(), fish => fish);
            string[] diffIds;
            if (value)
            {
                diffIds = indexedFishes.Keys.Except(_filteredfishNames.Keys).ToArray();
                foreach (var fishIdText in diffIds)
                    _filteredfishNames[fishIdText] = "*";
            }
            else
            {
                diffIds = indexedFishes.Keys.Intersect(_filteredfishNames.Keys).ToArray();
                foreach (var fishIdText in diffIds)
                    _filteredfishNames.Remove(fishIdText);
            }
            if (diffIds.Any())
            {
                Properties.Settings.Default.FilteredFishNames = string.Join("\n", _filteredfishNames.Keys);
                Properties.Settings.Default.Save();
                foreach (var id in diffIds)
                {
                    Fish fish;
                    if (indexedFishes.TryGetValue(id, out fish))
                    {
                        try
                        {
                            _fishFilterChanded(this, fish);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }


        event EventHandler<Fish> ISettingProvider.FishFilterChanded
        {
            add
            {
                _fishFilterChanded += value;
            }

            remove
            {
                _fishFilterChanded -= value;
            }
        }

        string ISettingProvider.GetFishMemo(Fish fish)
        {
            // まず設定済みのユーザのメモの取得を試みる
            string value;
            if (_fishMemoList.TryGetValue(fish.Id.ToString(), out value))
            {
                // ユーザのメモが存在した場合、そのメモを返す
                return value;
            }
            // ユーザのメモが未設定である場合は、既定値を返す。
            return _fishes[fish.Id].TranslatedMemo;
        }

        void ISettingProvider.SetFishMemo(Fish fish, string text)
        {
            var fishIdText = fish.Id.ToString();
            if (text == null || _fishes[fish.Id].TranslatedMemo == text)
            {
                // メモの削除が指定されているかあるいはユーザのメモの内容が既定値である場合
                // いずれにしろ、ユーザのメモは不要であるので、削除を試みる
                if (!_fishMemoList.Remove(fishIdText))
                {
                    // メモの削除を試みようとしたがもともとユーザのメモが存在しなかった場合
                    // 何もせずに復帰
                    return;
                }
                // 不要なユーザのメモの削除を行い、_fishMemoList が更新できた場合
                // 以降の処理で Settings を変更する
            }
            else
            {
                // 設定すべきユーザのメモが指定されており、かつメモの内容が既定値と一致していない場合
                // 現在設定されているユーザのメモを取得する
                string currentText;
                if (_fishMemoList.TryGetValue(fishIdText, out currentText) && currentText == text)
                {
                    // ユーザのメモが現在の設定値と同一である場合
                    // 何もせずに復帰
                    return;
                }
                // 設定すべきユーザのメモが指定されており、かつメモの内容が既定値と一致おらず、かつ現在の設定値と一致していない場合
                // メモの設定値を更新する
                _fishMemoList[fishIdText] = text;
                // 以降の処理で Settings を変更する
            }
            Properties.Settings.Default.FishMemoList = string.Join("\n", _fishMemoList.Select(item => string.Format("{0}\t{1}", item.Key.SimpleEncode(), item.Value.SimpleEncode())));
            Properties.Settings.Default.Save();
            try
            {
                _fishMemoChanded(this, fish);
            }
            catch (Exception)
            {
            }
        }

        event EventHandler<Fish> ISettingProvider.FishMemoChanged
        {
            add
            {
                _fishMemoChanded += value;
            }

            remove
            {
                _fishMemoChanded -= value;
            }
        }

        int ISettingProvider.ForecastWeatherDays
        {
            get
            {
                return Properties.Settings.Default.DaysOfForecast;
            }

            set
            {
                if (value != Properties.Settings.Default.DaysOfForecast)
                {
                    Properties.Settings.Default.DaysOfForecast = value;
                    Properties.Settings.Default.Save();
                    try
                    {
                        _forecastWeatherDaysChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }

                }
            }
        }

        event EventHandler ISettingProvider.ForecastWeatherDaysChanged
        {
            add
            {
                _forecastWeatherDaysChanged += value;
            }

            remove
            {
                _forecastWeatherDaysChanged -= value;
            }
        }

        string ISettingProvider.UserLanguage
        {
            get
            {
                return Properties.Settings.Default.UserLanguage;
            }

            set
            {
                if (value != Properties.Settings.Default.UserLanguage)
                {
                    Properties.Settings.Default.UserLanguage = value;
                    Properties.Settings.Default.Save();
                    Translate.Instance.SetLanguage(value);
                    try
                    {
                        _userLanguageChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.UserLanguageChanged
        {
            add
            {
                _userLanguageChanged += value;
            }

            remove
            {
                _userLanguageChanged -= value;
            }
        }

        bool ISettingProvider.IsEnabledToCheckNewVersionReleased
        {
            get => Properties.Settings.Default.IsEnabledToCheckNewVersionReleased;

            set
            {
                if (value != Properties.Settings.Default.IsEnabledToCheckNewVersionReleased)
                {
                    Properties.Settings.Default.IsEnabledToCheckNewVersionReleased = value;
                    Properties.Settings.Default.Save();
                    try
                    {
                        _isEnabledToCheckNewVersionReleasedChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.IsEnabledToCheckNewVersionReleasedChanged
        {
            add
            {
                _isEnabledToCheckNewVersionReleasedChanged += value;
            }

            remove
            {
                _isEnabledToCheckNewVersionReleasedChanged -= value;
            }
        }

        void ISettingProvider.CheckNewVersionReleased()
        {
            if (Properties.Settings.Default.IsEnabledToCheckNewVersionReleased && _currentVersionText != null)
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
                                NewVersionOfApplication = newVersionText.CompareVersionString(_currentVersionText) > 0 ? newVersionText : null;
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
                if ( value != _newVersionOfApplication)
                {
                    _newVersionOfApplication = value;
                    try
                    {
                        _newVersionOfApplicationChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        string ISettingProvider.CurrentVersionOfApplication => _currentVersionText;

        event EventHandler ISettingProvider.NewVersionOfApplicationChanged
        {
            add
            {
                _newVersionOfApplicationChanged += value;
            }

            remove
            {
                _newVersionOfApplicationChanged -= value;
            }
        }

        string ISettingProvider.UrlOfDownloadPage => Properties.Settings.Default.UrlOfDownloadPage;
    }
}