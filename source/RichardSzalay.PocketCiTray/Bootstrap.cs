using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using RichardSzalay.PocketCiTray.Services;
using WP7Contrib.Logging;

namespace RichardSzalay.PocketCiTray
{
    public class Bootstrap
    {
        private readonly IApplicationSettings applicationSettings;
        private readonly IClock clock;
        private readonly IMessageBoxFacade messageBox;
        private readonly IMutexService mutexService;
        private readonly ILogManager logManager;
        private readonly ISettingsApplier settingsApplier;
        private readonly IJobRepository jobRepository;
        private readonly IPeriodicJobUpdateService periodicJobUpdateService;
        private IDisposable applicationMutex;

        public Bootstrap(IApplicationSettings applicationSettings, IClock clock, 
            IMessageBoxFacade messageBox, IMutexService mutexService,
            ILogManager logManager, ISettingsApplier settingsApplier,
            IJobRepository jobRepository, IPeriodicJobUpdateService periodicJobUpdateService)
        {
            this.applicationSettings = applicationSettings;
            this.clock = clock;
            this.messageBox = messageBox;
            this.mutexService = mutexService;
            this.logManager = logManager;
            this.jobRepository = jobRepository;
            this.periodicJobUpdateService = periodicJobUpdateService;
            this.settingsApplier = settingsApplier;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Startup()
        {
            applicationMutex = mutexService.GetOwned(MutexNames.ForegroundApplication, TimeSpan.FromMilliseconds(100));

            jobRepository.Initialize();

            if (applicationSettings.FirstRun)
            {
                PerformFirstRun();
            }

            settingsApplier.ApplyToSession(applicationSettings);
        }

        public void Continue()
        {
            applicationMutex = mutexService.GetOwned(MutexNames.ForegroundApplication, TimeSpan.FromMilliseconds(100));
        }

        private void PerformFirstRun()
        {
            if (periodicJobUpdateService.CanRegisterBackgroundTask)
            {
                var result = messageBox.Show(Strings.EnableBackgroundTaskPrompt, String.Empty, MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    applicationSettings.BackgroundUpdateInterval = TimeSpan.Zero;
                }
            }
            else
            {
                applicationSettings.BackgroundUpdateInterval = TimeSpan.Zero;
            }

            applicationSettings.FirstRun = false;
            applicationSettings.Save();
        }

        public void Shutdown()
        {
            logManager.Disable();

            applicationMutex.Dispose();
        }
    }
}
