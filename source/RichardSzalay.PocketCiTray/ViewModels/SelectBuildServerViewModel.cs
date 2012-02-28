using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Infrastructure;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class SelectBuildServerViewModel : ViewModelBase
    {
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IMessageBoxFacade messageBoxFacade;
        private BuildServer selectedBuildServer;
        private ObservableCollection<BuildServer> buildServers;

        public SelectBuildServerViewModel(IJobRepository jobRepository, IJobProviderFactory jobProviderFactory, 
            INavigationService navigationService, ISchedulerAccessor schedulerAccessor, IMessageBoxFacade messageBoxFacade)
        {
            this.jobRepository = jobRepository;
            this.navigationService = navigationService;
            this.schedulerAccessor = schedulerAccessor;
            this.messageBoxFacade = messageBoxFacade;
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
            jobRepository.GetBuildServers()
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(result => BuildServers = new ObservableCollection<BuildServer>(result));
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
            navigationService.Navigate(ViewUris.AddBuildServer);
        }

        public void OnSelectBuildServer()
        {
            navigationService.Navigate(ViewUris.AddJobs(SelectedBuildServer));
        }

        private void OnDeleteBuildServer(BuildServer buildServer)
        {
            var result = messageBoxFacade.Show(Strings.DeleteBuildServerConfirmationMessage,
                Strings.DeleteBuildServerConfirmationDescription, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.jobRepository.DeleteBuildServer(buildServer)
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