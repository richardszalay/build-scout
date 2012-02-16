using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using RichardSzalay.PocketCiTray.Providers;

namespace RichardSzalay.PocketCiTray.Services
{
    public class JobUpdateService : IJobUpdateService
    {
        private const string LastUpdateKey = "JobUpdateService.LastUpdateTime";

        private IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly IClock clock;
        private readonly ISettingsFacade settings;

        private SerialDisposable disposable = new SerialDisposable();

        public JobUpdateService(IJobProviderFactory jobProviderFactory, IJobRepository jobRepository,
            IClock clock, ISettingsFacade settings)
        {
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.clock = clock;
            this.settings = settings;
        }

        public void UpdateAll()
        {
            OnStarted();

            disposable.Disposable = jobRepository.GetJobs()
                .SelectMany(jobs => jobs.GroupBy(j => j.BuildServer))
                .SelectMany(group => jobProviderFactory.Get(group.Key.Provider).UpdateAll(group.Key, group))
                .Finally(OnComplete)
                .Subscribe(OnJobGroupUpdated);
        }

        private void OnJobGroupUpdated(ICollection<Job> jobs)
        {
            jobRepository.UpdateAll(jobs);
        }

        public event EventHandler Complete;

        public DateTimeOffset? LastUpdateTime
        {
            get
            {
                return settings.ContainsKey(LastUpdateKey)
                           ? DateTimeOffset.Parse((string)settings[LastUpdateKey], CultureInfo.InvariantCulture)
                           : (DateTimeOffset?)null;
            }

            private set { settings[LastUpdateKey] = value.Value.ToString(CultureInfo.InvariantCulture); settings.Save(); }
        }

        private void OnStarted()
        {
            var handler = this.Started;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnComplete()
        {
            LastUpdateTime = clock.UtcNow;

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

        public event EventHandler Started;
    }
}