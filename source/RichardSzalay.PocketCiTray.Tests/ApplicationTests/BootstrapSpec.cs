using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Funq;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Tests.Mocks;
using RichardSzalay.PocketCiTray.Tests.Infrastructure;
using WP7Contrib.Logging;

namespace RichardSzalay.PocketCiTray.Tests.ApplicationTests
{
    public class BootstrapSpec
    {
        [TestClass]
        public class when_starting
        {
            private BlacklistMutexService mutexService;
            [ClassInitialize]
            public void because_of()
            {
                this.mutexService = new BlacklistMutexService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IMutexService>(mutexService);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
            }

            [TestMethod]
            public void it_should_obtain_a_global_mutex()
            {
                mutexService.TakenMutexes.Contains(MutexNames.ForegroundApplication);
            }
        }

        [TestClass]
        public class when_starting_for_the_first_time
        {
            private StubApplicationSettings applicationSettings;
            private StubMessageBoxFacade messageBoxFacade;
            [ClassInitialize]
            public void because_of()
            {
                this.messageBoxFacade = new StubMessageBoxFacade(MessageBoxResult.Cancel);

                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.FirstRun = true;

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IMessageBoxFacade>(messageBoxFacade);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
            }

            [TestMethod]
            public void it_should_ask_the_user_to_register_background_updates()
            {
                Assert.AreEqual(1, messageBoxFacade.RecordedMessageBoxes.Count);
            }

            [TestMethod]
            public void it_should_mark_firstrun_as_false()
            {
                Assert.IsFalse(applicationSettings.FirstRun);
            }

            [TestMethod]
            public void it_should_save_application_settings()
            {
                Assert.IsTrue(applicationSettings.TimesSaved > 0);
            }
        }

        [TestClass]
        public class when_accepting_background_updates_on_first_run
        {
            private StubApplicationSettings applicationSettings;
            private StubMessageBoxFacade messageBoxFacade;

            [ClassInitialize]
            public void because_of()
            {
                this.messageBoxFacade = new StubMessageBoxFacade(MessageBoxResult.OK);

                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.FirstRun = true;

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IMessageBoxFacade>(messageBoxFacade);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
            }

            [TestMethod]
            public void it_should_enable_background_updates()
            {
                Assert.IsTrue(applicationSettings.BackgroundUpdateEnabled);
            }
        }

        [TestClass]
        public class when_opting_out_of_background_updates_on_first_run
        {
            private StubApplicationSettings applicationSettings;
            private StubMessageBoxFacade messageBoxFacade;

            [ClassInitialize]
            public void because_of()
            {
                this.messageBoxFacade = new StubMessageBoxFacade(MessageBoxResult.Cancel);

                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.FirstRun = true;

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IMessageBoxFacade>(messageBoxFacade);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
            }

            [TestMethod]
            public void it_should_not_enable_background_updates()
            {
                Assert.IsFalse(applicationSettings.BackgroundUpdateEnabled);
            }
        }

        [TestClass]
        public class when_starting_with_foreground_updates_enabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.ApplicationUpdateInterval = TimeSpan.FromMinutes(5);

                this.periodicUpdateService = new StubPeriodicJobUpdateService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IPeriodicJobUpdateService>(periodicUpdateService);

                container.Resolve<Bootstrap>().Startup();
            }

            [TestMethod]
            public void it_should_start_the_job_update_service()
            {
                Assert.IsTrue(periodicUpdateService.IsRunning);
            }
        }

        [TestClass]
        public class when_starting_with_foreground_updates_disabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.ApplicationUpdateInterval = TimeSpan.Zero;

                this.periodicUpdateService = new StubPeriodicJobUpdateService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IPeriodicJobUpdateService>(periodicUpdateService);

                container.Resolve<Bootstrap>().Startup();
            }

            [TestMethod]
            public void it_should_start_the_job_update_service()
            {
                Assert.IsFalse(periodicUpdateService.IsRunning);
            }
        }

        [TestClass]
        public class when_starting_with_background_updates_enabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.BackgroundUpdateEnabled = true;

                this.periodicUpdateService = new StubPeriodicJobUpdateService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IPeriodicJobUpdateService>(periodicUpdateService);

                container.Resolve<Bootstrap>().Startup();
            }

            [TestMethod]
            public void it_should_register_the_background_update_service()
            {
                Assert.IsTrue(periodicUpdateService.IsBackgroundServiceRegistered);
            }
        }

        [TestClass]
        public class when_starting_with_background_updates_disabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.BackgroundUpdateEnabled = false;

                this.periodicUpdateService = new StubPeriodicJobUpdateService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IPeriodicJobUpdateService>(periodicUpdateService);

                container.Resolve<Bootstrap>().Startup();
            }

            [TestMethod]
            public void it_should_not_register_the_background_update_service()
            {
                Assert.IsFalse(periodicUpdateService.IsBackgroundServiceRegistered);
            }
        }

        [TestClass]
        public class when_starting_with_logging_enabled
        {
            private StubApplicationSettings applicationSettings;
            private StubLoggingService loggingService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.LoggingEnabled = true;

                this.loggingService = new StubLoggingService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<ILogManager>(loggingService);

                container.Resolve<Bootstrap>().Startup();
            }

            [TestMethod]
            public void it_should_enable_the_logging_service()
            {
                Assert.IsTrue(loggingService.IsEnabled);
            }
        }

        [TestClass]
        public class when_starting_with_logging_disabled
        {
            private StubApplicationSettings applicationSettings;
            private StubLoggingService loggingService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.LoggingEnabled = false;

                this.loggingService = new StubLoggingService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<ILogManager>(loggingService);

                container.Resolve<Bootstrap>().Startup();
            }

            [TestMethod]
            public void it_should_not_enable_the_logging_service()
            {
                Assert.IsFalse(loggingService.IsEnabled);
            }
        }

        [TestClass]
        public class when_continuing
        {
            private BlacklistMutexService mutexService;
            [ClassInitialize]
            public void because_of()
            {
                this.mutexService = new BlacklistMutexService();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IMutexService>(mutexService);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Continue();
            }

            [TestMethod]
            public void it_should_obtain_a_global_mutex()
            {
                mutexService.TakenMutexes.Contains(MutexNames.ForegroundApplication);
            }
        }

        [TestClass]
        public class when_shutting_down
        {
            private BlacklistMutexService mutexService;
            private StubLoggingService loggingService;

            [ClassInitialize]
            public void because_of()
            {
                this.mutexService = new BlacklistMutexService();
                this.loggingService = new StubLoggingService();
                this.loggingService.Enable();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IMutexService>(mutexService);
                container.Register<ILogManager>(loggingService);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
                bootstrap.Shutdown();
            }

            [TestMethod]
            public void it_should_disable_the_logging_service()
            {
                Assert.IsFalse(loggingService.IsEnabled);

            }
            [TestMethod]
            public void it_should_release_the_application_mutex()
            {
                mutexService.ReleasedMutexes.Contains(MutexNames.ForegroundApplication);
            }
        }
    }
}
