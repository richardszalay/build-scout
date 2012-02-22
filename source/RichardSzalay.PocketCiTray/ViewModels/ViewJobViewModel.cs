using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ViewJobViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IApplicationTileService applicationTileService;
        private readonly IWebBrowserTaskFacade webBrowserTask;

        private ICommand addJobCommand;
        private Job job;

        public ViewJobViewModel(INavigationService navigationService, IJobRepository jobRepository, 
            ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService, 
            IApplicationTileService applicationTileService, IWebBrowserTaskFacade webBrowserTask)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.jobUpdateService = jobUpdateService;
            this.applicationTileService = applicationTileService;
            this.webBrowserTask = webBrowserTask;
        }

        public Job Job
        {
            get { return job; }
            private set { job = value; OnPropertyChanged("Job"); }
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey("jobId"))
            {
                navigationService.GoBack();
                return;
            }

            PinJobCommand = CreateCommand(new ObservableCommand(CanPin()), OnPin);
            ViewWebUrlCommand = CreateCommand(new ObservableCommand(CanViewWebUrl()), OnViewWebUrl);

            int jobId = Int32.Parse(query["jobId"]);

            Disposables.Add(jobRepository.GetJob(jobId)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(j => Job = j));
        }

        private ICommand pinJobCommand;
        public ICommand PinJobCommand
        {
            get { return pinJobCommand; }
            private set { pinJobCommand = value; OnPropertyChanged("PinJobCommand"); }
        }

        [NotifyProperty]
        public ICommand ViewWebUrlCommand { get; set; }

        private void OnPin()
        {
            applicationTileService.AddJobTile(Job);
        }

        private void OnViewWebUrl()
        {
            webBrowserTask.Show(Job.WebUri);
        }

        private IObservable<bool> CanPin()
        {
            return this.GetPropertyValues(x => x.Job)
                .Where(x => x != null)
                .Select(j => !applicationTileService.IsPinned(j));
        }

        private IObservable<bool> CanViewWebUrl()
        {
            return this.GetPropertyValues(x => x.Job)
                .Where(x => x != null)
                .Select(j => j.WebUri != null)
                .StartWith(false);
        }
    }
}
