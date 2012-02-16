using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray
{
    public class Bootstrap
    {
        private readonly IPeriodicJobUpdateService periodicJobUpdateService;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IApplicationSettings applicationSettings;
        private readonly IClock clock;

        public Bootstrap(IPeriodicJobUpdateService periodicJobUpdateService, IJobUpdateService jobUpdateService, IApplicationSettings applicationSettings, IClock clock)
        {
            this.periodicJobUpdateService = periodicJobUpdateService;
            this.jobUpdateService = jobUpdateService;
            this.applicationSettings = applicationSettings;
            this.clock = clock;
        }

        public void Run()
        {
            applicationSettings.Updated += OnSettingsUpdated;
            
            StartPeriodicUpdateService();
        }

        private void StartPeriodicUpdateService()
        {
            if (applicationSettings.ApplicationUpdateInterval > TimeSpan.Zero)
            {
                TimeSpan firstUpdate = PeriodicTaskHelper.GetNextRunInterval(jobUpdateService.LastUpdateTime,
                    applicationSettings.ApplicationUpdateInterval, clock.UtcNow);

                periodicJobUpdateService.Start(firstUpdate, applicationSettings.ApplicationUpdateInterval);
            }

            if (applicationSettings.BackgroundUpdateInterval > TimeSpan.Zero)
            {
                periodicJobUpdateService.RegisterBackgroundTask();
            }
        }

        private void OnSettingsUpdated(object sender, EventArgs e)
        {
            if (applicationSettings.ApplicationUpdateInterval == TimeSpan.Zero)
            {
                periodicJobUpdateService.Stop();
            }

            if (applicationSettings.BackgroundUpdateInterval == TimeSpan.Zero)
            {
                periodicJobUpdateService.UnregisterBackgroundTask();
            }

            StartPeriodicUpdateService();
        }
    }
}
