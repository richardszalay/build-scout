﻿using System;
using System.IO;
using Microsoft.Phone.Scheduler;

using System.Windows.Media;
using Microsoft.Phone.Shell;
using System.Reactive;
using System.Reactive.Linq;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ISettingsApplier
    {
        void ApplyToSession(IApplicationSettings applicationSettings);
        void RebuildSharedResources(IApplicationSettings applicationSettings);
    }

    public class SettingsApplier : ISettingsApplier
    {
        private readonly ILogManager logManager;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IPeriodicJobUpdateService periodicJobUpdateService;
        private readonly IClock clock;
        private readonly IApplicationResourceFacade applicationResourceFacade;
        private readonly IPhoneApplicationServiceFacade phoneApplicationService;
        private readonly ITileImageGenerator tileImageGenerator;
        private readonly IIsolatedStorageFacade isolatedStorageFacade;
        private readonly ILog log;

        public SettingsApplier(ILogManager logManager, IJobUpdateService jobUpdateService,
            IPeriodicJobUpdateService periodicJobUpdateService, IClock clock,
            IApplicationResourceFacade applicationResourceFacade, IPhoneApplicationServiceFacade phoneApplicationService,
            ITileImageGenerator tileImageGenerator, IIsolatedStorageFacade isolatedStorageFacade,
            ILog log)
        {
            this.logManager = logManager;
            this.jobUpdateService = jobUpdateService;
            this.periodicJobUpdateService = periodicJobUpdateService;
            this.clock = clock;
            this.applicationResourceFacade = applicationResourceFacade;
            this.phoneApplicationService = phoneApplicationService;
            this.tileImageGenerator = tileImageGenerator;
            this.isolatedStorageFacade = isolatedStorageFacade;
            this.log = log;
        }

        public void ApplyToSession(IApplicationSettings applicationSettings)
        {
            StartPeriodicUpdateService(applicationSettings);

            if (applicationSettings.LoggingEnabled)
            {
                logManager.Enable();
            }

            if (applicationSettings.RunUnderLockScreen)
            {
                phoneApplicationService.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            ApplyBuildResultColors(applicationSettings);
        }

        private void ApplyBuildResultColors(IApplicationSettings applicationSettings)
        {
            CopyColorFrom(applicationSettings.SuccessColorResource, "BuildResultSuccessBrush");
            CopyColorFrom(applicationSettings.FailedColorResource, "BuildResultFailedBrush");
            CopyColorFrom(applicationSettings.UnavailableColorResource, "BuildResultUnavailableBrush");
        }

        private const string PhoneAccentBrushResourceKey = "PhoneAccentBrush";

        private void UpdateTileImages(IApplicationSettings applicationSettings)
        {
            var successColor = GetTileColor(applicationSettings.SuccessColorResource);
            var failedColor = GetTileColor(applicationSettings.FailedColorResource);
            var unavailableColor = GetTileColor(applicationSettings.UnavailableColorResource);

            if (applicationSettings.UseColoredTiles)
            {
                applicationSettings.SuccessTileUri = UpdateTileImage(successColor, @"Shared\ShellContent\SuccessTile.jpg");
                applicationSettings.FailureTileUri = UpdateTileImage(failedColor, @"Shared\ShellContent\FailedTile.jpg");
                applicationSettings.UnavailableTileUri = UpdateTileImage(unavailableColor, @"Shared\ShellContent\UnavailableTile.jpg");
            }
            else
            {
                Uri UncoloredTileUri = new Uri("Images/Tiles/Template.png", UriKind.Relative);
                applicationSettings.SuccessTileUri = UncoloredTileUri;
                applicationSettings.FailureTileUri = UncoloredTileUri;
                applicationSettings.UnavailableTileUri = UncoloredTileUri;
            }
            applicationSettings.Save();
        }

        private Color GetTileColor(string brushResourceKey)
        {
            return applicationResourceFacade.GetResource<SolidColorBrush>(brushResourceKey).Color;
        }

        private Uri UpdateTileImage(Color color, string path)
        {
            string directory = Path.GetDirectoryName(path);

            using (var output = isolatedStorageFacade.CreateFile(path))
            {
                tileImageGenerator.Create(color, output);
            }

            return new Uri("isostore:/" + path.Replace("/", @"\"), UriKind.RelativeOrAbsolute);
        }

        private Color CopyColorFrom(string fromResource, string toResource)
        {
            var fromBrush = applicationResourceFacade.GetResource<SolidColorBrush>(fromResource);
            var toBrush = applicationResourceFacade.GetResource<SolidColorBrush>(toResource);

            toBrush.Color = fromBrush.Color;

            return fromBrush.Color;
        }

        private void StartPeriodicUpdateService(IApplicationSettings applicationSettings)
        {
            if (applicationSettings.ForegroundUpdateInterval > TimeSpan.Zero)
            {
                TimeSpan firstUpdate = PeriodicTaskHelper.GetNextRunTime(jobUpdateService.LastUpdateTime,
                    applicationSettings.ForegroundUpdateInterval, clock.UtcNow);

                periodicJobUpdateService.Start(firstUpdate, applicationSettings.ForegroundUpdateInterval);
            }

            if (applicationSettings.BackgroundUpdateEnabled)
            {
                try
                {
                    periodicJobUpdateService.RegisterBackgroundTask();
                }
                catch (InvalidOperationException)
                {
                    log.Write("InvalidOperationException registering background service");
                }
                catch (SchedulerServiceException)
                {
                    log.Write("SchedulerServiceException registering background service");
                }
            }
        }

        public void RebuildSharedResources(IApplicationSettings applicationSettings)
        {
            UpdateTileImages(applicationSettings);
        }

    }
}
