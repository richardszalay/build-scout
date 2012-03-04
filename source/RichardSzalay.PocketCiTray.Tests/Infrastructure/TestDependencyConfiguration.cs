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
using Funq;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Tests.Mocks;

namespace RichardSzalay.PocketCiTray.Tests.Infrastructure
{
    public class TestDependencyConfiguration
    {
        public static Container Configure()
        {
            var container = ApplicationDependencyConfiguration.Configure();

            container.Register<IApplicationSettings>(new StubApplicationSettings());
            container.Register<IMutexService>(new BlacklistMutexService());
            container.Register<INavigationService>(new FakeNavigationService());
            container.Register<ISettingsApplier>(new MockSettingsApplier());

            return container;
        }
    }
}
