using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Browser;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Funq;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.ViewModels;
using WP7Contrib.Logging;
using Microsoft.Phone.Controls;

namespace RichardSzalay.PocketCiTray.Services
{
    public static class ApplicationDependencyConfiguration
    {
        public static Container Configure()
        {
            var container = CommonDependencyConfiguration.Configure();

            ConfigureFacades(container);

            ConfigureServices(container);

            //var child = container.CreateChildContainer();
            //child.DefaultReuse = ReuseScope.None;

            var child = container;

            ConfigureViewModels(child);

            return child;
        }

        private static void ConfigureServices(Container container)
        {
            container.Register<ISchedulerAccessor>(new SchedulerAccessor(DispatcherScheduler.Instance, Scheduler.ThreadPool));

            var log = new LoggingService(Logging.ApplicationLogName);

            container.Register<ILog>(log);
            container.Register<ILogManager>(log);

            container.Register<IPeriodicJobUpdateService>(l => new PeriodicJobUpdateService(
                l.Resolve<IJobUpdateService>(),
                l.Resolve<ISchedulerAccessor>().Background,
                l.Resolve<IScheduledActionServiceFacade>()
                ));

            container.Register<INavigationService>(l => 
                new PhoneApplicationFrameNavigationService(l.Resolve<PhoneApplicationFrame>()));

            container.Register<Bootstrap>(l => new Bootstrap(
                l.Resolve<IPeriodicJobUpdateService>(),
                l.Resolve<IJobUpdateService>(),
                l.Resolve<IApplicationSettings>(),
                l.Resolve<IClock>(),
                l.Resolve<IMessageBoxFacade>(),
                l.Resolve<IMutexService>(),
                l.Resolve<ILogManager>()));

            container.Register<IHelpService>(c => new HelpService(
                c.Resolve<IIsolatedStorageFacade>(),
                c.Resolve<IApplicationResourceFacade>()
                ));
        }

        private static void ConfigureViewModels(Container container)
        {
            container.Register(c => new ListJobsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IJobUpdateService>()
                ));

            container.Register(c => new ViewJobViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IJobUpdateService>(),
                c.Resolve<IApplicationTileService>(),
                c.Resolve<IWebBrowserTaskFacade>()
                ));

            container.Register(c => new SelectBuildServerViewModel(
                c.Resolve<IJobRepository>(),
                c.Resolve<IJobProviderFactory>(),
                c.Resolve<INavigationService>(),
                c.Resolve<ISchedulerAccessor>()
                ));

            container.Register(c => new AddBuildServerViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobProviderFactory>(), 
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>(),
                c.Resolve<IMessageBoxFacade>(),
                c.Resolve<INetworkInterfaceFacade>()
                ));

            container.Register(c => new AddJobsViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IJobProviderFactory>(),
                c.Resolve<IJobRepository>(),
                c.Resolve<ISchedulerAccessor>()
                ));

            container.Register(c => new ViewHelpViewModel(
                c.Resolve<INavigationService>(),
                c.Resolve<IHelpService>()
                ));
        }

        private static void ConfigureFacades(Container container)
        {
            container.Register<IScheduledActionServiceFacade>(new ScheduledActionServiceFacade());
            container.Register<IMessageBoxFacade>(new MessageBoxFacade());
            container.Register<IWebBrowserTaskFacade>(new WebBrowserTaskFacade());
            container.Register<IIsolatedStorageFacade>(new IsolatedStorageFacade(
                IsolatedStorageFile.GetUserStoreForApplication()
                ));
            container.Register<IApplicationResourceFacade>(new ApplicationResourceFacade());
        }
    }
}
