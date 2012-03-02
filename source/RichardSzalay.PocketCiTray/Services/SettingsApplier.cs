﻿using System;
using WP7Contrib.Logging;
using System.Windows.Media;
using Microsoft.Phone.Shell;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ISettingsApplier
    {
        void ApplyToSession(IApplicationSettings applicationSettings);
        void Rebuild(IApplicationSettings applicationSettings);
    }

    public class SettingsApplier : ISettingsApplier
    {
        private readonly ILogManager logManager;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IPeriodicJobUpdateService periodicJobUpdateService;
        private readonly IClock clock;
        private readonly IApplicationResourceFacade applicationResourceFacade;
        private readonly IPhoneApplicationServiceFacade phoneApplicationService;

        public SettingsApplier(ILogManager logManager, IJobUpdateService jobUpdateService,
            IPeriodicJobUpdateService periodicJobUpdateService, IClock clock,
            IApplicationResourceFacade applicationResourceFacade, IPhoneApplicationServiceFacade phoneApplicationService)
        {
            this.logManager = logManager;
            this.jobUpdateService = jobUpdateService;
            this.periodicJobUpdateService = periodicJobUpdateService;
            this.clock = clock;
            this.applicationResourceFacade = applicationResourceFacade;
            this.phoneApplicationService = phoneApplicationService;
        }

        public void ApplyToSession(IApplicationSettings applicationSettings)
        {
            StartPeriodicUpdateService(applicationSettings);

            if (applicationSettings.LoggingEnabled)
            {
                logManager.Enable();
            }

            CopyColorFrom(applicationSettings.SuccessColorResource, "BuildResultSuccessBrush");
            CopyColorFrom(applicationSettings.FailedColorResource, "BuildResultFailedBrush");
            CopyColorFrom(applicationSettings.UnavailableColorResource, "BuildResultUnavailableBrush");

            phoneApplicationService.ApplicationIdleDetectionMode = (applicationSettings.RunUnderLockScreen)
                ? IdleDetectionMode.Disabled
                : IdleDetectionMode.Enabled;
        }

        private void CopyColorFrom(string fromResource, string toResource)
        {
            var fromBrush = applicationResourceFacade.GetResource<SolidColorBrush>(fromResource);
            var toBrush = applicationResourceFacade.GetResource<SolidColorBrush>(toResource);

            toBrush.Color = fromBrush.Color;
        }

        private void StartPeriodicUpdateService(IApplicationSettings applicationSettings)
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

        public void Rebuild(IApplicationSettings applicationSettings)
        {
            ApplyToSession(applicationSettings);
        }

    }
}
