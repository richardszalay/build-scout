using System;
using System.IO;
using WP7Contrib.Logging;
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

            ApplyBuildResultColors(applicationSettings);
        }

        private void ApplyBuildResultColors(IApplicationSettings applicationSettings)
        {
            CopyColorFrom(applicationSettings.SuccessColorResource, "BuildResultSuccessBrush");
            CopyColorFrom(applicationSettings.FailedColorResource, "BuildResultFailedBrush");
            CopyColorFrom(applicationSettings.UnavailableColorResource, "BuildResultUnavailableBrush");
        }

        private void UpdateTileImages(IApplicationSettings applicationSettings)
        {
            var successColor = applicationResourceFacade.GetResource<SolidColorBrush>("BuildResultSuccessBrush").Color;
            var failedColor = applicationResourceFacade.GetResource<SolidColorBrush>("BuildResultFailedBrush").Color;
            var unavailableColor = applicationResourceFacade.GetResource<SolidColorBrush>("BuildResultUnavailableBrush").Color;

            applicationSettings.SuccessTileUri = UpdateTileImage(successColor, @"Shared\ShellContent\SuccessTile.png");
            applicationSettings.FailureTileUri = UpdateTileImage(failedColor, @"Shared\ShellContent\FailedTile.png");
            applicationSettings.UnavailableTileUri = UpdateTileImage(unavailableColor, @"Shared\ShellContent\UnavailableTile.png");
            applicationSettings.Save();
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
                periodicJobUpdateService.RegisterBackgroundTask();
            }
        }

        public void RebuildSharedResources(IApplicationSettings applicationSettings)
        {
            UpdateTileImages(applicationSettings);
        }

    }
}
