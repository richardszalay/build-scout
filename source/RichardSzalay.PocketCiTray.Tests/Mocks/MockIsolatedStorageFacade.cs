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
    public class MockIsolatedStorageFacade : IIsolatedStorageFacade
    {
        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream CreateFile(string path)
        {
            return CreatedFiles[path] = new MemoryStream();
        }

        public System.IO.Stream OpenFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, MemoryStream> CreatedFiles = new Dictionary<string, MemoryStream>();
    }
}
