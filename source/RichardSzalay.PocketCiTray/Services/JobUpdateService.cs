using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.ViewModels;

namespace RichardSzalay.PocketCiTray.Services
{
    public class JobUpdateService : IJobUpdateService
    {
        private readonly ISchedulerAccessor schedulerAccessor;
        private IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;

        private SerialDisposable disposable = new SerialDisposable();

        public JobUpdateService(ISchedulerAccessor schedulerAccessor, IJobProviderFactory jobProviderFactory, IJobRepository jobRepository)
        {
            this.schedulerAccessor = schedulerAccessor;
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
        }

        public void UpdateAll()
        {
            disposable.Disposable = GetJobs()
                .SelectMany(jobs => jobs.GroupBy(j => j.BuildServer))
                .SelectMany(group => jobProviderFactory.Get(group.Key.Provider).UpdateAll(group.Key, group))
                .Finally(OnComplete)
                .Subscribe(OnJobGroupUpdated);
        }

        private IObservable<ICollection<Job>> GetJobs()
        {
            return Observable.Return(jobRepository)
                .SubscribeOn(schedulerAccessor.Background)
                .Select(repo => repo.GetJobs());
        }

        private void OnJobGroupUpdated(ICollection<Job> jobs)
        {
            jobRepository.UpdateAll(jobs);
        }

        public event EventHandler Complete;

        private void OnComplete()
        {
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