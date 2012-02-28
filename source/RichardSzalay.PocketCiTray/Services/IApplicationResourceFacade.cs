using System;
using System.IO;
using System.Windows;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationResourceFacade
    {
        Stream GetResourceStream(Uri sharedContentUri);
        T GetResource<T>(string key);
    }

    public class ApplicationResourceFacade : IApplicationResourceFacade
    {
        public Stream GetResourceStream(Uri resourceUri)
        {
            return Application.GetResourceStream(resourceUri).Stream;
        }

        public T GetResource<T>(string key)
        {
            return (T)Application.Current.Resources[key];
        }
    }
}