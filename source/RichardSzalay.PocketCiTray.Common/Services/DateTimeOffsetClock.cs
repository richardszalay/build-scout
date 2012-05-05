using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public class DateTimeOffsetClock : IClock
    {
        public DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }


        public DateTimeOffset LocalNow
        {
            get { return DateTimeOffset.Now; }
        }
    }
}