using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class StubApplicationSettings : IApplicationSettings
    {

        public TimeSpan ApplicationUpdateInterval
        {
            get;
            set;
        }

        public TimeSpan BackgroundUpdateInterval
        {
            get;
            set;
        }

        public bool RunUnderLockScreen { get; set; }

        public bool BackgroundUpdateEnabled
        {
            get { return BackgroundUpdateInterval != TimeSpan.Zero; }
        }

        public bool FirstRun
        {
            get;
            set;
        }

        public Uri SuccessTileUri
        {
            get;
            set;
        }

        public Uri FailureTileUri
        {
            get;
            set;
        }

        public Uri UnavailableTileUri
        {
            get;
            set;
        }

        public event EventHandler Updated;

        public void FireUpdated()
        {
            var handler = Updated;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Save()
        {
            TimesSaved++;
        }

        public int TimesSaved
        {
            get;
            private set;
        }

        public bool LoggingEnabled
        {
            get;
            set;
        }


        public NotificationReason NotificationPreference
        {
            get;
            set;
        }


        public TimeSpan NotificationStart { get; set; }

        public TimeSpan NotificationEnd { get; set; }

        public DayOfWeek[] NotificationDays { get; set; }

        public string SuccessColorResource { get; set; }
        public string FailedColorResource { get; set; }
        public string UnavailableColorResource { get; set; }
    }
}
