using System;
using Microsoft.Phone.Controls;

namespace RichardSzalay.PocketCiTray.ViewModels
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
    }
}