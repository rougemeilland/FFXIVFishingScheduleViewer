using System;
using System.Collections.Generic;
using System.Linq;

namespace FishingScheduler
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

    }
}
