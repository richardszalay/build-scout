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
using RichardSzalay.PocketCiTray.Infrastructure;

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

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SelectedJobs = new ObservableCollection<Job>();

            AddJobsCommand = CreateCommand(new ObservableCommand(CanAddJobs()), OnAddJobs);

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
                .Do(server => BuildServer = server)
                .SelectMany(server => jobProviderFactory
                    .Get(server.Provider)
                    .GetJobsObservableAsync(server)
                    .SelectMany(jobs => RemoveExistingJobs(server, jobs))
                )
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(loadedJobs => Jobs = new ObservableCollection<Job>(loadedJobs));
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
            jobRepository.AddJobs(SelectedJobs)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => navigationService.GoBackTo(ViewUris.ListJobs));
        }

        [NotifyProperty]
        public ICommand AddJobsCommand { get; private set; }

        [NotifyProperty]
        public string JobSource { get; private set; }

        [NotifyProperty(AlsoNotifyFor = new string[] { "BuildServer" })]
        public ObservableCollection<Job> Jobs { get; private set; }

        [NotifyProperty]
        public ObservableCollection<Job> SelectedJobs { get; private set; }

        private IObservable<bool> CanAddJobs()
        {
            return this.GetPropertyValues(x => x.SelectedJobs)
                .Where(selectedJobs => selectedJobs != null)
                .Select(selectedJobs => Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => new NotifyCollectionChangedEventHandler(h), 
                    h => selectedJobs.CollectionChanged += h,
                    h => selectedJobs.CollectionChanged -= h
                ))
                .Switch()
                .Select(s => SelectedJobs.Count > 0);
        }

        [DoNotNotify]
        public BuildServer BuildServer { get; private set; }

        [NotifyProperty]
        public bool IsSelectionEnabled { get; set; }
    }

    public class AvailableJob
    {
        public AvailableJob(Job job)
        {
            this.Job = job;
        }

        [NotifyProperty]
        public bool IsSelected { get; set; }

        public Job Job { get; private set; }
    }
}
