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
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class StubSettingsService : ISettingsService
    {

        public System.Collections.Generic.IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>();
        }

        public void SaveSettings(System.Collections.Generic.IDictionary<string, object> modifiedValues)
        {
        }
    }
}
