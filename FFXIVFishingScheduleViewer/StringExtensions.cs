namespace FFXIVFishingScheduleViewer
{
    static class StringExtensions
    {
        public static string SimpleEncode(this string s)
        {
            return s
                .Replace("&", "&amp;")
                .Replace("\t", "&#9;")
                .Replace("\n", "&#10;")
                .Replace("\f", "&#12;")
                .Replace("\r", "&#13;");
        }

        public static string SimpleDecode(this string s)
        {
            return s
                .Replace("&#13;", "\r")
                .Replace("&#12;", "\f")
                .Replace("&#10;", "\n")
                .Replace("&#9;", "\t")
                .Replace("&amp;", "&");
        }
    }
}
