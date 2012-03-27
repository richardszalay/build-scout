using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;
using RichardSzalay.PocketCiTray.Controllers;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ViewJobViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IJobRepository jobRepository;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationTileService applicationTileService;
        private readonly IWebBrowserTaskFacade webBrowserTask;
        private readonly IMessageBoxFacade messageBoxFacade;
        private readonly IJobController jobController;

        public ViewJobViewModel(INavigationService navigationService, IJobRepository jobRepository, 
            ISchedulerAccessor schedulerAccessor, IApplicationTileService applicationTileService, 
            IWebBrowserTaskFacade webBrowserTask, IMessageBoxFacade messageBoxFacade,
            IJobController jobController)
        {
            this.navigationService = navigationService;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.applicationTileService = applicationTileService;
            this.webBrowserTask = webBrowserTask;
            this.messageBoxFacade = messageBoxFacade;
            this.jobController = jobController;
        }

        public Job Job { get; private set; }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey("jobId"))
            {
                navigationService.GoBack();
                return;
            }

            PinJobCommand = CreateCommand(new ObservableCommand(CanPin()), OnPin);
            DeleteJobCommand = CreateCommand(new ObservableCommand(), OnDelete);
            ViewWebUrlCommand = CreateCommand(new ObservableCommand(CanViewWebUrl()), OnViewWebUrl);

            int jobId = Int32.Parse(query["jobId"]);

            Job = jobRepository.GetJob(jobId);

            HasBuildLabel = !String.IsNullOrEmpty(Job.LastBuild.Label);
        }

        [NotifyProperty]
        public bool HasBuildLabel { get; set; }

        [NotifyProperty]
        public ICommand PinJobCommand { get; set; }

        [NotifyProperty]
        public ICommand DeleteJobCommand { get; set; }

        [NotifyProperty]
        public ICommand ViewWebUrlCommand { get; set; }

        private void OnPin()
        {
            applicationTileService.AddJobTile(Job);
        }

        private void OnDelete()
        {
            jobController.DeleteJob(Job)
                .Subscribe(_ => { }, navigationService.GoBack);
        }

        private void OnViewWebUrl()
        {
            webBrowserTask.Show(Job.WebUri);
        }

        private IObservable<bool> CanPin()
        {
            return this.GetPropertyValues(x => x.Job)
                .Where(x => x != null)
                .Select(j => !applicationTileService.IsPinned(j));
        }

        private IObservable<bool> CanViewWebUrl()
        {
            return this.GetPropertyValues(x => x.Job)
                .Where(x => x != null)
                .Select(j => j.WebUri != null)
                .StartWith(false);
        }
    }
}
