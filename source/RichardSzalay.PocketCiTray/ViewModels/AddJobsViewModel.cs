using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Disposables;
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
        private const string BuildServerIdKey = "buildServerId";
        private readonly INavigationService navigationService;
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private ObservableCollection<AvailableJob> allJobs;

        private SerialDisposable addJobsSubscrition;

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

            Jobs = new ObservableCollection<AvailableJob>();
            SelectedJobs = new ObservableCollection<Job>();

            Disposables.Add(addJobsSubscrition = new SerialDisposable());

            AddJobsCommand = CreateCommand(new ObservableCommand(CanAddJobs()), OnAddJobs);
            SelectAllJobsCommand = CreateCommand(new ObservableCommand(), OnSelectAllJobs);
            FilterJobsCommand = CreateCommand(new ObservableCommand(), OnFilterJobs);

            Disposables.Add(this.GetPropertyValues(x => x.FilterText)
                .Skip(1)
                .Select(filter => Observable.ToAsync(() => OnUpdateFilter(filter), schedulerAccessor.Background)())
                .Switch()
                .Select(jobs => new ObservableCollection<AvailableJob>(jobs))
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(jobs => Jobs = jobs));

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey(BuildServerIdKey))
            {
                navigationService.GoBack();
                return;
            }

            IsSelectionEnabled = true;

            int buildServerId = Int32.Parse(query[BuildServerIdKey]);

            StartLoading(Strings.FindingJobsStatusMessage);

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
                .Subscribe(loadedJobs => allJobs = Jobs = new ObservableCollection<AvailableJob>(loadedJobs.Select(CreateAvailableJob)));
        }

        private string previousFilterText;

        [NotifyProperty]
        public string FilterText { get; set; }

        public override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (ShowFilter)
            {
                e.Cancel = true;
                ShowFilter = false;
            }
            else
            {
                navigationService.GoBackToAny(ViewUris.SelectBuildServer, ViewUris.ListJobs);
            }
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
            var provider = jobProviderFactory.Get(BuildServer.Provider);

            bool jobsAlreadyHaveStatuses = (provider.Features & JobProviderFeature.JobDiscoveryIncludesStatus) != 0;

            IObservable<IList<Job>> jobsWithStatuses = (jobsAlreadyHaveStatuses)
                ? Observable.Return((IList<Job>)SelectedJobs)
                : provider.UpdateAll(BuildServer, SelectedJobs).ToList();

            StartLoading(Strings.UpdatingStatusMessage);

            addJobsSubscrition.Disposable = jobsWithStatuses
                .SelectMany(jobRepository.AddJobs)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(_ => navigationService.GoBackTo(ViewUris.ListJobs));
        }

        private void OnSelectAllJobs()
        {
            if (SelectedJobs.Count == 0)
            {
                foreach (var availableJob in Jobs)
                {
                    availableJob.IsSelected = true;
                }
            }
            else
            {
                foreach (var availableJob in Jobs)
                {
                    availableJob.IsSelected = false;
                }

            }
        }

        private void OnFilterJobs()
        {
            ShowFilter = true;
        }

        private ICollection<AvailableJob> OnUpdateFilter(string filter)
        {
            if (filter == null)
            {
                previousFilterText = null;

                return allJobs;
            }

            var filterSource = (previousFilterText != null && filter.StartsWith(previousFilterText))
                ? Jobs
                : allJobs;

            previousFilterText = filter;

            var filteredJobs = filterSource.Where(j => j.Job.Name.IndexOf(
                filter, StringComparison.InvariantCultureIgnoreCase) != -1);

            return new ObservableCollection<AvailableJob>(filteredJobs);
        }

        private AvailableJob CreateAvailableJob(Job job)
        {
            var availableJob = new AvailableJob(job);

            availableJob.SelectionChanged += OnJobSelectionChanged;

            return availableJob;
        }

        private void OnJobSelectionChanged(object sender, EventArgs e)
        {
            var availableJob = (AvailableJob) sender;

            if (availableJob.IsSelected)
            {
                SelectedJobs.Add(availableJob.Job);
            }
            else
            {
                SelectedJobs.Remove(availableJob.Job);
            }
        }

        private bool showFilter;

        [NotifyProperty]
        public bool ShowFilter
        {
            get
            {
                return showFilter;
            }
            private set
            {
                showFilter = value;

                if (!value)
                {
                    FilterText = null;
                }
            }
        }

        [NotifyProperty]
        public ICommand AddJobsCommand { get; private set; }

        [NotifyProperty]
        public ICommand SelectAllJobsCommand { get; private set; }

        [NotifyProperty]
        public ICommand FilterJobsCommand { get; private set; }

        [NotifyProperty]
        public string JobSource { get; private set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "BuildServer" })]
        public ObservableCollection<AvailableJob> Jobs { get; private set; }

        [NotifyProperty]
        public ObservableCollection<Job> SelectedJobs { get; private set; }

        private IObservable<bool> CanAddJobs()
        {
            IObservable<bool> isNotLoading = this.GetPropertyValues(x => x.ProgressIndicator)
                .Select(p => p == null || !p.IsVisible);

            IObservable<bool> hasSelection = this.GetPropertyValues(x => x.SelectedJobs)
                .Where(selectedJobs => selectedJobs != null)
                .Select(selectedJobs => Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => new NotifyCollectionChangedEventHandler(h), 
                    h => selectedJobs.CollectionChanged += h,
                    h => selectedJobs.CollectionChanged -= h
                ))
                .Switch()
                .Select(s => SelectedJobs.Count > 0);

            return isNotLoading.CombineLatest(hasSelection, (l,r) => l && r);
        }

        [DoNotNotify]
        public BuildServer BuildServer { get; private set; }

        [NotifyProperty]
        public bool IsSelectionEnabled { get; set; }
    }

    public class AvailableJob : PropertyChangeBase
    {
        public AvailableJob(Job job)
        {
            this.Job = job;

            var cmd =new ObservableCommand();
            cmd.Subscribe(_ => OnToggleSelection());

            this.ToggleSelectionCommand = cmd;
        }

        private bool isSelected;

        [NotifyProperty]
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnSelectionChanged(); }
        }

        public ICommand ToggleSelectionCommand { get; private set; }

        public event EventHandler SelectionChanged;

        private void OnSelectionChanged()
        {
            var handler = SelectionChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnToggleSelection()
        {
            IsSelected = !isSelected;
        }

        public Job Job { get; private set; }
    }
}
