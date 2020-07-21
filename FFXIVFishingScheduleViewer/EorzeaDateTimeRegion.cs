using System;

namespace FFXIVFishingScheduleViewer
{
    class EorzeaDateTimeRegion
    {
        public EorzeaDateTimeRegion(EorzeaDateTime begin, EorzeaDateTime end)
            : this(begin, end, end - begin)
        {

        }

        public EorzeaDateTimeRegion(EorzeaDateTime begin, EorzeaTimeSpan span)
            : this(begin, begin + span, span)
        {
        }

        private EorzeaDateTimeRegion(EorzeaDateTime begin, EorzeaDateTime end, EorzeaTimeSpan span)
        {
            if (begin >= end)
                throw new ArgumentException();
            Begin = begin;
            End = end;
            Span = span;
            IsEmpty = span.IsZero;
        }

        public EorzeaDateTime Begin { get; }
        public EorzeaDateTime End { get; }
        public EorzeaTimeSpan Span { get; }
        public bool IsEmpty { get; }
    }
}
