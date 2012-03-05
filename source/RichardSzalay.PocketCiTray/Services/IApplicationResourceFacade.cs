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
        private ResourceDictionary resources;

        public ApplicationResourceFacade()
        {
            this.resources = Application.Current.Resources;
        }

        public Stream GetResourceStream(Uri resourceUri)
        {
            return Application.GetResourceStream(resourceUri).Stream;
        }

        public T GetResource<T>(string key)
        {
            return (T)resources[key];
        }
    }
}