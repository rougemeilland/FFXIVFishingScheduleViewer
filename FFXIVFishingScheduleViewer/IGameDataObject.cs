using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    interface IGameDataObject
    {
        string InternalId { get; }
        GameDataObjectId Id { get; }
        TranslationTextId NameId { get; }
        IEnumerable<string> CheckTranslation();
    }
}
