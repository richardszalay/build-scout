using System.Net.Browser;
using RichardSzalay.PocketCiTray.Providers.Cruise;
using System.Net;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Providers
{
    public class JobProviderFactory : IJobProviderFactory
    {
        private readonly IWebRequestCreate webRequestCreator;
        private readonly IClock clock;

        public JobProviderFactory(IWebRequestCreate webRequestCreator, IClock clock)
        {
            this.webRequestCreator = webRequestCreator;
            this.clock = clock;
        }

        public IJobProvider Get(string serverType)
        {
            return new CruiseProvider(webRequestCreator, clock);
        }
    }
}