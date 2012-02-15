using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Extensions;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Providers.Cruise;

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

            this.AddJobsCommand = CreateCommand(new ObservableCommand(CanAddJobs()), OnAddJobs);
        }

        public ICommand AddJobsCommand { get; private set; }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey("buildServerId"))
            {
                navigationService.GoBack();
                return;
            }

            int buildServerId = Int32.Parse(query["buildServerId"]);

            Observable.Return(buildServerId)
                .ObserveOn(schedulerAccessor.Background)
                .Select(id => buildServer = jobRepository.GetBuildServer(buildServerId))
                .SelectMany(server => jobProviderFactory.Get(server.Provider).GetJobsObservableAsync(server))
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(loadedJobs =>
                {
                    this.jobs = new ObservableCollection<Job>(loadedJobs);
                    this.OnPropertyChanged("BuildServer");
                    this.OnPropertyChanged("Jobs");
                });
        }

        private void OnAddJobs()
        {
            schedulerAccessor.Background.Schedule(() =>
            {
                jobRepository.AddJobs(selectedJobs);

                schedulerAccessor.UserInterface.Schedule(() =>
                {
                    navigationService.RemoveBackEntry();
                    navigationService.RemoveBackEntry();
                    navigationService.GoBack();
                });
            });
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
        private BuildServer buildServer;

        public ObservableCollection<Job> Jobs
        {
            get { return jobs; }
            private set
            {
                jobs = value;
                OnPropertyChanged("Jobs");
            }
        }

        private readonly ObservableCollection<Job> selectedJobs = new ObservableCollection<Job>();
        public ObservableCollection<Job> SelectedJobs
        {
            get { return selectedJobs; }
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
    }
}
