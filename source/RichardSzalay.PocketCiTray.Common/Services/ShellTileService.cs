using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using Microsoft.Phone.Shell;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ShellTileService : RichardSzalay.PocketCiTray.Services.IShellTileService
    {
        public IEnumerable<Uri> GetUris()
        {
            return ShellTile.ActiveTiles.Select(t => t.NavigationUri);
        }

        public void Create(Uri navigationUri, StandardTileData tileData)
        {
            ShellTile.Create(navigationUri, tileData);
        }

        public void Update(Uri navigationUri, StandardTileData tileData)
        {
            var shellTile = ShellTile.ActiveTiles
                .FirstOrDefault(t => t.NavigationUri == navigationUri);

            if (shellTile != null)
            {
                shellTile.Update(tileData);
            }
        }
    }
}
