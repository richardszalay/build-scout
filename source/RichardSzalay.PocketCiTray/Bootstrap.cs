using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray
{
    public class Bootstrap
    {
        private readonly IPeriodicJobUpdateService periodicJobUpdateService;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IApplicationSettings applicationSettings;
        private readonly IClock clock;
        private readonly IMessageBoxFacade messageBox;
        private readonly IMutexService mutexService;
        private Mutex applicationMutex;

        public Bootstrap(IPeriodicJobUpdateService periodicJobUpdateService, IJobUpdateService jobUpdateService, IApplicationSettings applicationSettings, IClock clock, IMessageBoxFacade messageBox, IMutexService mutexService)
        {
            this.periodicJobUpdateService = periodicJobUpdateService;
            this.jobUpdateService = jobUpdateService;
            this.applicationSettings = applicationSettings;
            this.clock = clock;
            this.messageBox = messageBox;
            this.mutexService = mutexService;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Startup()
        {
            applicationMutex = mutexService.GetOwned(MutexNames.ForegroundApplication, TimeSpan.FromMilliseconds(100));

            if (applicationSettings.FirstRun)
            {
                PerformFirstRun();
            }

            applicationSettings.Updated += OnSettingsUpdated;

            StartPeriodicUpdateService();
        }

        public void Continue()
        {
            applicationMutex = mutexService.GetOwned(MutexNames.ForegroundApplication, TimeSpan.FromMilliseconds(100));
        }

        private void PerformFirstRun()
        {
            var result = messageBox.Show(Strings.EnableBackgroundTaskPrompt, String.Empty, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                applicationSettings.BackgroundUpdateEnabled = true;
            }

            applicationSettings.FirstRun = false;
            applicationSettings.Save();
        }

        private void StartPeriodicUpdateService()
        {
            if (applicationSettings.ApplicationUpdateInterval > TimeSpan.Zero)
            {
                TimeSpan firstUpdate = PeriodicTaskHelper.GetNextRunInterval(jobUpdateService.LastUpdateTime,
                    applicationSettings.ApplicationUpdateInterval, clock.UtcNow);

                periodicJobUpdateService.Start(firstUpdate, applicationSettings.ApplicationUpdateInterval);
            }

            if (applicationSettings.BackgroundUpdateEnabled)
            {
                periodicJobUpdateService.RegisterBackgroundTask();
            }
        }

        private void OnSettingsUpdated(object sender, EventArgs e)
        {
            periodicJobUpdateService.Stop();

            if (!applicationSettings.BackgroundUpdateEnabled)
            {
                periodicJobUpdateService.UnregisterBackgroundTask();
            }

            StartPeriodicUpdateService();
        }

        public void Shutdown()
        {
            applicationMutex.ReleaseMutex();
        }
    }
}
