using System.Collections.Generic;

namespace FishingScheduler
{
    interface IKeyValueCollection<VALUE_T>
        : IEnumerable<VALUE_T>
    {
        VALUE_T this[GameDataObjectId key] { get; }
    }
}
