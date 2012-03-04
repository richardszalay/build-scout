using System;
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
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Tests.Mocks;
using Funq;
using RichardSzalay.PocketCiTray.Tests.Infrastructure;
using WP7Contrib.Logging;
using System.IO;

namespace RichardSzalay.PocketCiTray.Tests.ApplicationTests.Services
{
    public class SettingsApplierSpec
    {
        [TestClass]
        public class when_applying_to_session
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            private SolidColorBrush successSetting = new SolidColorBrush(Colors.Red);
            private SolidColorBrush failedSetting = new SolidColorBrush(Colors.Green);
            private SolidColorBrush unavailableSetting = new SolidColorBrush(Colors.Blue);

            private SolidColorBrush successResource = new SolidColorBrush(Colors.Transparent);
            private SolidColorBrush failedResource = new SolidColorBrush(Colors.Transparent);
            private SolidColorBrush unavailableResource = new SolidColorBrush(Colors.Transparent);

            [ClassInitialize]
            public void because_of()
            {
                var applicationResourceFacade = new FakeApplicationResourceFacade();
                applicationResourceFacade.SetResource("SuccessResource", successSetting);
                applicationResourceFacade.SetResource("FailedResource", failedSetting);
                applicationResourceFacade.SetResource("UnavailableResource", unavailableSetting);
                applicationResourceFacade.SetResource("BuildResultSuccessBrush", successResource);
                applicationResourceFacade.SetResource("BuildResultFailedBrush", failedResource);
                applicationResourceFacade.SetResource("BuildResultUnavailableBrush", unavailableResource);

                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.ApplicationUpdateInterval = TimeSpan.Zero;
                this.applicationSettings.SuccessColorResource = "SuccessResource";
                this.applicationSettings.FailedColorResource = "FailedResource";
                this.applicationSettings.UnavailableColorResource = "UnavailableResource";

                periodicUpdateService = new StubPeriodicJobUpdateService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.ApplicationResourceFacade = applicationResourceFacade;
                builder.PeriodicJobUpdateService = periodicUpdateService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_update_the_success_brush_resource()
            {
                Assert.AreEqual(successSetting.Color, successResource.Color);
            }

            [TestMethod]
            public void it_should_update_the_failed_brush_resource()
            {
                Assert.AreEqual(failedSetting.Color, failedResource.Color);
            }

            [TestMethod]
            public void it_should_update_the_unavailable_brush_resource()
            {
                Assert.AreEqual(unavailableSetting.Color, unavailableResource.Color);
            }
        }

        [TestClass]
        public class when_applying_to_session_with_foreground_updates_enabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.ApplicationUpdateInterval = TimeSpan.FromMinutes(5);

                periodicUpdateService = new StubPeriodicJobUpdateService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.PeriodicJobUpdateService = periodicUpdateService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_start_the_job_update_service()
            {
                Assert.IsTrue(periodicUpdateService.IsRunning);
            }
        }

        [TestClass]
        public class when_applying_to_session_with_foreground_updates_disabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.ApplicationUpdateInterval = TimeSpan.Zero;

