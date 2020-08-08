using System.Collections;

namespace FFXIVFishingScheduleViewer.Strings
{
    public class GUITextTranslate
        : IEnumerable
    {
        static GUITextTranslate()
        {
            Instance = new GUITextTranslate();
        }

        private GUITextTranslate()
        {
        }

        public string this[string id]
        {
            get
            {
                return Translate.Instance[new TranslationTextId(TranslationCategory.GUIText, id)];
            }
        }

        public static GUITextTranslate Instance { get; }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
