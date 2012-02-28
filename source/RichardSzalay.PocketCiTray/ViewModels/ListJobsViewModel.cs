﻿using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Infrastructure;

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

        public ListJobsViewModel(INavigationService navigationService, IJobRepository jobRepository, 
            ISchedulerAccessor schedulerAccessor, IJobUpdateService jobUpdateService,
            IApplicationTileService applicationTileService, IMessageBoxFacade messageBoxFacade)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.jobUpdateService = jobUpdateService;
            this.applicationTileService = applicationTileService;
            this.messageBoxFacade = messageBoxFacade;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AddJobCommand = CreateCommand(new ObservableCommand(), OnAddJob);
            UpdateStatusesCommand = CreateCommand(new ObservableCommand(CanUpdateStatuses()), OnUpdateStatuses);
            ViewJobCommand = CreateCommand(new ObservableCommand<Job>(), OnViewJob);
            EditSettingsCommand = CreateCommand(new ObservableCommand(), OnEditSettings);

            PinJobCommand = CreateCommand(new ObservableCommand<Job>(CanPinJob), OnPinJob);
            DeleteJobCommand = CreateCommand(new ObservableCommand<Job>(), OnDeleteJob);

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Started += h, h => jobUpdateService.Started -= h)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => StartLoading(Strings.UpdatingStatusMessage)));

            Disposables.Add(Observable.FromEventPattern(h => jobUpdateService.Complete += h, h => jobUpdateService.Complete -= h)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Subscribe(_ => RefreshJobs()));

            this.RefreshJobs();
        }

        private IObservable<bool> CanUpdateStatuses()
        {
            return this.GetPropertyValues(x => x.HasJobs);
        }

        [NotifyProperty]
        public ICommand DeleteJobCommand { get; set; }

        [NotifyProperty]
        public ICommand PinJobCommand { get; set; }

        private void OnDeleteJob(Job job)
        {
            var result = messageBoxFacade.Show(Strings.DeleteJobConfirmationMessage, 
                Strings.DeleteJobConfirmationDescription, MessageBoxButton.OKCancel);

            if (result ==  MessageBoxResult.OK)
            {
                this.jobRepository.DeleteJob(job)
                    .ObserveOn(schedulerAccessor.UserInterface)
                    .Subscribe(_ => RefreshJobs());
            }
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
            StartLoading(Strings.LoadingStatusMessage);

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

        private void OnEditSettings()
        {
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
    }
}
