using System;
using System.Linq;
using Microsoft.Phone.Controls;
using RichardSzalay.PocketCiTray.Extensions.Extensions;

namespace RichardSzalay.PocketCiTray.Services
{
    public class PhoneApplicationFrameNavigationService : INavigationService
    {
        private readonly PhoneApplicationFrame rootVisual;

        public PhoneApplicationFrameNavigationService(PhoneApplicationFrame rootVisual)
        {
            this.rootVisual = rootVisual;
        }

        public void Navigate(Uri uri)
        {
            rootVisual.Navigate(uri);
        }

        public void GoBack()
        {
            rootVisual.GoBack();
        }

        public void RemoveBackEntry()
        {
            rootVisual.RemoveBackEntry();
        }

        public void GoBackTo(Uri pageUri)
        {
            GoBackToAny(pageUri);
        }

        public void GoBackToAny(params Uri[] pageUris)
        {
            while (rootVisual.CanGoBack)
            {
                var journeyEntry = rootVisual.BackStack.First();

                if (pageUris.Any(pageUri => journeyEntry.Source.MakeAbsolute().AbsolutePath == pageUri.MakeAbsolute().AbsolutePath))
                {
                    rootVisual.GoBack();
                    return;
                }

                rootVisual.RemoveBackEntry();
            }
        }


        public bool CanGoBack
        {
            get { return rootVisual.CanGoBack; }
        }
    }
}