using System;

namespace GameMasterBot.Utils
{
    public static class DateUtils
    {
        public static long ToUnixEpochTime(DateTime date) => (long) date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}