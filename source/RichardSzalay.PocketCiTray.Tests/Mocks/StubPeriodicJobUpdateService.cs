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
    public class StubPeriodicJobUpdateService : IPeriodicJobUpdateService
    {
        public bool IsRunning
        {
            get;
            private set;
        }

        public TimeSpan DueTime
        {
            get;
            private set;
        }

        public TimeSpan Period
        {
            get;
            private set;
        }

        public bool IsBackgroundServiceRegistered
        {
            get;
            private set;
        }

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            this.DueTime = dueTime;
            this.Period = period;
            this.IsRunning = true;
        }

        public void Stop()
        {
            this.IsRunning = false;
        }

        public void UnregisterBackgroundTask()
        {
            IsBackgroundServiceRegistered = false;
        }

        public void RegisterBackgroundTask()
        {
            IsBackgroundServiceRegistered = true;
        }
    }
}
