using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ListJobsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IJobUpdateService jobUpdateService;
        private readonly ICommand addJobCommand;
        private ObservableCollection<Job> jobs;
        private readonly ICommand updateStatusesCommand;

        public ListJobsViewModel(INavigationService navigationService, IJobRepository jobRepository, ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.jobUpdateService = jobUpdateService;

            addJobCommand = CreateCommand(new ObservableCommand(), OnAddJob);
            updateStatusesCommand = CreateCommand(new ObservableCommand(), OnUpdateStatuses);
        }

        public ICommand UpdateStatusesCommand
        {
            get { return updateStatusesCommand; }
        }

        public ObservableCollection<Job> Jobs
        {
            get { return jobs; }
            set { jobs = value; OnPropertyChanged("Jobs"); }
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Complete += h, h => jobUpdateService.Complete -= h)
                .Subscribe(_ => RefreshJobs()));

            this.RefreshJobs();
        }

        private void RefreshJobs()
        {
            schedulerAccessor.Background.Schedule(() =>
            {
                ICollection<Job> jobs = jobRepository.GetJobs();

                schedulerAccessor.UserInterface.Schedule(() =>
                {
                    this.Jobs = new ObservableCollection<Job>(jobs);
                });
            });
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
        }
    }
}
