using System;

namespace FFXIVFishingScheduleViewer
{
    static class TranslationCategoryExtensions
    {
        public static string ToInternalKeyText(this TranslationCategory category)
        {
            switch (category)
            {
                case TranslationCategory.Weather:
                    return "weather";
                case TranslationCategory.Language:
                    return "lang";
                case TranslationCategory.AreaGroup:
                    return "areaGroup";
                case TranslationCategory.Area:
                    return "area";
                case TranslationCategory.FishingSpot:
                    return "spot";
                case TranslationCategory.FishingBait:
                    return "bait";
                case TranslationCategory.FishMemo:
                    return "fishMemo";
                case TranslationCategory.Fish:
                    return "fish";
                case TranslationCategory.Action:
                    return "action";
                case TranslationCategory.License:
                    return "license";
                case TranslationCategory.Url:
                    return "url";
                case TranslationCategory.GUIText:
                    return "guiText";
                case TranslationCategory.Generic:
                    return "generic";
                default:
                    throw new Exception();
            }
        }
    }
}
