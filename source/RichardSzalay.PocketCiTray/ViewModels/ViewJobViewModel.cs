using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ViewJobViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IApplicationTileService applicationTileService;
        private ICommand addJobCommand;
        private Job job;

        public ViewJobViewModel(INavigationService navigationService, IJobRepository jobRepository, ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService, IApplicationTileService applicationTileService)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.jobUpdateService = jobUpdateService;
            this.applicationTileService = applicationTileService;
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

        private void OnPin()
        {
            applicationTileService.AddJobTile(Job);
        }

        private IObservable<bool> CanPin()
        {
            return this.GetPropertyValues(x => x.Job)
                .Select(j => !applicationTileService.IsPinned(j));
        }
    }
}
