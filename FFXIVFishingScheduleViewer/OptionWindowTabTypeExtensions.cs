namespace FFXIVFishingScheduleViewer
{
    static class OptionWindowTabTypeExtensions
    {
        public static string ToInternalId(this OptionWindowTabType value, string defaultValue)
        {
            switch (value)
            {
                case OptionWindowTabType.Generic:
                    return "generic";
                case OptionWindowTabType.FishingChanceList:
                    return "fishingChanceList";
                case OptionWindowTabType.Update:
                    return "update";
                default:
                    return defaultValue;
            }
        }

        public static OptionWindowTabType ToOptionWindowTab(this string value, OptionWindowTabType defaultValue)
        {
            switch (value)
            {
                case "generic":
                    return OptionWindowTabType.Generic;
                case "fishingChanceList":
                    return OptionWindowTabType.FishingChanceList;
                case "update":
                    return OptionWindowTabType.Update;
                default:
                    return defaultValue;
            }
        }
    }
}
