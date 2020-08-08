using System;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVFishingScheduleViewer.Models
{
    static class DateTimeExtensions
    {
        private const double TIME_ADJUST = 1278950400;
        private const double TIME_GAME = 144;
        private const double TIME_EARTH = 7;

        private static DateTime _baseDateTime;

        static DateTimeExtensions()
        {
            _baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime ToEarthDateTime(this EorzeaDateTime dateTime)
        {
            return _baseDateTime.AddSeconds(dateTime.EpochSeconds * TIME_EARTH / TIME_GAME + TIME_ADJUST);
        }

        public static EorzeaDateTime ToEorzeaDateTime(this DateTime dateTime)
        {
            return EorzeaDateTime.FromEpochSeconds((long)Math.Floor(((dateTime - _baseDateTime).TotalSeconds - TIME_ADJUST) * TIME_GAME / TIME_EARTH));
        }

        public static double GetEpochSeconds(this DateTime dateTime)
        {
            return (dateTime - _baseDateTime).TotalSeconds;
        }

        public static EorzeaTimeSpan Multiply(this int x, EorzeaTimeSpan y)
        {
            return y.Multiply(x);
        }

        public static string FormatEorzeaTimeRegion(this EorzeaDateTimeRegion region, EorzeaDateTimeRegion wholeRegion)
        {
            if (region.Begin <= wholeRegion.Begin)
            {
                if (region.End >= wholeRegion.End)
                    return "";
                else
                {
                    var end = region.End - EorzeaTimeSpan.FromSeconds(1);
                    return string.Format("- {0:D02}:{1:D02}", end.Hour, end.Minute);
                }
            }
            else
            {
                if (region.End >= wholeRegion.End)
                {
                    var start = region.Begin;
                    return string.Format("{0:D02}:{1:D02} -", start.Hour, start.Minute);
                }
                else
                {
                    var start = region.Begin;
                    var end = region.End - EorzeaTimeSpan.FromSeconds(1);
                    return string.Format("{0:D02}:{1:D02} - {2:D02}:{3:D02}", start.Hour, start.Minute, end.Hour, end.Minute);
                }
            }
        }

        public static string FormatLocalTimeRegion(this EorzeaDateTimeRegion region, EorzeaDateTimeRegion wholeRegion)
        {
            if (region.Begin <= wholeRegion.Begin)
            {
                if (region.End >= wholeRegion.End)
                    return "";
                else
                {
                    var end = region.End.ToEarthDateTime().ToLocalTime() - TimeSpan.FromSeconds(1);
                    return string.Format("- {0:D02}:{1:D02}:{2:D02}", end.Hour, end.Minute, end.Second);
                }
            }
            else
            {
                if (region.End >= wholeRegion.End)
                {
                    var start = region.Begin.ToEarthDateTime().ToLocalTime();
                    return string.Format("{0:D02}:{1:D02}:{2:D02} -", start.Hour, start.Minute, start.Second);
                }
                else
                {
                    var start = region.Begin.ToEarthDateTime().ToLocalTime();
                    var end = region.End.ToEarthDateTime().ToLocalTime() - TimeSpan.FromSeconds(1);
                    return string.Format("{0:D02}:{1:D02}:{2:D02} - {3:D02}:{4:D02}:{5:D02}", start.Hour, start.Minute, start.Second, end.Hour, end.Minute, end.Second);
                }
            }
        }

    }
}
