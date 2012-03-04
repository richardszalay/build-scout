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
            private MockSettingsApplier settingsApplier;

            [ClassInitialize]
            public void because_of()
            {
                this.mutexService = new BlacklistMutexService();
                this.settingsApplier = new MockSettingsApplier();

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IMutexService>(mutexService);
                container.Register<ISettingsApplier>(settingsApplier);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
            }

            [TestMethod]
            public void it_should_obtain_a_global_mutex()
            {
                mutexService.TakenMutexes.Contains(MutexNames.ForegroundApplication);
            }

            [TestMethod]
            public void it_should_apply_settings_to_the_session()
            {
                Assert.AreEqual(1, settingsApplier.TimedAppliedToSession);
            }
        }

        [TestClass]
        public class when_starting_for_the_first_time
        {
            private StubApplicationSettings applicationSettings;
            private StubMessageBoxFacade messageBoxFacade;
            private MockSettingsApplier settingsApplier;

            [ClassInitialize]
            public void because_of()
            {
                this.messageBoxFacade = new StubMessageBoxFacade(MessageBoxResult.Cancel);

                settingsApplier = new MockSettingsApplier();

                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.FirstRun = true;

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IMessageBoxFacade>(messageBoxFacade);
                container.Register<ISettingsApplier>(settingsApplier);

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

            [TestMethod]
            public void it_should_apply_application_settings()
            {
                Assert.IsTrue(applicationSettings.TimesSaved > 0);
            }
        }

        [TestClass]
        public class when_accepting_background_updates_on_first_run
        {
            private TimeSpan startingBackgroundInterval = TimeSpan.FromSeconds(5);
            private StubApplicationSettings applicationSettings;
            private StubMessageBoxFacade messageBoxFacade;
            

            [ClassInitialize]
            public void because_of()
            {
                this.messageBoxFacade = new StubMessageBoxFacade(MessageBoxResult.OK);

                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.FirstRun = true;
                this.applicationSettings.BackgroundUpdateInterval = startingBackgroundInterval;

                Container container = TestDependencyConfiguration.Configure();
                container.Register<IApplicationSettings>(applicationSettings);
                container.Register<IMessageBoxFacade>(messageBoxFacade);

                var bootstrap = container.Resolve<Bootstrap>();

                bootstrap.Startup();
            }

            [TestMethod]
            public void it_should_not_disable_background_updates()
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
            public void it_should_disable_background_updates()
            {
                Assert.IsFalse(applicationSettings.BackgroundUpdateEnabled);
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
