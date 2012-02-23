using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IIsolatedStorageFacade
    {
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        Stream CreateFile(string path);
    }

    public class IsolatedStorageFacade : IIsolatedStorageFacade
    {
        private readonly IsolatedStorageFile isolatedStorageFile;

        public IsolatedStorageFacade(IsolatedStorageFile isolatedStorageFile)
        {
            this.isolatedStorageFile = isolatedStorageFile;
        }

        public bool DirectoryExists(string path)
        {
            return isolatedStorageFile.DirectoryExists(path);
        }

        public void CreateDirectory(string path)
        {
            isolatedStorageFile.CreateDirectory(path);
        }

        public Stream CreateFile(string path)
        {
            return isolatedStorageFile.CreateFile(path);
        }
    }
}