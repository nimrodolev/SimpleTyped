using System;

namespace SimplyTyped.Utils
{
    internal class UnixTimestampUtility
    {
        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UnixTimestampToDatetime(long timestamp)
        {
            return _epoch.AddSeconds(timestamp);
        }

        public static long DatetimeToUnixTimestamp(DateTime datetime)
        {
            if (datetime < _epoch)
                throw new ArgumentOutOfRangeException("Datetime value smaller (earlier) than epoch can not be represented as a unix timestamp");

            return (long)(datetime - _epoch).TotalSeconds;
        }
    }
}