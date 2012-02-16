using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Phone.Scheduler;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IPeriodicJobUpdateService
    {
        void Start(TimeSpan dueTime, TimeSpan period);

        void Stop();
        void UnregisterBackgroundTask();
        void RegisterBackgroundTask();
    }

    public class PeriodicJobUpdateService : IPeriodicJobUpdateService
    {
        private readonly IJobUpdateService jobUpdateService;
        private readonly IScheduler scheduler;

        public PeriodicJobUpdateService(IJobUpdateService jobUpdateService, IScheduler scheduler)
        {
            this.jobUpdateService = jobUpdateService;
            this.scheduler = scheduler;
        }

        private SerialDisposable subscription = new SerialDisposable();

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            subscription.Disposable = ObservableExtensions.Subscribe<long>(Observable.Timer(dueTime, period, scheduler), _ => jobUpdateService.UpdateAll());
        }

        public void Stop()
        {
            subscription.Disposable = null;
        }

        public void UnregisterBackgroundTask()
        {
            ScheduledActionService.Remove(PeriodicTaskName);
        }

        public void RegisterBackgroundTask()
        {
            ScheduledActionService.Add(new PeriodicTask(PeriodicTaskName)
            {
                Description = Strings.BackgroundUpdateAgentDescription
            });
        }

        private const string PeriodicTaskName = "PocketBuild.BackgroundUpdateAgent";
    }
}
