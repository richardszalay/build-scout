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
    public class MockJobUpdateService : IJobUpdateService
    {

        public void UpdateAll(TimeSpan timeout)
        {
            UpdateRequested = true;
        }

        public event EventHandler Complete;

        public void Cancel()
        {
            CancelRequested = true;
        }

        public void FireComplete()
        {
            var handler = Complete;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void FireStarted()
        {
            var handler = Started;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public bool CancelRequested { get; set; }
        public bool UpdateRequested { get; set; }

        public event EventHandler Started;

        public DateTimeOffset? LastUpdateTime
        {
            get;
            set;
        }
    }
}
