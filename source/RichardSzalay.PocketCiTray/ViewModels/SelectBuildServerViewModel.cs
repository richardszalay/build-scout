using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Infrastructure;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Controllers;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class SelectBuildServerViewModel : ViewModelBase
    {
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IMessageBoxFacade messageBoxFacade;
        private readonly IJobController jobController;
        private BuildServer selectedBuildServer;
        private ObservableCollection<BuildServer> buildServers;

        public SelectBuildServerViewModel(IJobRepository jobRepository, IJobController jobController, 
            IJobProviderFactory jobProviderFactory, INavigationService navigationService, 
            ISchedulerAccessor schedulerAccessor, IMessageBoxFacade messageBoxFacade)
        {
            this.jobRepository = jobRepository;
            this.navigationService = navigationService;
            this.schedulerAccessor = schedulerAccessor;
            this.messageBoxFacade = messageBoxFacade;
            this.jobController = jobController;
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AddBuildServerCommand = CreateCommand(new ObservableCommand(), OnAddNewBuildServer);

            DeleteBuildServerCommand = CreateCommand(new ObservableCommand<BuildServer>(), OnDeleteBuildServer);

            this.SelectedBuildServer = null;

            StartLoading();

            LoadBuildServers();
        }

        private void LoadBuildServers()
        {
            var buildServers = jobRepository.GetBuildServers();
            BuildServers = new ObservableCollection<BuildServer>(buildServers);
        }

        [NotifyProperty]
        public ICommand AddBuildServerCommand { get; set; }

        [NotifyProperty]
        public ICommand DeleteBuildServerCommand { get; set; }

        public BuildServer SelectedBuildServer
        {
            get { return selectedBuildServer; }
            set
            {
                selectedBuildServer = value;
                OnPropertyChanged("SelectedBuildServer");

                if (value != null)
                {
                    TransitionMode = TransitionMode.ItemDetails;
                    navigationService.Navigate(ViewUris.AddJobs(value));
                }
            }
        }

        public ObservableCollection<BuildServer> BuildServers
        {
            get { return buildServers; }
            set { buildServers = value; OnPropertyChanged("BuildServers"); }
        }

        public void OnAddNewBuildServer()
        {
            TransitionMode = TransitionMode.NewItem;
            navigationService.Navigate(ViewUris.AddBuildServer);
        }

        private void OnDeleteBuildServer(BuildServer buildServer)
        {
            var result = messageBoxFacade.Show(Strings.DeleteBuildServerConfirmationMessage,
                Strings.DeleteBuildServerConfirmationDescription, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                Observable.ToAsync(() => this.jobController.DeleteBuildServer(buildServer), schedulerAccessor.Background)()
                    .ObserveOn(schedulerAccessor.UserInterface)
                    .Subscribe(_ =>
                    {
                        if (BuildServers.Count == 1)
                        {
                            navigationService.GoBack();
                        }
                        else
                        {
                            LoadBuildServers();
                        }
                    });
            }
        }
    }
}