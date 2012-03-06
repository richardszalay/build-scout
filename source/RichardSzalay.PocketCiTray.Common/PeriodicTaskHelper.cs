using System;

namespace RichardSzalay.PocketCiTray
{
    public class PeriodicTaskHelper
    {
        public static TimeSpan GetNextRunTime(DateTimeOffset? lastRunTime, TimeSpan runInterval, DateTimeOffset now)
        {
            lastRunTime = NormalizeLastRunTime(lastRunTime, now);

            TimeSpan firstUpdate = (lastRunTime.HasValue)
                    ? (lastRunTime.Value + runInterval) - now
                    : TimeSpan.Zero;

            if (firstUpdate < TimeSpan.Zero)
            {
                firstUpdate = TimeSpan.Zero;
            }

            return firstUpdate;
        }

        private static DateTimeOffset? NormalizeLastRunTime(DateTimeOffset? lastRunTime, DateTimeOffset now)
        {
            return (lastRunTime.HasValue && lastRunTime.Value > now)
                ? (DateTimeOffset?)now
                : null;
        }

    }
}
