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
    public class FakeNavigationService : INavigationService
    {

        public void Navigate(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void RemoveBackEntry()
        {
            throw new NotImplementedException();
        }

        public void GoBackTo(Uri pageUri)
        {
            throw new NotImplementedException();
        }

        public void GoBackToAny(params Uri[] pageUris)
        {
            throw new NotImplementedException();
        }


        public bool CanGoBack
        {
            get { throw new NotImplementedException(); }
        }
    }
}
