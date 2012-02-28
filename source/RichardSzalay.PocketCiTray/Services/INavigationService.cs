using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface INavigationService
    {
        void Navigate(Uri uri);
        void GoBack();
        void RemoveBackEntry();
        void GoBackTo(Uri pageUri);
        void GoBackToAny(params Uri[] pageUris);
    }
}