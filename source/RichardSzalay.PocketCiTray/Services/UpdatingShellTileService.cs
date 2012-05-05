using System.Linq;
using System.Collections.Generic;
using Microsoft.Phone.Shell;
using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public class UpdatingShellTileService : IShellTileService
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


        public void Remove(Uri uri)
        {
            ShellTile.ActiveTiles
                .First(x => x.NavigationUri == uri)
                .Delete();
        }
    }
}

