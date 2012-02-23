using System;
using System.IO;
using System.Windows;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IApplicationResourceFacade
    {
        Stream GetResourceStream(Uri sharedContentUri);
    }

    public class ApplicationResourceFacade : IApplicationResourceFacade
    {
        public Stream GetResourceStream(Uri resourceUri)
        {
            return Application.GetResourceStream(resourceUri).Stream;
        }
    }
}