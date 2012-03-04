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
using System.IO;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class FakeApplicationResourceFacade : IApplicationResourceFacade
    {
        public System.IO.Stream GetResourceStream(Uri sharedContentUri)
        {
            throw new NotImplementedException();
        }

        public T GetResource<T>(string key)
        {
            return (T)applicationResources[key];
        }

        public void SetResource(string key, object value)
        {
            applicationResources[key] = value;
        }

        private Dictionary<string, object> applicationResources = new Dictionary<string, object>();
    }
}
