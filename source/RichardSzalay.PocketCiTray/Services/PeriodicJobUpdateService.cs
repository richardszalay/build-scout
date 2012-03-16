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

        private static readonly TimeSpan UpdateTimeout = TimeSpan.FromSeconds(30);

        private readonly IJobUpdateService jobUpdateService;
        private readonly IScheduler scheduler;
        private readonly IScheduledActionServiceFacade scheduledActionService;
        private readonly IDeviceInformationService deviceInformationService;

        public PeriodicJobUpdateService(IJobUpdateService jobUpdateService, IScheduler scheduler, 
            IScheduledActionServiceFacade scheduledActionService, IDeviceInformationService deviceInformationService)
        {
            this.jobUpdateService = jobUpdateService;
            this.scheduler = scheduler;
            this.scheduledActionService = scheduledActionService;
            this.deviceInformationService = deviceInformationService;
        }

        private readonly SerialDisposable subscription = new SerialDisposable();

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            subscription.Disposable = Observable.Timer(dueTime, period, scheduler)
                .Subscribe(_ => jobUpdateService.UpdateAll(UpdateTimeout));
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

#if DEBUG_AGENT
            scheduledActionService.LaunchForTest(PeriodicTaskName, TimeSpan.Zero);
#endif
        }

        public bool CanRegisterBackgroundTask
        {
            get { return !deviceInformationService.IsLowEndDevice; }
        }

        private const string PeriodicTaskName = "BuildScout.BackgroundUpdateAgent";
    }
}