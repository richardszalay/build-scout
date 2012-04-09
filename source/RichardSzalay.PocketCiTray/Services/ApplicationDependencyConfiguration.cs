using System.Reactive.Concurrency;
using Funq;
using Google.WebAnalytics;
using Microsoft.WebAnalytics;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.ViewModels;
using WP7Contrib.Logging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RichardSzalay.PocketCiTray.Controllers;

namespace RichardSzalay.PocketCiTray.Services
{
    public static class ApplicationDependencyConfiguration
    {
        public static Container Configure()
        {
            var container = CommonDependencyConfiguration.Configure();

            ConfigureFacades(container);

            ConfigureServices(container);

            var child = container;

            ConfigureViewModels(child);

            return child;
        }

        private static void ConfigureServices(Container container)
        {
            container.Register<ISchedulerAccessor>(new SchedulerAccessor(DispatcherScheduler.Instance, Scheduler.ThreadPool));

            var log = new ThreadSafeLoggingService(Logging.ApplicationLogName);

            container.Register<ILog>(log);
            container.Register<ILogManager>(log);

#if DEBUG_256_DEVICE
            container.Register<IDeviceInformationService>(c => new LowEndDeviceInformationService());
#endif

            container.Register<IPeriodicJobUpdateService>(l => new PeriodicJobUpdateService(
                l.Resolve<IJobUpdateService>(),
                l.Resolve<ISchedulerAccessor>().Background,
                l.Resolve<IScheduledActionServiceFacade>(),
                l.Resolve<IDeviceInformationService>()
                ));

            container.Register<INavigationService>(l => 
                new PhoneApplicationFrameNavigationService(l.Resolve<PhoneApplicationFrame>()));

            container.Register<ITileImageGenerator>(c => new TileImageGenerator(
                c.Resolve<IApplicationResourceFacade>()));

            container.Register<ISettingsApplier>(c => new SettingsApplier(
                c.Resolve<ILogManager>(),
                c.Resolve<IJobUpdateService>(),
                c.Resolve<IPeriodicJobUpdateService>(),
                c.Resolve<IClock>(),
                c.Resolve<IApplicationResourceFacade>(),
                c.Resolve<IPhoneApplicationServiceFacade>(),
                c.Resolve<ITileImageGenerator>(),
                c.Resolve<IIsolatedStorageFacade>(),
                c.Resolve<ILog>()));

            container.Register<Bootstrap>(l => new Bootstrap(l.Resolve<IApplicationSettings>(),
                l.Resolve<IClock>(),
                l.Resolve<IMessageBoxFacade>(),
                l.Resolve<IMutexService>(),
                l.Resolve<ILogManager>(),
                l.Resolve<ISettingsApplier>(),
                l.Resolve<IJobRepository>(),
                l.Resolve<IPeriodicJobUpdateService>()));

            container.Register<IThemeCssGenerator>(c => new PhoneThemeCssGenerator(
                c.Resolve<IApplicationResourceFacade>()
                ));

            container.Register<IHelpService>(c => new HelpService(
                c.Resolve<IIsolatedStorageFacade>(),
                c.Resolve<IApplicationResourceFacade>(),
                c.Resolve<IThemeCssGenerator>()
                ));

            container.Register<IJobController>(c => new JobController(
                c.Resolve<IJobRepository>(),
                c.Resolve<IApplicationTileService>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IMessageBoxFacade>()
                ));
        }

        private static void ConfigureViewModels(Container container)
        {
            container.Register(c => new ListJobsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IJobUpdateService>(), 
                c.Resolve<IApplicationTileService>(), 
                c.Resolve<IMessageBoxFacade>(),
                c.Resolve<IApplicationSettings>(),
                c.Resolve<IApplicationResourceFacade>(),
                c.Resolve<IJobController>())
                ).ReusedWithin(ReuseScope.None);

            container.Register(c => new ViewJobViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IApplicationTileService>(),
                c.Resolve<IWebBrowserTaskFacade>(), c.Resolve<IMessageBoxFacade>(),
                c.Resolve<IJobController>())
                ).ReusedWithin(ReuseScope.None);

            container.Register(c => new SelectBuildServerViewModel(
                c.Resolve<IJobRepository>(),
                c.Resolve<IJobProviderFactory>(),
                c.Resolve<INavigationService>(),
                c.Resolve<ISchedulerAccessor>(), c.Resolve<IMessageBoxFacade>())
                ).ReusedWithin(ReuseScope.None);

            container.Register(c => new AddBuildServerViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobProviderFactory>(), 
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IMessageBoxFacade>(),
                c.Resolve<INetworkInterfaceFacade>()
                )).ReusedWithin(ReuseScope.None);

            container.Register(c => new AddJobsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobProviderFactory>(),
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>()
                )).ReusedWithin(ReuseScope.None);

            container.Register(c => new ViewHelpViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IHelpService>()
                )).ReusedWithin(ReuseScope.None);

            container.Register(c => new EditSettingsViewModel(
                c.Resolve<IApplicationSettings>(),
                c.Resolve<IDeviceInformationService>())
                ).ReusedWithin(ReuseScope.None);

            container.Register(c => new EditNotificationSettingsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IApplicationSettings>(),
                c.Resolve<IApplicationResourceFacade>(), c.Resolve<ISettingsApplier>()
                )).ReusedWithin(ReuseScope.None);

            container.Register(c => new EditColourSettingsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IApplicationSettings>(),
                c.Resolve<IApplicationResourceFacade>(), c.Resolve<ISettingsApplier>()
                )).ReusedWithin(ReuseScope.None);

            container.Register(c => new EditScheduleSettingsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IApplicationSettings>(),
                c.Resolve<IApplicationResourceFacade>(), 
                c.Resolve<ISettingsApplier>(),
                c.Resolve<IDeviceInformationService>()
                )).ReusedWithin(ReuseScope.None);

            container.Register(c => new AboutViewModel(
                c.Resolve<IEmailComposeTaskFacade>()
                )).ReusedWithin(ReuseScope.None);

            container.Register<IGoogleAnalyticsFactory>(c => new GoogleAnalyticsFactory(
                TrackingCodes.Foreground,
                c.Resolve<IDeviceInformationService>(),
                c.Resolve<IApplicationInformation>()
                )).ReusedWithin(ReuseScope.None);
        }

        private static void ConfigureFacades(Container container)
        {
            container.Register<IScheduledActionServiceFacade>(new ScheduledActionServiceFacade());
            container.Register<IMessageBoxFacade>(new MessageBoxFacade());
            container.Register<IWebBrowserTaskFacade>(new WebBrowserTaskFacade());

            container.Register<IEmailComposeTaskFacade>(new EmailComposeTaskFacade());
            
            container.Register<IApplicationResourceFacade>(new ApplicationResourceFacade());

            container.Register<IPhoneApplicationServiceFacade>(c => new PhoneApplicationServiceFacade(
                PhoneApplicationService.Current));
        }
    }
}
