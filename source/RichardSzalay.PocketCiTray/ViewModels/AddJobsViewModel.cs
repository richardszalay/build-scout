using System;
using System.ComponentModel;
using System.Diagnostics;
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
using RichardSzalay.PocketCiTray.Controllers;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class AddJobsViewModel : ViewModelBase
    {
        private const string BuildServerIdKey = "buildServerId";
        private readonly INavigationService navigationService;
        private readonly IJobController jobController;
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;

        private SerialDisposable addJobsSubscrition;

        public AddJobsViewModel(INavigationService navigationService, IJobController jobController,
            IJobProviderFactory jobProviderFactory, IJobRepository jobRepository, ISchedulerAccessor schedulerAccessor)
        {
            this.navigationService = navigationService;
            this.jobController = jobController;
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SelectedJobs = new ObservableCollection<Job>();

            Disposables.Add(addJobsSubscrition = new SerialDisposable());

            AddJobsCommand = CreateCommand(new ObservableCommand(CanAddJobs()), OnAddJobs);
            SelectAllJobsCommand = CreateCommand(new ObservableCommand(), OnSelectAllJobs);
            FilterJobsCommand = CreateCommand(new ObservableCommand(), OnFilterJobs);

            Disposables.Add(this.GetPropertyValues(x => x.FilterText)
                .Where(f => f != null && Jobs.Filter != f)
                .Sample(TimeSpan.FromMilliseconds(200), schedulerAccessor.UserInterface)
                .Select(filter => Observable.ToAsync(() => Jobs.Filter = filter, schedulerAccessor.Background)())
                .Switch()
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => State = (Jobs.Count == 0)
                    ? AddJobsViewState.NoResults
                    : AddJobsViewState.Results));

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey(BuildServerIdKey))
            {
                navigationService.GoBack();
                return;
            }

            IsSelectionEnabled = true;

            int buildServerId = Int32.Parse(query[BuildServerIdKey]);

            StartLoading(Strings.FindingJobsStatusMessage);

            BuildServer = jobRepository.GetBuildServer(buildServerId);

            this.State = AddJobsViewState.Loading;

            jobProviderFactory
                .Get(BuildServer.Provider)
                .GetJobsObservableAsync(BuildServer)
                .Select(jobs => RemoveExistingJobs(BuildServer, jobs))
                .Select(jobs => jobs.Select(CreateAvailableJob)
                    .OrderBy(j => j.Job.Name)
                    .ToList()
                )
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(loadedJobs =>
                {
                    allJobs = loadedJobs;
                    Jobs = new FilteredObservableCollection<AvailableJob>(loadedJobs, FilterJob, schedulerAccessor.UserInterface);

                    State = (Jobs.Count > 0)
                        ? AddJobsViewState.Results
                        : AddJobsViewState.NoResults;
                }, ex =>
                {
                    ErrorDescription = ex.Message;
                    State = AddJobsViewState.Error;
                });
        }

        private bool FilterJob(AvailableJob job, string filter)
        {
            return filter == null || 
                job.Job.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        [NotifyProperty]
        public string FilterText { get; set; }

        public override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (ShowFilter)
            {
                Jobs.Filter = null;
                FilterText = null;
                ShowFilter = false;

                State = Jobs.Count == 0
                    ? AddJobsViewState.NoResults
                    : AddJobsViewState.Results;

                e.Cancel = true;
            }
            else
            {
                navigationService.GoBackToAny(ViewUris.SelectBuildServer, ViewUris.ListJobs);
            }
        }

        private ICollection<Job> RemoveExistingJobs(BuildServer buildServer, ICollection<Job> jobs)
        {
            var existingJobs = jobRepository.GetJobs(buildServer);

            return jobs.Except(existingJobs)
                .ToList();
        }

        private void OnAddJobs()
        {
            var provider = jobProviderFactory.Get(BuildServer.Provider);

            bool jobsAlreadyHaveStatuses = (provider.Features & JobProviderFeature.JobDiscoveryIncludesStatus) != 0;

            IObservable<IList<Job>> jobsWithStatuses = (jobsAlreadyHaveStatuses)
                ? Observable.Return((IList<Job>)SelectedJobs)
                : provider.UpdateAll(BuildServer, SelectedJobs).ToList();

            StartLoading(Strings.UpdatingStatusMessage);

            addJobsSubscrition.Disposable = jobController.AddJobs(SelectedJobs)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(_ => navigationService.GoBackTo(ViewUris.ListJobs));
        }

        private void OnSelectAllJobs()
        {
            if (Jobs != null)
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
        }

        private void OnFilterJobs()
        {
            if (State == AddJobsViewState.Results)
            {
                ShowFilter = true;
            }
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

        private List<AvailableJob> allJobs;

        [NotifyProperty]
        public bool ShowFilter
        {
            get;
            private set;
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
        public FilteredObservableCollection<AvailableJob> Jobs { get; private set; }

        [NotifyProperty]
        public ObservableCollection<Job> SelectedJobs { get; private set; }

        [NotifyProperty]
        public string ErrorDescription { get; private set; }

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

        [NotifyProperty(AlsoNotifyFor = new [] { "ShowFilter" })]
        public AddJobsViewState State { get; set; }
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

        public override string ToString()
        {
            return Job.Name;
        }
    }

    public enum AddJobsViewState
    {
        Loading,
        Results,
        NoResults,
        Error
    }
}
