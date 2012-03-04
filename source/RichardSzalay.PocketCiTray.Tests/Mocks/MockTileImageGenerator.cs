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
using System.Collections.Generic;
using System.IO;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class MockTileImageGenerator : ITileImageGenerator
    {
        public void Create(Color color, System.IO.Stream outputStream)
        {
            CreatedTiles[outputStream] = color;
        }

        public Dictionary<Stream, Color> CreatedTiles = new Dictionary<Stream, Color>();
    }
}
