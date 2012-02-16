using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class SelectBuildServerViewModel : ViewModelBase
    {
        private ICommand addBuildServerCommand;
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private BuildServer selectedBuildServer;
        private ObservableCollection<BuildServer> buildServers;

        public SelectBuildServerViewModel(IJobRepository jobRepository, IJobProviderFactory jobProviderFactory, 
            INavigationService navigationService, ISchedulerAccessor schedulerAccessor)
        {
            this.jobRepository = jobRepository;
            this.navigationService = navigationService;
            this.schedulerAccessor = schedulerAccessor;
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AddBuildServerCommand = CreateCommand(new ObservableCommand(), OnAddNewBuildServer);

            this.SelectedBuildServer = null;

            StartLoading();

            jobRepository.GetBuildServers()
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(result => BuildServers = new ObservableCollection<BuildServer>(result));
        }

        public ICommand AddBuildServerCommand
        {
            get { return addBuildServerCommand; }
            set { addBuildServerCommand = value; OnPropertyChanged("AddBuildServerCommand"); }
        }

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
    }
}