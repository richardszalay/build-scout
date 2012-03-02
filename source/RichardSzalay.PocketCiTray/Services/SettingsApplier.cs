using System;
using System.IO;
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
        private readonly ITileImageGenerator tileImageGenerator;
        private readonly IIsolatedStorageFacade isolatedStorageFacade;

        public SettingsApplier(ILogManager logManager, IJobUpdateService jobUpdateService,
            IPeriodicJobUpdateService periodicJobUpdateService, IClock clock,
            IApplicationResourceFacade applicationResourceFacade, IPhoneApplicationServiceFacade phoneApplicationService,
            ITileImageGenerator tileImageGenerator, IIsolatedStorageFacade isolatedStorageFacade)
        {
            this.logManager = logManager;
            this.jobUpdateService = jobUpdateService;
            this.periodicJobUpdateService = periodicJobUpdateService;
            this.clock = clock;
            this.applicationResourceFacade = applicationResourceFacade;
            this.phoneApplicationService = phoneApplicationService;
            this.tileImageGenerator = tileImageGenerator;
            this.isolatedStorageFacade = isolatedStorageFacade;
        }

        public void ApplyToSession(IApplicationSettings applicationSettings)
        {
            StartPeriodicUpdateService(applicationSettings);

            if (applicationSettings.LoggingEnabled)
            {
                logManager.Enable();
            }

            phoneApplicationService.ApplicationIdleDetectionMode = (applicationSettings.RunUnderLockScreen)
                ? IdleDetectionMode.Disabled
                : IdleDetectionMode.Enabled;

            UpdateTiles(applicationSettings);
        }

        private void UpdateTiles(IApplicationSettings applicationSettings)
        {
            var successColor = CopyColorFrom(applicationSettings.SuccessColorResource, "BuildResultSuccessBrush");
            var failedColor = CopyColorFrom(applicationSettings.FailedColorResource, "BuildResultFailedBrush");
            var unavailableColor = CopyColorFrom(applicationSettings.UnavailableColorResource, "BuildResultUnavailableBrush");

            applicationSettings.SuccessTileUri = UpdateTileImage(successColor, @"Shared\ShellContent\SuccessTile.png");
            applicationSettings.FailureTileUri = UpdateTileImage(failedColor, @"Shared\ShellContent\FailedTile.png");
            applicationSettings.UnavailableTileUri = UpdateTileImage(unavailableColor, @"Shared\ShellContent\UnavailableTile.png");
            applicationSettings.Save();
        }

        private Uri UpdateTileImage(Color color, string path)
        {
            string directory = Path.GetDirectoryName(path);

            if (!isolatedStorageFacade.DirectoryExists(directory))
            {
                isolatedStorageFacade.CreateDirectory(directory);
            }

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
