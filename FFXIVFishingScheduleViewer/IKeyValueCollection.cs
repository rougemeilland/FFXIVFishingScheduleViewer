using System.Collections.Generic;

namespace FFXIVFishingScheduleViewer
{
    interface IKeyValueCollection<VALUE_T>
        : IEnumerable<VALUE_T>
    {
        VALUE_T this[GameDataObjectId key] { get; }
    }
}
