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
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Data;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using RichardSzalay.PocketCiTray.Tests.Mocks;
using System.ComponentModel;

namespace RichardSzalay.PocketCiTray.Tests.CommonTests.Services
{
    public class ApplicationSettingsSpec
    {
        [TestClass]
        public class when_changing_a_property_value
        {
            private ICollection<BuildServer> buildServers;

            private PropertyChangedEventArgs eventArgs;

            [ClassInitialize]
            public void because_of()
            {

                ApplicationSettings settings = new ApplicationSettings(new StubSettingsService());

                settings.PropertyChanged += (s, e) => eventArgs = e;

                settings.ApplicationUpdateInterval = TimeSpan.FromSeconds(15);
            }

            [TestMethod]
            public void it_should_raise_a_property_changed_event()
            {
                Assert.IsNotNull(eventArgs);
            }
        }
    }
}
