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
        private ICommand addJobCommand;
        private Job job;
        private ICommand updateStatusesCommand;

        public ViewJobViewModel(INavigationService navigationService, IJobRepository jobRepository, ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.jobUpdateService = jobUpdateService;
        }

        public ICommand UpdateStatusesCommand
        {
            get { return updateStatusesCommand; }
            private set { updateStatusesCommand = value; OnPropertyChanged("UpdateStatusesCommand"); }
        }

        public Job Job
        {
            get { return job; }
            private set { job = value; OnPropertyChanged("Job"); }
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AddJobCommand = CreateCommand(new ObservableCommand(), OnAddJob);
            UpdateStatusesCommand = CreateCommand(new ObservableCommand(), OnUpdateStatuses);

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey("jobId"))
            {
                navigationService.GoBack();
                return;
            }

            int jobId = Int32.Parse(query["jobId"]);

            Disposables.Add(jobRepository.GetJob(jobId)
                .Subscribe(j => Job = j));
        }

        private void OnAddJob()
        {
            navigationService.Navigate(ViewUris.SelectBuildServer);
        }

        private void OnUpdateStatuses()
        {
            jobUpdateService.UpdateAll();
        }

        public ICommand AddJobCommand
        {
            get { return addJobCommand; }
            private set { addJobCommand = value; OnPropertyChanged("AddJobCommand"); }
        }
    }
}
