using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVFishingScheduleViewer.Strings;

namespace FFXIVFishingScheduleViewer.Models
{
    static class DateTimeExtensions
    {
        private const double TIME_ADJUST = 1278950400;
        private const double TIME_GAME = 144;
        private const double TIME_EARTH = 7;
        private static DateTime _baseDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static TranslationTextId _relativeDateTimeFormatIdOfToday = new TranslationTextId(TranslationCategory.Generic, "DateTime.Today");
        private static TranslationTextId _relativeDateTimeFormatIdOfTomorrow = new TranslationTextId(TranslationCategory.Generic, "DateTime.Tomorrow");
        private static TranslationTextId _relativeDateTimeFormatIdOfDaysAfter = new TranslationTextId(TranslationCategory.Generic, "DateTime.DaysAfter");

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

        public static EorzeaTimeSpan ToEorzeaTimeSpan(this TimeSpan duration)
        {
            return EorzeaTimeSpan.FromSeconds((long)Math.Floor(duration.TotalSeconds * TIME_GAME / TIME_EARTH + 0.5));
        }

        public static TimeSpan  ToEarthTimeSpan(this EorzeaTimeSpan duration)
        {
            return TimeSpan.FromSeconds(duration.EorzeaTimeSeconds * TIME_EARTH / TIME_GAME);
        }

        public static string FormatEorzeaTimeRegion(this EorzeaDateTimeRegion region, EorzeaDateTimeRegion wholeRegion, DateTime now)
        {
            if (region.Begin <= wholeRegion.Begin)
            {
                if (region.End >= wholeRegion.End)
                    return "";
                else
                {
                    var end = region.End - EorzeaTimeSpan.FromSeconds(1);
                    return string.Format("- {0}", end.GetRelativeDateTimeExpression(now));
                }
            }
            else
            {
                if (region.End >= wholeRegion.End)
                {
                    var start = region.Begin;
                    return string.Format("{0} -", start.GetRelativeDateTimeExpression(now));
                }
                else
                {
                    var start = region.Begin;
                    var end = region.End - EorzeaTimeSpan.FromSeconds(1);
                    return string.Format("{0} - {1}", start.GetRelativeDateTimeExpression(now), end.GetRelativeDateTimeExpression(now));
                }
            }
        }

        public static string FormatLocalTimeRegion(this EorzeaDateTimeRegion region, EorzeaDateTimeRegion wholeRegion, DateTime now)
        {
            if (region.Begin <= wholeRegion.Begin)
            {
                if (region.End >= wholeRegion.End)
                    return "";
                else
                {
                    var end = region.End.ToEarthDateTime() - TimeSpan.FromSeconds(1);
                    return string.Format("- {0}", end.GetRelativeDateTimeExpression(now));
                }
            }
            else
            {
                if (region.End >= wholeRegion.End)
                {
                    var start = region.Begin.ToEarthDateTime();
                    return string.Format("{0} -", start.GetRelativeDateTimeExpression(now));
                }
                else
                {
                    var start = region.Begin.ToEarthDateTime();
                    var end = region.End.ToEarthDateTime() - TimeSpan.FromSeconds(1);
                    return string.Format("{0} - {1}", start.GetRelativeDateTimeExpression(now), end.GetRelativeDateTimeExpression(now));
                }
            }
        }

        public static string GetRelativeDateTimeExpression(this DateTime dateTime, DateTime now)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException();
            if (now.Kind != DateTimeKind.Utc)
                throw new ArgumentException();
            var localDateTime = dateTime.ToLocalTime();
            var localDatetimeNow = now.ToLocalTime();
            var localDate = localDateTime.Date;
            var localDateNow = localDatetimeNow.Date;
            var days = (localDate - localDateNow).TotalDays;
            if (days < 0.5)
            {
                // dateTime が now と同じ日付の場合
                return
                    string.Format(
                        Translate.Instance[_relativeDateTimeFormatIdOfToday],
                        string.Format("{0:D2}:{1:D2}:{2:D2}", localDateTime.Hour, localDateTime.Minute, localDateTime.Second));
            }
            else if (days < 1.5)
            {
                // dateTime が now の翌日の場合
                return
                    string.Format(
                        Translate.Instance[_relativeDateTimeFormatIdOfTomorrow],
                        string.Format("{0:D2}:{1:D2}:{2:D2}", localDateTime.Hour, localDateTime.Minute, localDateTime.Second));
            }
            else
            {
                // dateTime が now の2日後以降の場合
                return
                    string.Format(
                        Translate.Instance[_relativeDateTimeFormatIdOfDaysAfter],
                        days.ToString("F0"),
                        string.Format("{0:D2}:{1:D2}:{2:D2}", localDateTime.Hour, localDateTime.Minute, localDateTime.Second));
            }
        }

        public static string GetRelativeDateTimeExpression(this EorzeaDateTime dateTime, DateTime now)
        {
            if (now.Kind != DateTimeKind.Utc)
                throw new ArgumentException();
            var eorzeaDateTime = dateTime;
            var eorzeaDatetimeNow = now.ToEorzeaDateTime();
            var localDate = eorzeaDateTime.GetStartOfDay();
            var localDateNow = eorzeaDatetimeNow.GetStartOfDay();
            var days = (localDate - localDateNow).EorzeaTimeDays;
            if (days <= 0)
            {
                // dateTime が now と同じ日付の場合
                return
                    string.Format(
                        Translate.Instance[_relativeDateTimeFormatIdOfToday],
                        string.Format("{0:D2}:{1:D2}", eorzeaDateTime.Hour, eorzeaDateTime.Minute));
            }
            else if (days == 1)
            {
                // dateTime が now の翌日の場合
                return
                    string.Format(
                        Translate.Instance[_relativeDateTimeFormatIdOfTomorrow],
                        string.Format("{0:D2}:{1:D2}", eorzeaDateTime.Hour, eorzeaDateTime.Minute));
            }
            else
            {
                // dateTime が now の2日後以降の場合
                return
                    string.Format(
                        Translate.Instance[_relativeDateTimeFormatIdOfDaysAfter],
                        days,
                        string.Format("{0:D2}:{1:D2}", eorzeaDateTime.Hour, eorzeaDateTime.Minute));
            }
        }
    }
}
