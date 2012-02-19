using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class AddJobsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;

        public AddJobsViewModel(INavigationService navigationService, IJobProviderFactory jobProviderFactory, IJobRepository jobRepository, ISchedulerAccessor schedulerAccessor)
        {
            this.navigationService = navigationService;
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
        }

        public ICommand AddJobsCommand { get; private set; }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            selectedJobs = new ObservableCollection<Job>();

            this.AddJobsCommand = CreateCommand(new ObservableCommand(CanAddJobs()), OnAddJobs);
            OnPropertyChanged("AddJobsCommand");

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey("buildServerId"))
            {
                navigationService.GoBack();
                return;
            }

            IsSelectionEnabled = true;

            int buildServerId = Int32.Parse(query["buildServerId"]);

            StartLoading("finding jobs");

            Observable.Return(buildServerId)
                .ObserveOn(schedulerAccessor.Background)
                .SelectMany(id => jobRepository.GetBuildServer(buildServerId))
                .Do(server => buildServer = server)
                .SelectMany(server => jobProviderFactory
                    .Get(server.Provider)
                    .GetJobsObservableAsync(server)
                    .SelectMany(jobs => RemoveExistingJobs(server, jobs))
                )
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(loadedJobs =>
                {
                    OnPropertyChanged("BuildServer");
                    this.jobs = new ObservableCollection<Job>(loadedJobs);
                    this.OnPropertyChanged("BuildServer");
                    this.OnPropertyChanged("Jobs");
                });
        }

        private IObservable<ICollection<Job>> RemoveExistingJobs(BuildServer buildServer, ICollection<Job> jobs)
        {
            return jobRepository.GetJobs()
                .Select(existingJobs => existingJobs.Where(j => j.BuildServer.Equals(buildServer)))
                .Select(existingJobs => jobs.Except(existingJobs))
                .Select(filteredJobs => (ICollection<Job>)filteredJobs.ToList());
        }

        private void OnAddJobs()
        {
            jobRepository.AddJobs(selectedJobs)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => navigationService.GoBackTo(ViewUris.ListJobs));
        }

        private string jobSource;
        public string JobSource
        {
            get { return jobSource; }
            set
            {
                jobSource = value;
                OnPropertyChanged("JobSource");
            }
        }

        private ObservableCollection<Job> jobs;

        public ObservableCollection<Job> Jobs
        {
            get { return jobs; }
            private set
            {
                jobs = value;
                OnPropertyChanged("Jobs");
            }
        }

        private bool isSelectionEnabled;
        public bool IsSelectionEnabled
        {
            get { return isSelectionEnabled; }
            set { isSelectionEnabled = value; OnPropertyChanged("IsSelectionEnabled"); }
        }

        private ObservableCollection<Job> selectedJobs;
        private PocketCiTray.BuildServer buildServer;
        public ObservableCollection<Job> SelectedJobs
        {
            get { return selectedJobs; }
            private set { selectedJobs = value; OnPropertyChanged("SelectedJobs"); }
        }

        private IObservable<bool> CanAddJobs()
        {
            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => new NotifyCollectionChangedEventHandler(h), 
                h => selectedJobs.CollectionChanged += h,
                h => selectedJobs.CollectionChanged -= h
                )
                .Select(s => selectedJobs.Count > 0);
        }

        public BuildServer BuildServer
        {
            get { return buildServer; }
            set { buildServer = value; OnPropertyChanged("BuildServer"); }
        }
    }
}
