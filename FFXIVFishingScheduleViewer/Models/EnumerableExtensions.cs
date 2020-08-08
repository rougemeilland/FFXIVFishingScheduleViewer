using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer.Models
{
    static class EnumerableExtensions
    {
        public static int IndexOf<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, Func<ELEMENT_T, bool> predicate)
        {
            var found =
                source
                    .Select((elementOfSource, index) => new { elementOfSource, index })
                    .Where(item => predicate(item.elementOfSource))
                    .FirstOrDefault();
            return found != null ? found.index : -1;
        }
    }
}
