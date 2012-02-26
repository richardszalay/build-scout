using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationSettings
    {
        TimeSpan ApplicationUpdateInterval { get; set; }
        TimeSpan BackgroundUpdateInterval { get; set; }
        bool BackgroundUpdateEnabled { get; set; }
        bool FirstRun { get; set; }

        Uri SuccessTileUri { get; set; }
        Uri FailureTileUri { get; set; }
        Uri UnavailableTileUri { get; set; }

        event EventHandler Updated;
        void Save();

        bool LoggingEnabled { get; set; }

        NotificationReason NotificationPreference { get; set; }

        TimeSpan NotificationStart { get; set; }
        TimeSpan NotificationEnd { get; set; }
        DayOfWeek[] NotificationDays { get; set; }
    }
}