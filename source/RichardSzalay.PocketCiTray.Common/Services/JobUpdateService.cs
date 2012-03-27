using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Extensions;
using System.Diagnostics;
using System.Net;
using WP7Contrib.Logging;

namespace RichardSzalay.PocketCiTray.Services
{
    public class JobUpdateService : IJobUpdateService
    {
        private const string LastUpdateKey = "JobUpdateService.LastUpdateTime";

        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly IClock clock;
        private readonly ISettingsService settingsService;
        private readonly IMutexService mutexService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationTileService applicationTileService;
        private readonly IJobNotificationService jobNotificationService;
        private readonly ILog log;
        private readonly INetworkInterfaceFacade networkInterfaceFacade;
        private readonly ITrackingService trackingService;

        public event EventHandler Started;
        public event EventHandler Complete;
        private bool isUpdating;        

        private SerialDisposable disposable = new SerialDisposable();
        private readonly TimeSpan BackgroundAgentTimeout = TimeSpan.FromSeconds(20);

        public JobUpdateService(IJobProviderFactory jobProviderFactory, IJobRepository jobRepository,
            IClock clock, ISettingsService settingsService, IMutexService mutexService,
            ISchedulerAccessor schedulerAccessor, IApplicationTileService applicationTileService,
            IJobNotificationService jobNotificationService, ILog log,
            INetworkInterfaceFacade networkInterfaceFacade, ITrackingService trackingService)
        {
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.clock = clock;
            this.settingsService = settingsService;
            this.mutexService = mutexService;
            this.schedulerAccessor = schedulerAccessor;
            this.applicationTileService = applicationTileService;
            this.jobNotificationService = jobNotificationService;
            this.log = log;
            this.networkInterfaceFacade = networkInterfaceFacade;
            this.trackingService = trackingService;
        }

        public void UpdateAll(TimeSpan timeout)
        {
            OnStarted();

            using (var mutex = mutexService.GetOwned(MutexNames.JobUpdateService, TimeSpan.FromMilliseconds(100)))
            {
                if (mutex == null)
                {
                    schedulerAccessor.Background.Schedule(() =>
                    {
                        if (!Debugger.IsAttached)
                        {
                            mutexService.WaitOne(MutexNames.JobUpdateService, BackgroundAgentTimeout);
                        }

                        CommitUpdatedJobs(new List<Job>());                        
                    });

                    return;
                }

                if (isUpdating)
                {
                    return;
                }

                isUpdating = true;
            }

            var connectionAvailableAsync = Observable.ToAsync(() => networkInterfaceFacade.IsConnectionAvailable, 
                schedulerAccessor.Background)();

            disposable.Disposable = connectionAvailableAsync
                .SelectMany(connectionAvailable =>
                {
                    if (connectionAvailable)
                    {
                        return UpdateJobs(jobRepository.GetJobs(), timeout);
                    }
                    else
                    {
                        log.Write("Job update skipped: no network connectivity");

                        return Observable.Empty<IList<Job>>();
                    }
                })
                .Subscribe(CommitUpdatedJobs, OnFailed, OnCompleted);
        }

        private IObservable<IList<Job>> UpdateJobs(ICollection<Job> jobs, TimeSpan timeout)
        {
            var serverGroups = jobs.GroupBy(j => j.BuildServer);

            return serverGroups
                .ToObservable(schedulerAccessor.Background)
                .SelectMany(group => jobProviderFactory
                    .Get(group.Key.Provider)
                    .UpdateAll(group.Key, group)
                    .Catch<Job, Exception>(ex =>
                    {
                        log.Write("Job update failed for {0} ({1}): {2}",
                            group.Key.Name, group.Key.Provider, WebExceptionService.GetDebugMessage(ex));

                        if (WebExceptionService.IsJobUnavailable(ex))
                        {
                            return group.Select(x => x.MakeUnavailable(ex, clock.UtcNow))
                                .ToObservable(schedulerAccessor.Background);
                        }

                        return Observable.Empty<Job>();
                    })
                )
                .Buffer(timeout).Take(1);
        }

        public DateTimeOffset? LastUpdateTime
        {
            get
            {
                var settings = this.settingsService.GetSettings();

                return settings.ContainsKey(LastUpdateKey)
                           ? (DateTimeOffset)settings[LastUpdateKey]
                           : (DateTimeOffset?)null;
            }

            private set { settingsService.SaveSettings(new Dictionary<string, object> { { LastUpdateKey, value }}); }
        }

        private void OnStarted()
        {
            log.Write("Job update started");

            var handler = this.Started;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void CommitUpdatedJobs(IList<Job> updatedJobs)
        {
            log.Write("Job update complete - {0} jobs updated", updatedJobs.Count);

            jobRepository.UpdateAll(updatedJobs);

            var allJobs = jobRepository.GetJobs();
            applicationTileService.UpdateAll(allJobs);

            jobNotificationService.Notify(updatedJobs);

            LastUpdateTime = clock.UtcNow;
        }

        private void OnFailed(Exception ex)
        {
            log.Write("Job updated failed", ex);
        }

        private void OnCompleted()
        {
            isUpdating = false;

            var handler = Complete;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Cancel()
        {
            disposable.Dispose();
        }

        
    }
}   