using System.Net;
using RichardSzalay.PocketCiTray.Services;
using System;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Providers
{
    public class JobProviderFactory : IJobProviderFactory
    {
        private readonly IWebRequestCreate webRequestCreator;
        private readonly IClock clock;
        private readonly ISchedulerAccessor schedulerAccessor;

        public JobProviderFactory(IWebRequestCreate webRequestCreator, IClock clock, ISchedulerAccessor schedulerAccessor)
        {
            this.webRequestCreator = webRequestCreator;
            this.clock = clock;
            this.schedulerAccessor = schedulerAccessor;
        }

        public ICollection<string> GetProviders()
        {
            return new string[]
            {
                CruiseProvider.ProviderName,
                HudsonProvider.ProviderName
            };
        }

        public IJobProvider Get(string serverType)
        {
            switch (serverType)
            {
                case CruiseProvider.ProviderName:
                    return new CruiseProvider(webRequestCreator, clock);

                case HudsonProvider.ProviderName:
                    return new HudsonProvider(webRequestCreator, clock);

                default:
                    throw new ArgumentException("Invalid provider: " + serverType);
            }
        }
    }
}