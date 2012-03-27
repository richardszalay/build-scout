using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Infrastructure;
using System.Windows.Media;
using RichardSzalay.PocketCiTray.Controllers;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ListJobsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IJobUpdateService jobUpdateService;
        private readonly IApplicationTileService applicationTileService;
        private readonly IMessageBoxFacade messageBoxFacade;
        private readonly IApplicationSettings applicationSettings;
        private readonly IApplicationResourceFacade applicationResourceFacade;
        private readonly IJobController jobController;

        private DateTimeOffset? lastUpdateDate;

        public ListJobsViewModel(INavigationService navigationService, IJobRepository jobRepository, 
            ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService,
            IApplicationTileService applicationTileService, IMessageBoxFacade messageBoxFacade,
            IApplicationSettings applicationSettings, IApplicationResourceFacade applicationResourceFacade,
            IJobController jobController)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.jobUpdateService = jobUpdateService;
            this.applicationTileService = applicationTileService;
            this.messageBoxFacade = messageBoxFacade;
            this.applicationSettings = applicationSettings;
            this.applicationResourceFacade = applicationResourceFacade;
            this.jobController = jobController;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AddJobCommand = CreateCommand(new ObservableCommand(), OnAddJob);
            UpdateStatusesCommand = CreateCommand(new ObservableCommand(CanUpdateStatuses()), OnUpdateStatuses);
            ViewJobCommand = CreateCommand(new ObservableCommand<Job>(), OnViewJob);
            EditSettingsCommand = CreateCommand(new ObservableCommand(), OnEditSettings);

            SuccessBrush = applicationResourceFacade.GetResource<Brush>(applicationSettings.SuccessColorResource);
            FailedBrush = applicationResourceFacade.GetResource<Brush>(applicationSettings.FailedColorResource);
            UnavailableBrush = applicationResourceFacade.GetResource<Brush>(applicationSettings.UnavailableColorResource);

            PinJobCommand = CreateCommand(new ObservableCommand<Job>(CanPinJob), OnPinJob);
            DeleteJobCommand = CreateCommand(new ObservableCommand<Job>(), OnDeleteJob);

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Started += h, h => jobUpdateService.Started -= h)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => StartLoading(Strings.UpdatingStatusMessage)));

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Complete += h, h => jobUpdateService.Complete -= h)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => { StopLoading(); RefreshJobs(); }));

            if (lastUpdateDate == null || lastUpdateDate.Value < jobRepository.LastUpdateDate)
            {
                this.RefreshJobs();
            }
        }

        private IObservable<bool> CanUpdateStatuses()
        {
            var hasJobs = this.GetPropertyValues(x => x.HasJobs);

            var isLoading = this.GetPropertyValues(x => x.ProgressIndicator)
                .Select(p => p != null && p.IsVisible);

            return hasJobs.CombineLatest(isLoading, (j, l) => j && !l);
        }

        [NotifyProperty]
        public ICommand DeleteJobCommand { get; set; }

        [NotifyProperty]
        public ICommand PinJobCommand { get; set; }

        private void OnDeleteJob(Job job)
        {
            jobController.DeleteJob(job)
                .Subscribe(_ => { }, RefreshJobs);
        }

        private void OnPinJob(Job job)
        {
            applicationTileService.AddJobTile(job);
        }

        private bool CanPinJob(Job job)
        {
            return job != null && !applicationTileService.IsPinned(job);
        }

        private void RefreshJobs()
        {
            var jobs = jobRepository.GetJobs();
            Jobs = new ObservableCollection<Job>(jobs);
            lastUpdateDate = jobRepository.LastUpdateDate;
        }

        private void OnViewJob(Job job)
        {
            TransitionMode = ViewModels.TransitionMode.ItemDetails;

            navigationService.Navigate(ViewUris.ViewJob(job));
        }

        private void OnAddJob()
        {
            TransitionMode = ViewModels.TransitionMode.NewItem;

            var buildServers = jobRepository.GetBuildServers();
            var hasBuildServers = buildServers.Count > 0;
            
            Uri uri = hasBuildServers 
                ? ViewUris.SelectBuildServer 
                : ViewUris.AddBuildServer;

            navigationService.Navigate(uri);
        }

        private void OnUpdateStatuses()
        {
            jobUpdateService.UpdateAll(UpdateTimeout);
        }

        private void OnEditSettings()
        {
            TransitionMode = ViewModels.TransitionMode.UnrelatedSection;
            navigationService.Navigate(ViewUris.EditSettings);
        }

        [NotifyProperty]
        public ICommand UpdateStatusesCommand { get; set; }

        [NotifyProperty]
        public ICommand ViewJobCommand { get; set; }

        [NotifyProperty]
        public ICommand EditSettingsCommand { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "HasJobs" })]
        public ObservableCollection<Job> Jobs { get; set; }

        [NotifyProperty]
        public ICommand AddJobCommand { get; private set; }

        public bool HasJobs
        {
            get { return Jobs == null || Jobs.Count > 0; }
        }

        private static readonly TimeSpan UpdateTimeout = TimeSpan.FromSeconds(30);

        [NotifyProperty]
        public Brush SuccessBrush { get; private set; }

        [NotifyProperty]
        public Brush FailedBrush { get; private set; }

        [NotifyProperty]
        public Brush UnavailableBrush { get; private set; }
    }
}
