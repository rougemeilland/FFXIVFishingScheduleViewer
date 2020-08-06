using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    partial class Translate
    {
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
