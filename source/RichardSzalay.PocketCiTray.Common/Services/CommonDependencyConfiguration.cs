using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Browser;
using System.Reactive.Concurrency;
using Funq;
using RichardSzalay.PocketCiTray.Providers;
using WP7Contrib.Logging;

namespace RichardSzalay.PocketCiTray.Services
{
    public static class CommonDependencyConfiguration
    {
        public static Container Configure()
        {
            var container = new Container();

            ConfigureFacades(container);

            ConfigureServices(container);

            return container;
        }

        private static void ConfigureServices(Container container)
        {
            container.Register<IWebRequestCreate>(SharpGIS.WebRequestCreator.GZip);

            container.Register<ISchedulerAccessor>(new SchedulerAccessor(null, Scheduler.ThreadPool));

            container.Register<IMutexService>(new MutexService());

            container.Register<IApplicationSettings>(l => new ApplicationSettings(
                l.Resolve<ISettingsFacade>()));

            container.Register<IJobProviderFactory>(l => new JobProviderFactory(
                l.Resolve<IWebRequestCreate>(),
                l.Resolve<IClock>(),
                l.Resolve<ISchedulerAccessor>(),
                l.Resolve<ILog>()));

            container.Register<IJobRepository>(l => new JobRepository(
                l.Resolve<ISchedulerAccessor>()));

            container.Register<IJobUpdateService>(l => new JobUpdateService(
                l.Resolve<IJobProviderFactory>(),
                l.Resolve<IJobRepository>(),
                l.Resolve<IClock>(),
                l.Resolve<ISettingsFacade>(),
                l.Resolve<IMutexService>(),
                l.Resolve<ISchedulerAccessor>(),
                l.Resolve<IApplicationTileService>()));

            container.Register<IApplicationTileService>(l => new ApplicationTileService(
                l.Resolve<IApplicationSettings>(),
                l.Resolve<IShellTileService>()));
        }

        private static void ConfigureFacades(Container container)
        {
            container.Register<IClock>(new DateTimeOffsetClock());
            container.Register<ISettingsFacade>(new IsolatedStorageSettingsFacade(IsolatedStorageSettings.ApplicationSettings));

            container.Register<IShellTileService>(new ShellTileService());

            container.Register<INetworkInterfaceFacade>(new NetworkInterfaceFacade());
            
        }
    }
}
