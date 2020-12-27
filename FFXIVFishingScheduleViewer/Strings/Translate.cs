using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer.Strings
{
    partial class Translate
    {
#if DEBUG && false
        private class TranslateItemList
        {
            private static System.Text.RegularExpressions.Regex _idPattern;
            private static IDictionary<string, string> _mapCategoryIdToCategoryName;
            private static IDictionary<string, int> _mapCategoryIdToCategoryOrder;
            private static IDictionary<string, int> _mapLanguageIdToLanguageOrder;

            private class ParsedTranslationItem
            {

                static ParsedTranslationItem()
                {
                    _idPattern = new System.Text.RegularExpressions.Regex(@"^(?<category>[A-Za-z]+)\.(?<key>.*)\.(?<lang>ja|en|fr|de)$", System.Text.RegularExpressions.RegexOptions.Compiled);
                }

                public ParsedTranslationItem(TranslationElement element)
                {
                    Element = element;
                    var m = _idPattern.Match(element.id);
                    if (!m.Success)
                        throw new Exception();
                    CategoryId = m.Groups["category"].Value;
                    Key = m.Groups["key"].Value;
                    Language = m.Groups["lang"].Value;
                    LanguageOrder = _mapLanguageIdToLanguageOrder[Language];
                }

                public TranslationElement Element { get; }
                public string CategoryId { get; }
                public string Key { get; }
                public string Language { get; }
                public int LanguageOrder { get; }
            }


            static TranslateItemList()
            {
                _mapCategoryIdToCategoryName = new[]
                {
                    new { id = "lang", name = "言語" },
                    new { id = "area", name = "エリア" },
                    new { id = "areaGroup", name = "地方" },
                    new { id = "spot", name = "釣り場" },
                    new { id = "bait", name = "釣り餌" },
                    new { id = "fish", name = "魚" },
                    new { id = "weather", name = "天候" },
                    new { id = "action", name = "アクション" },
                    new { id = "license", name = "ライセンス" },
                    new { id = "url", name = "URL" },
                    new { id = "guiText", name = "GUI" },
                    new { id = "generic", name = "一般" },
                }
                .ToDictionary(item => item.id, item => item.name);
                _mapCategoryIdToCategoryOrder = new[]
                {
                    new { id = "lang", order = 1, },
                    new { id = "area", order = 2, },
                    new { id = "areaGroup", order = 3, },
                    new { id = "spot", order = 4, },
                    new { id = "bait", order = 5, },
                    new { id = "fish", order = 6, },
                    new { id = "weather", order = 7, },
                    new { id = "action", order = 8, },
                    new { id = "license", order = 9, },
                    new { id = "url", order = 10, },
                    new { id = "guiText", order = 11, },
                    new { id = "generic", order = 12, },
                }
                .ToDictionary(item => item.id, item => item.order);
                _mapLanguageIdToLanguageOrder = new[]
                {
                    new { id = "ja", order = 1, },
                    new { id = "en", order = 2, },
                    new { id = "fr", order = 3, },
                    new { id = "de", order = 4, },
                }
                .ToDictionary(item => item.id, item => item.order);
            }

            private string _categoryId;
            private string _categoryName;
            private int _categoryOrder;
            private IEnumerable<ParsedTranslationItem> _elements;

            private TranslateItemList(string categoryId, IEnumerable<ParsedTranslationItem> elements)
            {
                _categoryId = categoryId;
                _categoryName = _mapCategoryIdToCategoryName[categoryId];
                _categoryOrder = _mapCategoryIdToCategoryOrder[categoryId];
                _elements =
                    elements
                    .OrderBy(item => item.LanguageOrder)
                    .OrderBy(item => item.Key)
                    .ToList();
            }

            public static void Render(IEnumerable<TranslationElement> elements, Action<string> lineWriter)
            {
                var source =
                    elements
                    .Select(element => new ParsedTranslationItem(element))
                    .GroupBy(item => item.CategoryId)
                    .Select(g => new TranslateItemList(g.Key, g))
                    .OrderBy(item => item._categoryOrder);
                foreach (var element in source)
                    element.RenderElement(lineWriter);
            }

            private void RenderElement(Action<string> lineWriter)
            {
                lineWriter(string.Format("#region {0}", _categoryName));
                foreach (var element in _elements.Select(item => item.Element))
                {
                    lineWriter(
                        string.Format(
                            "                new {{ id = \"{0}\", text = \"{1}\" }},",
                            element.id,
                            element.text
                                .Replace(@"\", @"\\")
                                .Replace("\n", "\\n")
                                .Replace("\r", "\\r")
                                .Replace("\n", "\\n")
                                .Replace("\"", "\\\"")));
                }
                lineWriter("#endregion");
                lineWriter("");
            }
        }

#endif

        private const string _defaultText = "**???**";
        private IDictionary<string, string> _translateTable;
        private string _lang;

        static Translate()
        {
            Instance = new Translate();
        }

        public Translate()
        {
            var source = GetSource();
            var duplicated =
                source
                    .GroupBy(item => item.id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToArray();
            if (duplicated.Any())
                throw new Exception(string.Format("duplicated: {0}", string.Join(", ", duplicated)));
#if DEBUG && false
            var outputFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Translate).Assembly.Location), "translationItemList.txt");
            using (var writer = new System.IO.StreamWriter(outputFilePath, false))
            {
                TranslateItemList.Render(source, s => writer.WriteLine(s));
            }
#endif
            _translateTable = source.ToDictionary(item => item.id, item => item.text);
            _lang = "en";
        }

        public static Translate Instance { get; }

        public static IEnumerable<string> SupportedLanguages = new[] { "ja", "en", "fr", "de" };

        public void SetLanguage(string lang)
        {
            _lang = lang;
        }

        public IEnumerable<string> CheckTranslation(TranslationTextId id)
        {
#if DEBUG
            return
                SupportedLanguages
                .Select(lang => new { id, lang, text = this[id, lang] })
                .Where(item => item.text == _defaultText)
                .Select(item => BuildInternalTableKey(item.id, item.lang));
#else
            return new string[0];
#endif
        }

        public string this[TranslationTextId id] => this[id, _lang];

        public string this[TranslationTextId id, string lang]
        {
            get
            {
                string text;
                if (!_translateTable.TryGetValue(BuildInternalTableKey(id, lang), out text) &&
                    !_translateTable.TryGetValue(BuildInternalTableKey(id, "en"), out text))
                        return _defaultText;
                return text;
            }
        }

        public void Add(TranslationTextId id, string lang, string text)
        {
            if (text.Contains(_defaultText))
                throw new Exception();
            _translateTable.Add(BuildInternalTableKey(id, lang), text);
        }

        private static string BuildInternalTableKey(TranslationTextId id, string lang)
        {
            return string.Format("{0}.{1}", id.ToString(), lang);
        }
    }
}
