using System;
namespace RichardSzalay.PocketCiTray.Services
{
    public interface IShellTileService
    {
        void Create(Uri navigationUri, Microsoft.Phone.Shell.StandardTileData tileData);
        System.Collections.Generic.IEnumerable<Uri> GetUris();
        void Update(Uri navigationUri, Microsoft.Phone.Shell.StandardTileData tileData);
    }
}
