using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer
{
    class EorzeaDateTimeHourRegions
    {
        private ISet<long> _imp;


        public EorzeaDateTimeHourRegions()
            : this(new HashSet<long>())
        {

        }

        public EorzeaDateTimeHourRegions(IEnumerable<EorzeaDateTimeRegion> regions)
            : this(ToSetOfHour(regions))
        {
        }

        private EorzeaDateTimeHourRegions(ISet<long> set)
        {
            _imp = set;
            DateTimeRegions = ToEorzeaDateTimeRegions(_imp);
            if (set.Any())
            {
                Begin = EorzeaDateTime.FromEpochHours(set.Min());
                End = EorzeaDateTime.FromEpochHours(set.Max()) + EorzeaTimeSpan.FromHours(1);
                IsEmpty = false;
            }
            else
            {
                Begin = EorzeaDateTime.FromEpochHours(0);
                End = EorzeaDateTime.FromEpochHours(0);
                IsEmpty = true;
            }
        }

        public IEnumerable<EorzeaDateTimeRegion> DateTimeRegions { get; }
        public EorzeaDateTime Begin { get; }
        public EorzeaDateTime End { get; }
        public bool IsEmpty { get; }

        public EorzeaDateTimeHourRegions Union(EorzeaDateTimeHourRegions other)
        {
            var set = new HashSet<long>(_imp);
            set.UnionWith(other._imp);
            return new EorzeaDateTimeHourRegions(set);
        }

        public EorzeaDateTimeHourRegions Intersect(EorzeaDateTimeHourRegions other)
        {
            var set = new HashSet<long>(_imp);
            set.IntersectWith(other._imp);
            return new EorzeaDateTimeHourRegions(set);
        }

        public EorzeaDateTimeHourRegions Except(EorzeaDateTimeHourRegions other)
        {
            var set = new HashSet<long>(_imp);
            set.ExceptWith(other._imp);
            return new EorzeaDateTimeHourRegions(set);
        }

        public bool Contains(EorzeaDateTime time)
        {
            return _imp.Contains(time.EpochHours);
        }

        private static ISet<long> ToSetOfHour(IEnumerable<EorzeaDateTimeRegion> regions)
        {
            var set = new HashSet<long>();
            foreach (var region in regions)
            {
                for (var hour = region.Begin.EpochHours; hour < region.End.EpochHours; hour++)
                    set.Add(hour);
            }
            return set;
        }

        private static IEnumerable<EorzeaDateTimeRegion> ToEorzeaDateTimeRegions(IEnumerable<long> values)
        {
            var collection = new List<EorzeaDateTimeRegion>();
            var isFirst = true;
            var startValue = 0L;
            var previousValue = 0L;
            foreach (var value in values.OrderBy(x => x))
            {
                if (isFirst)
                {
                    startValue = value;
                }
                else if (value == previousValue + 1)
                {
                    // NOP
                }
                else
                {
                    collection.Add(new EorzeaDateTimeRegion(EorzeaDateTime.FromEpochHours(startValue), EorzeaDateTime.FromEpochHours(previousValue + 1)));
                    startValue = value;
                }
                previousValue = value;
                isFirst = false;
            }
            collection.Add(new EorzeaDateTimeRegion(EorzeaDateTime.FromEpochHours(startValue), EorzeaDateTime.FromEpochHours(previousValue + 1)));
            return collection;
        }

    }
}
