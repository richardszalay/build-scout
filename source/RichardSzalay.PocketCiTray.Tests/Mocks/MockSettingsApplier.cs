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
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class MockSettingsApplier : ISettingsApplier
    {
        public void ApplyToSession(IApplicationSettings applicationSettings)
        {
            this.TimedAppliedToSession++;
        }

        public void RebuildSharedResources(IApplicationSettings applicationSettings)
        {
            this.Rebuilt++;
        }

        public int TimedAppliedToSession { get; private set; }

        public int Rebuilt { get; private set; }
    }
}
