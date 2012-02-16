using System;

namespace RichardSzalay.PocketCiTray
{
    public class PeriodicTaskHelper
    {
        public static TimeSpan GetNextRunInterval(DateTimeOffset? lastRunTime, TimeSpan runInterval, DateTimeOffset now)
        {
            TimeSpan firstUpdate = (lastRunTime.HasValue)
                    ? (lastRunTime.Value + runInterval) - now
                    : TimeSpan.Zero;

            if (firstUpdate < TimeSpan.Zero)
            {
                firstUpdate = TimeSpan.Zero;
            }

            return firstUpdate;
        }

    }
}
