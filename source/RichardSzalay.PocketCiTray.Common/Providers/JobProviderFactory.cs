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
        private readonly ILog log;

        public JobProviderFactory(IWebRequestCreate webRequestCreator, IClock clock, 
            ISchedulerAccessor schedulerAccessor, ILog log)
        {
            this.webRequestCreator = webRequestCreator;
            this.clock = clock;
            this.schedulerAccessor = schedulerAccessor;
            this.log = log;
        }

        public ICollection<string> GetProviders()
        {
            return new string[]
            {
                CcTrayProvider.ProviderName,
                HudsonProvider.ProviderName,
                TeamCity6Provider.ProviderName
            };
        }

        public IJobProvider Get(string serverType)
        {
            switch (serverType)
            {
                case CcTrayProvider.ProviderName:
                    return new CcTrayProvider(webRequestCreator, clock);

                case HudsonProvider.ProviderName:
                    return new HudsonProvider(webRequestCreator, clock);

                case TeamCity6Provider.ProviderName:
                    return new TeamCity6Provider(webRequestCreator, clock, log);

                default:
                    throw new ArgumentException("Invalid provider: " + serverType);
            }
        }
    }
}