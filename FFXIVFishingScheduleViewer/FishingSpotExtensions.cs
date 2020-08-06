using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FFXIVFishingScheduleViewer
{
    static partial class FishingSpotExtensions
    {
        private class FishingSpotPageIdElement
        {
            public string spotId { get; set; }
            public string pageId { get; set; }
        }


        private static IDictionary<string, string> _pageIdSourceOfCBH;

        static FishingSpotExtensions()
        {
            _pageIdSourceOfCBH =
                GetPageIdSourceOfCBH()
                .ToDictionary(item => item.spotId, item => item.pageId);
        }

        public static string GetCBHLink(this FishingSpot spot, string lang = null)
        {
            string pageId;
            if (!_pageIdSourceOfCBH.TryGetValue(((IGameDataObject)spot).InternalId, out pageId))
                return null;
            var translateId = new TranslationTextId(TranslationCategory.Url, "CBH.SpotPage");
            var format = lang != null ? Translate.Instance[translateId, lang] : Translate.Instance[translateId];
            return string.Format(format, pageId);
        }

        internal static async Task<string> DownloadCBHPage(this FishingSpot fishingSpot, FileInfo cacheFile)
        {
            if (cacheFile.Exists)
                return File.ReadAllText(cacheFile.FullName);
            else
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36");
                    var url = fishingSpot.GetCBHLink("ja");
                    while (true)
                    {
                        var success = false;
                        try
                        {
                            var doc = await client.GetStringAsync(url);
                            Directory.CreateDirectory(cacheFile.DirectoryName);
                            File.WriteAllText(cacheFile.FullName, doc);
                            success = true;
                            return doc;
                        }
                        catch (HttpRequestException)
                        {
                        }
                        finally
                        {
                            if (!success)
                                cacheFile.Delete();
                        }
                    }
                }
            }
        }
    }
}
