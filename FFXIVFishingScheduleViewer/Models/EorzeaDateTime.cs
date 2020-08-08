using System;

namespace FFXIVFishingScheduleViewer.Models
{
    class EorzeaDateTime
        : IComparable<EorzeaDateTime>
    {
        private EorzeaDateTime(long eorzeaEpochSeconds)
        {
            EpochSeconds = eorzeaEpochSeconds;
            if (EpochSeconds < 0)
                throw new ArgumentException();
            EpochMinutes = EpochSeconds / 60;
            EpochHours = EpochMinutes / 60;
            EpochDays = EpochHours / 24;
            EpochMonths = EpochDays / 32;
            EpochYears = EpochMonths / 12;
            Second = (int)(EpochSeconds % 60);
            Minute = (int)(EpochMinutes % 60);
            Hour = (int)(EpochHours % 24);
            Day = (int)(EpochDays % 32) + 1;
            Month = (int)(EpochMonths % 12) + 1;
            Year = (int)EpochYears;
        }

        public static EorzeaDateTime From(int year, int month, int day, int hour, int minute, int second)
        {
            return new EorzeaDateTime(((((year * 12L + month - 1) * 32 + day - 1) * 24 + hour) * 60 + minute) * 60 + second);
        }

        public static EorzeaDateTime FromEpochSeconds(long seconds)
        {
            return new EorzeaDateTime(seconds);
        }

        public static EorzeaDateTime FromEpochMinutes(long minutes)
        {
            return new EorzeaDateTime(minutes * 60L);
        }

        public static EorzeaDateTime FromEpochHours(long hours)
        {
            return new EorzeaDateTime(hours * (60L * 60));
        }

        public static EorzeaDateTime FromEpochDays(long days)
        {
            return new EorzeaDateTime(days * (60L * 60 * 24));
        }

        public static EorzeaDateTime FromEpochMonths(long months)
        {
            return new EorzeaDateTime(months * (60L * 60 * 24 * 32));
        }

        public static EorzeaDateTime FromEpochYears(long years)
        {
            return new EorzeaDateTime(years * (60L * 60 * 24 * 32 * 12));
        }

        public int Year { get; }

        public int Month { get; }

        public int Day { get; }

        public int Hour { get; }

        public int Minute { get; }

        public int Second { get; }

        public long EpochYears { get; }

        public long EpochMonths { get; }

        public long EpochDays { get; }

        public long EpochHours { get; }

        public long EpochMinutes { get; }

        public long EpochSeconds { get; }

        public EorzeaDateTime GetStartOfDay()
        {
            var x = 24 * 60 * 60;
            return new EorzeaDateTime(EpochSeconds / x * x);
        }

        public EorzeaDateTime GetStartOf8Hour()
        {
            var x = 8 * 60 * 60;
            return new EorzeaDateTime(EpochSeconds / x * x);
        }

        public EorzeaDateTime GetStartOfHour()
        {
            var x = 60 * 60;
            return new EorzeaDateTime(EpochSeconds / x * x);
        }

        public EorzeaDateTime GetStartOfMinute()
        {
            var x = 60;
            return new EorzeaDateTime(EpochSeconds / x * x);
        }

        public EorzeaDateTime Add(EorzeaTimeSpan span)
        {
            return new EorzeaDateTime(EpochSeconds + span.EorzeaTimeSeconds);
        }

        public EorzeaDateTime Subtruct(EorzeaTimeSpan span)
        {
            return new EorzeaDateTime(EpochSeconds - span.EorzeaTimeSeconds);
        }

        public EorzeaTimeSpan Subtruct(EorzeaDateTime time)
        {
            return EorzeaTimeSpan.FromSeconds(EpochSeconds - time.EpochSeconds);
        }

        public int CompareTo(EorzeaDateTime o)
        {
            if (o == null)
                return 1;
            return EpochSeconds.CompareTo(o.EpochSeconds);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return EpochSeconds == ((EorzeaDateTime)o).EpochSeconds;
        }

        public override int GetHashCode()
        {
            return EpochSeconds.GetHashCode();
        }

        public static EorzeaDateTime operator + (EorzeaDateTime x, EorzeaTimeSpan y)
        {
            return x.Add(y);
        }

        public static EorzeaDateTime operator +(EorzeaTimeSpan x, EorzeaDateTime y)
        {
            return y.Add(x);
        }

        public static EorzeaDateTime operator -(EorzeaDateTime x, EorzeaTimeSpan y)
        {
            return x.Subtruct(y);
        }

        public static EorzeaTimeSpan operator -(EorzeaDateTime x, EorzeaDateTime y)
        {
            return x.Subtruct(y);
        }

        public static bool operator <(EorzeaDateTime x, EorzeaDateTime y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(EorzeaDateTime x, EorzeaDateTime y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >(EorzeaDateTime x, EorzeaDateTime y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator >=(EorzeaDateTime x, EorzeaDateTime y)
        {
            return x.CompareTo(y) >= 0;
        }

        public static bool operator ==(EorzeaDateTime x, EorzeaDateTime y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EorzeaDateTime x, EorzeaDateTime y)
        {
            return !x.Equals(y);
        }

        public override string ToString()
        {
            return string.Format("{0}-{1:D02}-{2:D02} {3:D02}:{4:D02}:{5:D02}", Year, Month, Day, Hour, Minute, Second);
        }
    }
}
