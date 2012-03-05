using System;
using System.ComponentModel;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationSettings : INotifyPropertyChanged
    {
        TimeSpan ApplicationUpdateInterval { get; set; }
        TimeSpan BackgroundUpdateInterval { get; set; }

        bool RunUnderLockScreen { get; set; }
        bool BackgroundUpdateEnabled { get; }
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
        
        string SuccessColorResource { get; set; }
        string FailedColorResource { get; set; }
        string UnavailableColorResource { get; set; }
    }
}