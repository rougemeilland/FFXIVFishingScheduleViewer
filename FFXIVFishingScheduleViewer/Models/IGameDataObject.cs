using FFXIVFishingScheduleViewer.Strings;
using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer.Models
{
    interface IGameDataObject
    {
        string InternalId { get; }
        GameDataObjectId Id { get; }
        TranslationTextId NameId { get; }
        IEnumerable<string> CheckTranslation();
    }
}
