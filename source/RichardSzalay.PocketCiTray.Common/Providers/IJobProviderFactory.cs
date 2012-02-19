using System.Collections.Generic;
namespace RichardSzalay.PocketCiTray.Providers
{
    public interface IJobProviderFactory
    {
        ICollection<string> GetProviders();
        IJobProvider Get(string serverType);
    }
}