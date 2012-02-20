﻿using System.Net;
using RichardSzalay.PocketCiTray.Services;
using System;
using System.Collections.Generic;
using WP7Contrib.Logging;

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

                case TeamCity6Provider.ProviderName:
                    return new TeamCity6Provider(webRequestCreator, clock, log);

                default:
                    throw new ArgumentException("Invalid provider: " + serverType);
            }
        }
    }
}