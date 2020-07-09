using System.Collections.Generic;

namespace FishingScheduler
{
    interface IKeyValueCollection<KEY_T, VALUE_T>
        : IEnumerable<VALUE_T>
    {
        VALUE_T this[KEY_T key] { get; }
    }
}
