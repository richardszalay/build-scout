using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WebAnalytics;

namespace RichardSzalay.PocketCiTray.Services
{
    public class CommonTrackingEvents
    {
    
    }

    public class TrackingCodes
    {
        public const string Foreground = "UA-29991961-1";
        public const string Background = "UA-29991961-2";
    }

    public interface ITrackingService
    {
        void Track(string category, string name, string value);
    }

    public class WebAnalyticsTrackingService : ITrackingService
    {
        private readonly Action<AnalyticsEvent> log;

        public WebAnalyticsTrackingService(Action<AnalyticsEvent> log)
        {
            this.log = log;
        }

        public void Track(string category, string name, string value)
        {
            log(new AnalyticsEvent
            {
                Category = category,
                Name = name,
                ObjectName = value
            });
        }
    }

    public class NullTrackingService : ITrackingService
    {
        public void Track(string category, string name, string value)
        {
        }
    }
}
