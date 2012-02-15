using System;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public interface INavigationService
    {
        void Navigate(Uri uri);
        void GoBack();
        void RemoveBackEntry();
    }
}