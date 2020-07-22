using System.Globalization;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    static class AboutExtensions
    {
        public static int CompareVersionString(this string version1, string version2)
        {
            var sequence1 = version1.Split('.').Select(s => int.Parse(s, NumberStyles.None)).Concat(Enumerable.Repeat(-1, 4)).Take(4);
            var sequence2 = version2.Split('.').Select(s => int.Parse(s, NumberStyles.None)).Concat(Enumerable.Repeat(-1, 4)).Take(4);
            return sequence1.Zip(sequence2, (x, y) => x.CompareTo(y)).Where(v => v != 0).FirstOrDefault();
        }
    }
}
