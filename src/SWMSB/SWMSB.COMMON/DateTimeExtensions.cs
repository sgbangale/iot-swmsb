using System;

namespace SWMSB.COMMON
{
    public static class DateTimeExtensions
    {
     
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long EpochTime(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
        public static DateTime MilitaryTimeNow(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime EpochTimeToUtcDateTime(this long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        public static DateTime EpochTimeToLocalDateTime(this long unixTime)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local)).AddSeconds(unixTime);
        }


    }
}
