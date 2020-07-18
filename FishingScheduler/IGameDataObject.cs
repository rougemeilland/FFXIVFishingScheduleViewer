using System.Collections.Generic;

namespace FishingScheduler
{
    interface IGameDataObject
    {
        GameDataObjectId Id { get; }
        IEnumerable<string> CheckTranslation();
    }
}
