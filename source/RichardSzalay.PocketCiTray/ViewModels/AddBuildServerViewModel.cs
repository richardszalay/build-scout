using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Phone.Net.NetworkInformation;
using RichardSzalay.PocketCiTray.Infrastructure;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class AddBuildServerViewModel : ViewModelBase
    {
        private const string AddServerHelpKey = "AddServer";

        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private SerialDisposable validateBuildServer;
        private readonly IMessageBoxFacade messageBoxFacade;
        private readonly INetworkInterfaceFacade networkInterface;

        private ICommand addBuildServerCommand;
        private string buildServerUrl = "http://guest@teamcity.codebetter.com";
        private string selectedProvider = "teamcity6";
        private ICollection<string> providers;

        private bool canRestrictToNetwork = false;
        private string networkName;

        public AddBuildServerViewModel(INavigationService navigationService,
                                       IJobProviderFactory jobProviderFactory, IJobRepository jobRepository,
                                       ISchedulerAccessor schedulerAccessor, IMessageBoxFacade messageBoxFacade,
                                       INetworkInterfaceFacade networkInterface)
        {
            this.navigationService = navigationService;
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;
            this.messageBoxFacade = messageBoxFacade;
            this.networkInterface = networkInterface;
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Providers = jobProviderFactory.GetProviders();
            SelectedProvider = Providers.First();

            validateBuildServer = new SerialDisposable();
            Disposables.Add(validateBuildServer);

            AddBuildServerCommand = CreateCommand<string>(
                new ObservableCommand<string>(CanAdd), OnAddBuildServer);

            ViewHelpCommand = CreateCommand(new ObservableCommand(), OnViewHelp);

            Disposables.Add(Observable.FromEventPattern<NetworkNotificationEventArgs>(
                h => networkInterface.NetworkChanged += h,
                h => networkInterface.NetworkChanged -= h
                )
                .Select(_ => networkInterface.IsOnWifi)
                .Subscribe(OnNetworkInterfaceChanged));
        }

        public string BuildServerUrl
        {
            get { return buildServerUrl; }
            set
            {
                buildServerUrl = value;
                OnPropertyChanged("BuildServerUrl");
            }
        }

        public bool CanRestrictToNetwork
        {
            get { return canRestrictToNetwork; }
            set { canRestrictToNetwork = value; OnPropertyChanged("CanRestrictToNetwork"); }
        }

        public string NetworkName
        {
            get { return networkName; }
            set { networkName = value; OnPropertyChanged("NetworkName"); }
        }

        public ICommand AddBuildServerCommand
        {
            get { return addBuildServerCommand; }
            set { addBuildServerCommand = value; OnPropertyChanged("AddBuildServerCommand"); }
        }

        [NotifyProperty]
        public ICommand ViewHelpCommand { get; set; }

        public string SelectedProvider
        {
            get { return selectedProvider; }
            set { selectedProvider = value; OnPropertyChanged("SelectedProvider"); }
        }

        public ICollection<string> Providers
        {
            get { return providers; }
            private set { providers = value; OnPropertyChanged("Providers"); }
        }

        private void OnAddBuildServer(string buildServerUrl)
        {
            IJobProvider provider = jobProviderFactory.Get(SelectedProvider);

            BuildServer buildServer = BuildServer.FromUri(provider.Name, new Uri(buildServerUrl, UriKind.Absolute));

            StartLoading(Strings.ValidatingBuildServerStatusMessage);

            validateBuildServer.Disposable = provider.ValidateBuildServer(buildServer)
                .SelectMany(jobRepository.AddBuildServer)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(OnAddBuildSucceeded, OnAddBuildServerFailed);
        }

        private void OnViewHelp()
        {
            navigationService.Navigate(ViewUris.Help(AddServerHelpKey));
        }

        private void OnAddBuildSucceeded(BuildServer addedBuildServer)
        {
            navigationService.Navigate(ViewUris.AddJobs(addedBuildServer));
        }

        private void OnAddBuildServerFailed(Exception ex)
        {
            messageBoxFacade.Show(ex.Message, Strings.ErrorValidatingBuildServer, MessageBoxButton.OK);
        }

        private bool CanAdd(string value)
        {
            Uri tempUri;
            return !String.IsNullOrEmpty(value) &&
                Uri.TryCreate(value, UriKind.Absolute, out tempUri);
        }

        private void OnNetworkInterfaceChanged(bool isNetworkAvailable)
        {
            CanRestrictToNetwork = isNetworkAvailable && networkInterface.IsOnWifi;

            if (CanRestrictToNetwork)
            {
                NetworkName = networkInterface.NetworkName;
            }
        }
    }
}