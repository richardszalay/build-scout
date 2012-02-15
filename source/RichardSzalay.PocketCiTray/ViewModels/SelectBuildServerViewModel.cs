using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions;
using RichardSzalay.PocketCiTray.Providers;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class SelectBuildServerViewModel : ViewModelBase
    {
        private readonly ICommand addBuildServerCommand;
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private BuildServer selectedBuildServer;

        public SelectBuildServerViewModel(IJobRepository jobRepository,
                                          IJobProviderFactory jobProviderFactory, INavigationService navigationService)
        {
            this.jobRepository = jobRepository;
            this.navigationService = navigationService;
            this.jobProviderFactory = jobProviderFactory;

            addBuildServerCommand = CreateCommand(new ObservableCommand(), OnAddNewBuildServer);
        }

        public ICommand AddBuildServerCommand
        {
            get { return addBuildServerCommand; }
        }

        public BuildServer SelectedBuildServer
        {
            get { return selectedBuildServer; }
            set
            {
                selectedBuildServer = value;
                OnPropertyChanged("SelectedBuildServer");
            }
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