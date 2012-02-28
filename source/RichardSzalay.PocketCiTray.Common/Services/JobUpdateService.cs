using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using RichardSzalay.PocketCiTray.Providers;

namespace RichardSzalay.PocketCiTray.Services
{
    public class JobUpdateService : IJobUpdateService
    {
        private const string LastUpdateKey = "JobUpdateService.LastUpdateTime";

        private IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly IClock clock;
        private readonly ISettingsService settingsService;
        private readonly IMutexService mutexService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationTileService applicationTileService;
        private readonly IJobNotificationService jobNotificationService;

        public event EventHandler Started;
        private bool isUpdating;        

        private SerialDisposable disposable = new SerialDisposable();
        private TimeSpan BackgroundAgentTimeout;

        public JobUpdateService(IJobProviderFactory jobProviderFactory, IJobRepository jobRepository,
            IClock clock, ISettingsService settingsService, IMutexService mutexService,
            ISchedulerAccessor schedulerAccessor, IApplicationTileService applicationTileService,
            IJobNotificationService jobNotificationService)
        {
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.clock = clock;
            this.settingsService = settingsService;
            this.mutexService = mutexService;
            this.schedulerAccessor = schedulerAccessor;
            this.applicationTileService = applicationTileService;
            this.jobNotificationService = jobNotificationService;
        }

        public void UpdateAll(TimeSpan timeout)
        {
            OnStarted();
                
            Mutex mutex = mutexService.GetOwned(MutexNames.JobUpdateService, TimeSpan.FromMilliseconds(100));

            if (mutex == null)
            {
                schedulerAccessor.Background.Schedule(() =>
                {
                    BackgroundAgentTimeout = TimeSpan.FromSeconds(20);
                    mutexService.WaitOne(MutexNames.JobUpdateService, BackgroundAgentTimeout);

                    OnComplete(new List<Job>());
                });

                return;
            }

            if (isUpdating)
            {
                return;
            }

            isUpdating = true;

            mutex.ReleaseMutex();

            disposable.Disposable = jobRepository.GetJobs()
                .SelectMany(jobs => jobs.GroupBy(j => j.BuildServer))
                .SelectMany(group => jobProviderFactory
                    .Get(group.Key.Provider)
                    .UpdateAll(group.Key, group)
                    .Catch<Job, Exception>(ex =>
                    {
                        // TODO: Log the error in the provider

                        return Observable.Empty<Job>();
                    })
                )
                .Buffer(timeout).Take(1)
                .Subscribe(OnComplete);
        }

        private IObservable<ICollection<Job>> OnJobGroupUpdated(ICollection<Job> jobs)
        {
            return jobRepository.UpdateAll(jobs);
        }

        public event EventHandler Complete;

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
            var handler = this.Started;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnComplete(IList<Job> updatedJobs)
        {
            jobRepository.UpdateAll(updatedJobs)
                .SelectMany(_ => jobRepository.GetJobs())
                .Subscribe(allJobs =>
                    {
                        applicationTileService.UpdateAll(allJobs);

                        jobNotificationService.Notify(updatedJobs);

                        LastUpdateTime = clock.UtcNow;

                        isUpdating = false;

                        var handler = Complete;

                        if (handler != null)
                        {
                            handler(this, EventArgs.Empty);
                        }
                    });
        }

        public void Cancel()
        {
            disposable.Dispose();
        }

        
    }
}