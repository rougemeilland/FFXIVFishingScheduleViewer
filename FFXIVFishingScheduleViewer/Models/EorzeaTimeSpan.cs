using System;

namespace FFXIVFishingScheduleViewer.Models
{
    class EorzeaTimeSpan
        : IComparable<EorzeaTimeSpan>
    {
        static EorzeaTimeSpan()
        {
            Zero = new EorzeaTimeSpan(0);
        }

        private EorzeaTimeSpan(long seconds)
        {
            EorzeaTimeSeconds = seconds;
            EorzeaTimeMinutes = EorzeaTimeSeconds / 60;
            EorzeaTimeHours = EorzeaTimeMinutes / 60;
            EorzeaTimeDays = EorzeaTimeHours / 24;
            EorzeaTimeMonths = EorzeaTimeDays / 32;
            EorzeaTimeYears = EorzeaTimeMonths / 12;
            IsZero = seconds == 0;
        }

        public long EorzeaTimeSeconds { get; }

        public long EorzeaTimeMinutes { get; }

        public long EorzeaTimeHours { get; }

        public long EorzeaTimeDays { get; }

        public long EorzeaTimeMonths { get; }

        public long EorzeaTimeYears { get; }

        public bool IsZero { get; }

        public static EorzeaTimeSpan Zero { get; }

        public static EorzeaTimeSpan FromSeconds(long seconds)
        {
            return new EorzeaTimeSpan(seconds);
        }

        public static EorzeaTimeSpan FromMinutes(long minutes)
        {
            return new EorzeaTimeSpan(minutes * 60L);
        }

        public static EorzeaTimeSpan FromHours(long hours)
        {
            return new EorzeaTimeSpan(hours * (60L * 60));
        }

        public static EorzeaTimeSpan FromDays(long days)
        {
            return new EorzeaTimeSpan(days * (60L * 60 * 24));
        }

        public static EorzeaTimeSpan FromMonths(long months)
        {
            return new EorzeaTimeSpan(months * (60L * 60 * 24 * 32));
        }

        public static EorzeaTimeSpan FromYears(long years)
        {
            return new EorzeaTimeSpan(years * (60L * 60 * 24 * 32 * 12));
        }

        public EorzeaTimeSpan Add(EorzeaTimeSpan span)
        {
            return new EorzeaTimeSpan(EorzeaTimeSeconds + span.EorzeaTimeSeconds);
        }

        public EorzeaDateTime Add(EorzeaDateTime time)
        {
            return time.Add(this);
        }

        public EorzeaTimeSpan Subtruct(EorzeaTimeSpan span)
        {
            return new EorzeaTimeSpan(EorzeaTimeSeconds - span.EorzeaTimeSeconds);
        }

        public EorzeaTimeSpan Multiply(int x)
        {
            return new EorzeaTimeSpan(EorzeaTimeSeconds * x);
        }

        public EorzeaTimeSpan Divide(int x)
        {
            return new EorzeaTimeSpan(EorzeaTimeSeconds / x);
        }

        public EorzeaTimeSpan Negate()
        {
            return new EorzeaTimeSpan(-EorzeaTimeSeconds);
        }

        public int CompareTo(EorzeaTimeSpan o)
        {
            if (o == null)
                return 1;
            return EorzeaTimeSeconds.CompareTo(o.EorzeaTimeSeconds);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;
            return EorzeaTimeSeconds == ((EorzeaTimeSpan)o).EorzeaTimeSeconds;
        }

        public override int GetHashCode()
        {
            return EorzeaTimeSeconds.GetHashCode();
        }

        public static EorzeaTimeSpan operator +(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.Add(y);
        }

        public static EorzeaTimeSpan operator -(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.Subtruct(y);
        }

        public static EorzeaTimeSpan operator *(int x, EorzeaTimeSpan y)
        {
            return x.Multiply(y);
        }

        public static EorzeaTimeSpan operator *(EorzeaTimeSpan x, int y)
        {
            return x.Multiply(y);
        }

        public static EorzeaTimeSpan operator /(EorzeaTimeSpan x, int y)
        {
            return x.Divide(y);
        }

        public static EorzeaTimeSpan operator -(EorzeaTimeSpan x)
        {
            return x.Negate();
        }

        public static bool operator <(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator <=(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator >(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator >=(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.CompareTo(y) >= 0;
        }

        public static bool operator ==(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EorzeaTimeSpan x, EorzeaTimeSpan y)
        {
            return !x.Equals(y);
        }


        public override string ToString()
        {
            if (EorzeaTimeSeconds >= 0)
            {
                return string.Format("{0}:{1:D02}:{2:D02}", EorzeaTimeHours, EorzeaTimeMinutes % 60, EorzeaTimeSeconds % 60);
            }
            else
            {
                return string.Format("-{0}:{1:D02}:{2:D02}", -EorzeaTimeHours, -EorzeaTimeMinutes % 60, -EorzeaTimeSeconds % 60);
            }
        }
    }
}