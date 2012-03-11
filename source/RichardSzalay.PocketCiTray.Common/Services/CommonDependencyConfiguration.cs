using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Browser;
using System.Reactive.Concurrency;
using Funq;
using RichardSzalay.PocketCiTray.Providers;
using WP7Contrib.Logging;
using RichardSzalay.PocketCiTray.Data;

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
                l.Resolve<ISettingsService>()));

            container.Register<IJobProviderFactory>(l => new JobProviderFactory(
                l.Resolve<IWebRequestCreate>(),
                l.Resolve<IClock>(),
                l.Resolve<ISchedulerAccessor>(),
                l.Resolve<ILog>()));

            //container.Register<IJobRepository>(l => new InMemoryJobRepository(l.Resolve<IClock>()));

            container.Register<ICredentialEncryptor>(c => new CredentialEncryptor());

            container.Register<IJobDataContextFactory>(c => new JobDataContextFactory(
                c.Resolve<IMutexService>()));
            
            container.Register<IJobRepository>(l => new DbJobRepository(
                l.Resolve<IJobDataContextFactory>(),
                l.Resolve<ICredentialEncryptor>(),
                l.Resolve<IClock>()));

            container.Register<IJobUpdateService>(l => new JobUpdateService(
                l.Resolve<IJobProviderFactory>(),
                l.Resolve<IJobRepository>(),
                l.Resolve<IClock>(),
                l.Resolve<ISettingsService>(),
                l.Resolve<IMutexService>(),
                l.Resolve<ISchedulerAccessor>(),
                l.Resolve<IApplicationTileService>(),
                l.Resolve<IJobNotificationService>(),
                l.Resolve<ILog>()));

            container.Register<IApplicationTileService>(l => new ApplicationTileService(
                l.Resolve<IApplicationSettings>(),
                l.Resolve<IShellTileService>()));

            container.Register<IJobNotificationService>(l => new JobNotificationService(
                l.Resolve<IApplicationSettings>(),
                l.Resolve<IShellToastFacade>(),
                l.Resolve<IClock>()));

            container.Register<ISettingsDictionarySerializer>(c => new SettingsDictionarySerializer());

            container.Register<ISettingsService>(c => new IsolatedStorageFileSettingsService(
                c.Resolve<ISettingsDictionarySerializer>(),
                c.Resolve<IIsolatedStorageFacade>(),
                c.Resolve<IMutexService>(),
                c.Resolve<ILog>()
                ));
        }

        private static void ConfigureFacades(Container container)
        {
            container.Register<IClock>(new DateTimeOffsetClock());

            container.Register<IIsolatedStorageFacade>(new IsolatedStorageFacade(
                IsolatedStorageFile.GetUserStoreForApplication()
                ));

            container.Register<IShellTileService>(new ShellTileService());
            container.Register<IShellToastFacade>(new ShellToastFacade());

            container.Register<INetworkInterfaceFacade>(new NetworkInterfaceFacade());
            
        }
    }
}
