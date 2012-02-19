using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using System.Windows.Navigation;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ListJobsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IJobUpdateService jobUpdateService;
        private ICommand addJobCommand;
        private ObservableCollection<Job> jobs;
        private ICommand updateStatusesCommand;
        private ICommand viewJobCommand;

        public ListJobsViewModel(INavigationService navigationService, IJobRepository jobRepository, ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService)
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

        public ICommand ViewJobCommand
        {
            get { return viewJobCommand; }
            private set { viewJobCommand = value; OnPropertyChanged("ViewJobCommand"); }
        }

        public ObservableCollection<Job> Jobs
        {
            get { return jobs; }
            set { jobs = value; OnPropertyChanged("Jobs"); }
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AddJobCommand = CreateCommand(new ObservableCommand(), OnAddJob);
            UpdateStatusesCommand = CreateCommand(new ObservableCommand(), OnUpdateStatuses);
            ViewJobCommand = CreateCommand<Job>(new ObservableCommand<Job>(), OnViewJob);

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Started += h, h => jobUpdateService.Started -= h)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => StartLoading("refreshing")));

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Complete += h, h => jobUpdateService.Complete -= h)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => RefreshJobs()));

            this.RefreshJobs();
        }

        private void RefreshJobs()
        {
            StartLoading("refreshing");

            jobRepository.GetJobs()
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(jobs => Jobs = new ObservableCollection<Job>(jobs));
        }

        private void OnViewJob(Job job)
        {
            TransitionMode = ViewModels.TransitionMode.ItemDetails;

            navigationService.Navigate(ViewUris.ViewJob(job));
        }

        private void OnAddJob()
        {
            TransitionMode = ViewModels.TransitionMode.NewItem;

            jobRepository.GetBuildServers()
                .Select(lst => lst.Count > 0)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(hasBuildServers =>
                    {
                        Uri uri = hasBuildServers 
                            ? ViewUris.SelectBuildServer 
                            : ViewUris.AddBuildServer;

                        navigationService.Navigate(uri);
                    });
        }

        private void OnUpdateStatuses()
        {
            jobUpdateService.UpdateAll(UpdateTimeout);
        }

        public ICommand AddJobCommand
        {
            get { return addJobCommand; }
            private set { addJobCommand = value; OnPropertyChanged("AddJobCommand"); }
        }

        private static readonly TimeSpan UpdateTimeout = TimeSpan.FromSeconds(30);
    }
}
