using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Phone.Scheduler;

namespace RichardSzalay.PocketCiTray.Services
{
    public class PeriodicJobUpdateService : IPeriodicJobUpdateService
    {
        private static readonly TimeSpan TestBackgroundInterval = TimeSpan.FromSeconds(10);

        private readonly IJobUpdateService jobUpdateService;
        private readonly IScheduler scheduler;
        private readonly IScheduledActionServiceFacade scheduledActionService;

        public PeriodicJobUpdateService(IJobUpdateService jobUpdateService, IScheduler scheduler, IScheduledActionServiceFacade scheduledActionService)
        {
            this.jobUpdateService = jobUpdateService;
            this.scheduler = scheduler;
            this.scheduledActionService = scheduledActionService;
        }

        private readonly SerialDisposable subscription = new SerialDisposable();

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            subscription.Disposable = Observable.Timer(dueTime, period, scheduler)
                .Subscribe(_ => jobUpdateService.UpdateAll());
        }

        public void Stop()
        {
            subscription.Disposable = null;
        }

        public void UnregisterBackgroundTask()
        {
            scheduledActionService.Remove(PeriodicTaskName);
        }

        public void RegisterBackgroundTask()
        {
            scheduledActionService.Remove(PeriodicTaskName);
            scheduledActionService.Add(PeriodicTaskName, Strings.BackgroundUpdateAgentDescription);

            scheduledActionService.LaunchForTest(PeriodicTaskName, TestBackgroundInterval);
        }

        private const string PeriodicTaskName = "PocketBuild.BackgroundUpdateAgent";
    }
}