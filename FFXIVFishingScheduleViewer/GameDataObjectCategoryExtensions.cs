using System;

namespace FFXIVFishingScheduleViewer
{
    static class GameDataObjectCategoryExtensions
    {
        public static string ToInternalKeyText(this GameDataObjectCategory category)
        {
            switch (category)
            {
                case GameDataObjectCategory.AreaGroup:
                    return "areaGroup";
                case GameDataObjectCategory.Area:
                    return "area";
                case GameDataObjectCategory.FishingSpot:
                    return "spot";
                case GameDataObjectCategory.FishingBait:
                    return "bait";
                case GameDataObjectCategory.Fish:
                    return "fish";
                default:
                    throw new Exception();
            }
        }
    }
}
