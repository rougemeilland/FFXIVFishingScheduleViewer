using FFXIVFishingScheduleViewer.Strings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FFXIVFishingScheduleViewer.Models
{
    class SettingProvider
        : ISettingProvider
    {
        private Properties.Settings _settings;
        private event EventHandler _selectedMainViewTabIndexChanged;
        private event EventHandler _selectedOptionCategoryTabIndexChanged;
        private event EventHandler _selectedOptionAreaGroupTabIndexChanged;
        private event EventHandler<AreaGroup> _areaGroupOnForecastWeatherExpanded;
        private event EventHandler<AreaGroup> _areaGroupOnForecastWeatherContracted;
        private event EventHandler<Fish> _fishFilterChanded;
        private event EventHandler<FishMemoChangedEventArgs> _fishMemoChanded;
        private event EventHandler _forecastWeatherDaysChanged;
        private event EventHandler _userLanguageChanged;
        private event EventHandler _isEnabledToCheckNewVersionReleasedChanged;
        private event EventHandler _fishingChanceListTextEffectChanged;
        private event EventHandler _requestedToClearSettingsChanged;

        private FishCollection _fishes;
        private IDictionary<string, string> _filteredfishNames;
        private IDictionary<string, string> _expandedAreaGroupNames;
        private IDictionary<string, string> _fishMemoList;

        public SettingProvider(FishCollection fishes)
        {
            _settings = Properties.Settings.Default;
            _fishes = fishes;
            _filteredfishNames =
                _settings.FilteredFishNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");

            _expandedAreaGroupNames =
                _settings.ExpandedAreaGroupNames
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(s => s, s => "*");

            _fishMemoList =
                _settings.FishMemoList
                .Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                .Where(columns => columns.Length == 2)
                .Select(columns => new { name = columns[0].SimpleDecode(), text = columns[1].SimpleDecode() })
                .ToDictionary(item => item.name, item => item.text);
            if (_settings.UserLanguage == "*")
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
                _settings.UserLanguage = lang;
                _settings.Save();
            }
            Translate.Instance.SetLanguage(_settings.UserLanguage);
        }

        int ISettingProvider.SelectedMainViewTabIndex
        {
            get => _settings.SelectedMainViewTabIndex;

            set
            {
                if (value != _settings.SelectedMainViewTabIndex)
                {
                    _settings.SelectedMainViewTabIndex = value;
                    _settings.Save();
                    try
                    {
                        _selectedMainViewTabIndexChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.SelectedMainViewTabIndexChanged
        {
            add
            {
                _selectedMainViewTabIndexChanged += value;
            }
            
            remove
            {
                _selectedMainViewTabIndexChanged -= value;
            }
        }

        int ISettingProvider.SelectedOptionCategoryTabIndex
        {
            get => _settings.SelectedOptionCategoryTabIndex;

            set
            {
                if (value != _settings.SelectedOptionCategoryTabIndex)
                {
                    _settings.SelectedOptionCategoryTabIndex = value;
                    _settings.Save();
                    try
                    {
                        _selectedOptionCategoryTabIndexChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.SelectedOptionCategoryTabIndexChanged
        {
            add
            {
                _selectedOptionCategoryTabIndexChanged += value;
            }

            remove
            {
                _selectedOptionCategoryTabIndexChanged -= value;
            }
        }

        int ISettingProvider.SelectedOptionAreaGroupTabIndex
        {
            get => _settings.SelectedOptionAreaGroupTabIndex;

            set
            {
                if (value != _settings.SelectedOptionAreaGroupTabIndex)
                {
                    _settings.SelectedOptionAreaGroupTabIndex = value;
                    _settings.Save();
                    try
                    {
                        _selectedOptionAreaGroupTabIndexChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.SelectedOptionAreaGroupTabIndexChanged
        {
            add
            {
                _selectedOptionAreaGroupTabIndexChanged += value;
            }

            remove
            {
                _selectedOptionAreaGroupTabIndexChanged -= value;
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
            _settings.ExpandedAreaGroupNames = string.Join("\n", _expandedAreaGroupNames.Keys);
            _settings.Save();
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
            _settings.FilteredFishNames = string.Join("\n", _filteredfishNames.Keys);
            _settings.Save();
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
                _settings.FilteredFishNames = string.Join("\n", _filteredfishNames.Keys);
                _settings.Save();
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

        string ISettingProvider.GetFishMemo(Fish fish, FishingSpot fishingSpot)
        {
            var key = string.Format("{0}**{1}", ((IGameDataObject)fish).InternalId, ((IGameDataObject)fishingSpot).InternalId);
            var condition =
                _fishes[fish.Id].FishingConditions
                .Where(item => item.FishingSpot == fishingSpot)
                .Single();
            // まず設定済みのユーザのメモの取得を試みる
            string value;
            if (_fishMemoList.TryGetValue(key, out value))
            {
                // ユーザのメモが存在した場合、そのメモを返す
                return value;
            }
            // ユーザのメモが未設定である場合は、既定値を返す。
            return condition.DefaultMemoText;
        }

        void ISettingProvider.SetFishMemo(Fish fish, FishingSpot fishingSpot, string text)
        {
            var key = string.Format("{0}**{1}", ((IGameDataObject)fish).InternalId, ((IGameDataObject)fishingSpot).InternalId);
            var condition =
                _fishes[fish.Id].FishingConditions
                .Where(item => item.FishingSpot == fishingSpot)
                .Single();
            if (text == null || condition.DefaultMemoText == text)
            {
                // メモの削除が指定されているかあるいはユーザのメモの内容が既定値である場合
                // いずれにしろ、ユーザのメモは不要であるので、削除を試みる
                if (!_fishMemoList.Remove(key))
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
                if (_fishMemoList.TryGetValue(key, out currentText) && currentText == text)
                {
                    // ユーザのメモが現在の設定値と同一である場合
                    // 何もせずに復帰
                    return;
                }
                // 設定すべきユーザのメモが指定されており、かつメモの内容が既定値と一致おらず、かつ現在の設定値と一致していない場合
                // メモの設定値を更新する
                _fishMemoList[key] = text;
                // 以降の処理で Settings を変更する
            }
            _settings.FishMemoList = string.Join("\n", _fishMemoList.Select(item => string.Format("{0}\t{1}", item.Key.SimpleEncode(), item.Value.SimpleEncode())));
            _settings.Save();
            try
            {
                _fishMemoChanded(this, new FishMemoChangedEventArgs(fish, fishingSpot));
            }
            catch (Exception)
            {
            }
        }

        event EventHandler<FishMemoChangedEventArgs> ISettingProvider.FishMemoChanged
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
                return _settings.DaysOfForecast;
            }

            set
            {
                if (value != _settings.DaysOfForecast)
                {
                    _settings.DaysOfForecast = value;
                    _settings.Save();
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
                return _settings.UserLanguage;
            }

            set
            {
                if (value != _settings.UserLanguage)
                {
                    _settings.UserLanguage = value;
                    _settings.Save();
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
            get => _settings.IsEnabledToCheckNewVersionReleased;

            set
            {
                if (value != _settings.IsEnabledToCheckNewVersionReleased)
                {
                    _settings.IsEnabledToCheckNewVersionReleased = value;
                    _settings.Save();
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

        FishingChanceListTextEffectType ISettingProvider.FishingChanceListTextEffect
        {
            get
            {
                switch (_settings.FishingChanceListTextEffect)
                {
                    case "effect1":
                        return FishingChanceListTextEffectType.Effect1;
                    case "effect2":
                        return FishingChanceListTextEffectType.Effect2;
                    case "normal":
                    default:
                        return FishingChanceListTextEffectType.Normal;
                }
            }

            set
            {
                string valueText;
                switch (value)
                {
                    case FishingChanceListTextEffectType.Effect1:
                        valueText = "effect1";
                        break;
                    case FishingChanceListTextEffectType.Effect2:
                        valueText = "effect2";
                        break;
                    case FishingChanceListTextEffectType.Normal:
                    default:
                        valueText = "normal";
                        break;
                }
                if (valueText != _settings.FishingChanceListTextEffect)
                {
                    _settings.FishingChanceListTextEffect = valueText;
                    _settings.Save();
                    try
                    {
                        _fishingChanceListTextEffectChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.FishingChanceListTextEffectChanged
        {
            add
            {
                _fishingChanceListTextEffectChanged += value;
            }

            remove
            {
                _fishingChanceListTextEffectChanged -= value;
            }
        }

        bool ISettingProvider.RequestedToClearSettings
        {
            get
            {
                return _settings.RequestedToClearSettings;
            }

            set
            {
                if (value != _settings.RequestedToClearSettings)
                {
                    _settings.RequestedToClearSettings = value;
                    _settings.Save();
                    try
                    {
                        _requestedToClearSettingsChanged(this, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        event EventHandler ISettingProvider.RequestedToClearSettingsChanged
        {
            add
            {
                _requestedToClearSettingsChanged += value;
            }

            remove
            {
                _requestedToClearSettingsChanged -= value;
            }
        }

    }
}