using FFXIVFishingScheduleViewer.Strings;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer.Models
{
    static partial class FishingBaitExtensions
    {
        private class FishingBaitPageIdElement
        {
            public string baitId { get; set; }
            public string pageId { get; set; }
        }

        private static IDictionary<string, string> _pageIdSourceOfCBH;
        private static IDictionary<string, string> _pageIdSourceOfEDB;

        static FishingBaitExtensions()
        {
            _pageIdSourceOfCBH =
                GetPageIdSourceOfCBH()
                .ToDictionary(item => item.baitId, item => item.pageId);

            _pageIdSourceOfEDB =
                GetPageIdSourceOfEDB()
                .ToDictionary(item => item.baitId, item => item.pageId);
        }

        public static string GetCBHLink(this FishingBait bait)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(((IGameDataObject)bait).InternalId, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "CBH.BaitPage")];
            return string.Format(format, pageId);
        }

        public static string GetEDBLink(this FishingBait bait)
        {
            string pageId;
            if (!_pageIdSourceOfEDB.TryGetValue(((IGameDataObject)bait).InternalId, out pageId))
                return null;
            var format = Translate.Instance[new TranslationTextId(TranslationCategory.Url, "EDB.BaitPage")];
            return string.Format(format, pageId);
        }

        internal static bool IsFishingBaitRawId(this string text)
        {
            return _pageIdSourceOfCBH.ContainsKey(text);
        }
    }
}
