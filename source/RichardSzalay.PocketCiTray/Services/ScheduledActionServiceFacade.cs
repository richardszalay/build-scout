using System;
using Microsoft.Phone.Scheduler;
using System.Diagnostics;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ScheduledActionServiceFacade : IScheduledActionServiceFacade
    {
        public void Add(string name, string description)
        {
            ScheduledActionService.Add(new PeriodicTask(name)
            {
                Description = description
            });
        }

        public void Remove(string name)
        {
            if (ScheduledActionService.Find(name) != null)
            {
                ScheduledActionService.Remove(name);
            }
        }

        public void LaunchForTest(string name, TimeSpan delay)
        {
            // Interface implementations cannot be conditional
            LaunchForTestInternal(name, delay);
        }

        [Conditional("DEBUG_AGENT")]
        private static void LaunchForTestInternal(string name, TimeSpan delay)
        {
            ScheduledActionService.LaunchForTest(name, delay);
        }
    }
}