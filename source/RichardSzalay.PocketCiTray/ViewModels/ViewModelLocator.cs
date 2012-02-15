using System.Net.Browser;
using System.Reactive.Concurrency;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ViewModelLocator
    {
        private readonly IJobRepository jobRepository = new JobRepository();
        private readonly IJobProviderFactory jobProviderFactory = new JobProviderFactory(WebRequestCreator.ClientHttp, new DateTimeOffsetClock());
        private readonly IJobUpdateService jobUpdateService;
        private readonly SchedulerAccessor schedulerAccessor;

        public ViewModelLocator()
        {
            schedulerAccessor = new SchedulerAccessor(DispatcherScheduler.Instance, Scheduler.ThreadPool);

            jobUpdateService = new JobUpdateService(schedulerAccessor, jobProviderFactory, jobRepository);
        }

        public ListJobsViewModel ListJobsViewModel
        {
            get
            {
                return new ListJobsViewModel(
                    new PhoneApplicationFrameNavigationService(((App)App.Current).RootFrame),
                    jobRepository,
                    schedulerAccessor,
                    jobUpdateService
                    );
            }
        }

        public SelectBuildServerViewModel SelectBuildServerViewModel
        {
            get { return new SelectBuildServerViewModel(jobRepository, jobProviderFactory, new PhoneApplicationFrameNavigationService(((App)App.Current).RootFrame)); }
        }

        public AddBuildServerViewModel AddBuildServerViewModel
        {
            get
            {
                return new AddBuildServerViewModel(
                    new PhoneApplicationFrameNavigationService(((App) App.Current).RootFrame),
                    jobProviderFactory, jobRepository,
                    DispatcherScheduler.Instance);
            }
        }

        public AddJobsViewModel AddJobsViewModel
        {
            get
            {
                return new AddJobsViewModel(
                    new PhoneApplicationFrameNavigationService(((App)App.Current).RootFrame),
                    jobProviderFactory, jobRepository,
                    schedulerAccessor);
            }
        }
    }
}
