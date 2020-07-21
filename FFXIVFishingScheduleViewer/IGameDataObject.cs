using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    interface IGameDataObject
    {
        GameDataObjectId Id { get; }
        IEnumerable<string> CheckTranslation();
    }
}