                periodicUpdateService = new StubPeriodicJobUpdateService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.PeriodicJobUpdateService = periodicUpdateService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_not_start_the_job_update_service()
            {
                Assert.IsFalse(periodicUpdateService.IsRunning);
            }
        }

        [TestClass]
        public class when_applying_to_session_with_background_updates_enabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.BackgroundUpdateInterval = TimeSpan.FromSeconds(5);

                periodicUpdateService = new StubPeriodicJobUpdateService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.PeriodicJobUpdateService = periodicUpdateService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_register_the_background_update_service()
            {
                Assert.IsTrue(periodicUpdateService.IsBackgroundServiceRegistered);
            }
        }

        [TestClass]
        public class when_applying_to_session_with_background_updates_disabled
        {
            private StubApplicationSettings applicationSettings;
            private StubPeriodicJobUpdateService periodicUpdateService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.BackgroundUpdateInterval = TimeSpan.Zero;

                periodicUpdateService = new StubPeriodicJobUpdateService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.PeriodicJobUpdateService = periodicUpdateService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_not_register_the_background_update_service()
            {
                Assert.IsFalse(periodicUpdateService.IsBackgroundServiceRegistered);
            }
        }

        [TestClass]
        public class when_applying_to_session_with_logging_enabled
        {
            private StubApplicationSettings applicationSettings;
            private StubLoggingService loggingService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.LoggingEnabled = true;

                loggingService = new StubLoggingService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.LoggingService = loggingService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_enable_the_logging_service()
            {
                Assert.IsTrue(loggingService.IsEnabled);
            }
        }

        [TestClass]
        public class when_applying_to_session_with_logging_disabled
        {
            private StubApplicationSettings applicationSettings;
            private StubLoggingService loggingService;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.LoggingEnabled = false;

                loggingService = new StubLoggingService();

                var builder = new SettingsApplierInstanceBuilder();

                builder.LoggingService = loggingService;

                builder.Create().ApplyToSession(applicationSettings);
            }

            [TestMethod]
            public void it_should_not_enable_the_logging_service()
            {
                Assert.IsFalse(loggingService.IsEnabled);
            }
        }

        [TestClass]
        public class when_rebuilding_shared_resources
        {
            private FakeApplicationResourceFacade applicationResourceFacade;
            private StubApplicationSettings applicationSettings;
            private MockIsolatedStorageFacade isolatedStorageFacade;
            private MockTileImageGenerator tileImageGenerator;

            [ClassInitialize]
            public void because_of()
            {
                this.applicationSettings = new StubApplicationSettings();
                this.applicationSettings.LoggingEnabled = false;

                applicationResourceFacade = new FakeApplicationResourceFacade();
                applicationResourceFacade.SetResource("BuildResultSuccessBrush", new SolidColorBrush(Colors.Red));
                applicationResourceFacade.SetResource("BuildResultFailedBrush", new SolidColorBrush(Colors.Green));
                applicationResourceFacade.SetResource("BuildResultUnavailableBrush", new SolidColorBrush(Colors.Blue));

                isolatedStorageFacade = new MockIsolatedStorageFacade();
                tileImageGenerator = new MockTileImageGenerator();

                var builder = new SettingsApplierInstanceBuilder();
                builder.ApplicationResourceFacade = applicationResourceFacade;
                builder.TileImageGenerator = tileImageGenerator;
                builder.IsolatedStorageFacade = isolatedStorageFacade;

                builder.Create().RebuildSharedResources(applicationSettings);
            }

            [TestMethod]
            public void it_should_generate_a_tile_image_for_success()
            {
                VerifyTileCreated(@"Shared\ShellContent\SuccessTile.png", Colors.Red);
            }

            [TestMethod]
            public void it_should_generate_a_tile_image_for_failed()
            {
                VerifyTileCreated(@"Shared\ShellContent\FailedTile.png", Colors.Green);
            }

            [TestMethod]
            public void it_should_generate_a_tile_image_for_unavailable()
            {
                VerifyTileCreated(@"Shared\ShellContent\UnavailableTile.png", Colors.Blue);
            }

            [TestMethod]
            public void it_should_update_the_success_tile_uri()
            {
                Assert.IsNotNull(applicationSettings.SuccessTileUri);
            }

            [TestMethod]
            public void it_should_update_the_failed_tile_uri()
            {
                Assert.IsNotNull(applicationSettings.FailureTileUri);
            }

            [TestMethod]
            public void it_should_update_the_unavailable_tile_uri()
            {
                Assert.IsNotNull(applicationSettings.UnavailableTileUri);
            }

            [TestMethod]
            public void it_should_save_application_settings()
            {
                Assert.AreEqual(1, applicationSettings.TimesSaved);
            }

            private void VerifyTileCreated(string path, Color color)
            {
                MemoryStream stream;

                if (!isolatedStorageFacade.CreatedFiles.TryGetValue(path, out stream))
                {
                    Assert.Fail("Expected file was not created: " + path);
                }

                Assert.AreEqual(color, tileImageGenerator.CreatedTiles[stream]);
            }
        }

        private class SettingsApplierInstanceBuilder
        {
            public StubLoggingService LoggingService = new StubLoggingService();
            public MockJobUpdateService JobUpdateService = new MockJobUpdateService();
            public StubPeriodicJobUpdateService PeriodicJobUpdateService = new StubPeriodicJobUpdateService();
            public StubClock Clock = new StubClock();
            public FakeApplicationResourceFacade ApplicationResourceFacade = new FakeApplicationResourceFacade();
            public StubPhoneApplicationServiceFacade PhoneApplicationServiceFacade = new StubPhoneApplicationServiceFacade();
            public MockTileImageGenerator TileImageGenerator = new MockTileImageGenerator();
            public MockIsolatedStorageFacade IsolatedStorageFacade = new MockIsolatedStorageFacade();

            public SettingsApplier Create()
            {
                return new SettingsApplier(
                    LoggingService,
                    JobUpdateService,
                    PeriodicJobUpdateService,
                    Clock,
                    ApplicationResourceFacade,
                    PhoneApplicationServiceFacade,
                    TileImageGenerator,
                    IsolatedStorageFacade
                    );
            }
        }
    }
}
