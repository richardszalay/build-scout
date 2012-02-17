using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Providers.Cruise;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class AddBuildServerViewModel : ViewModelBase
    {
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly SerialDisposable validateBuildServer = new SerialDisposable();
        private ICommand addBuildServerCommand;
        private string buildServerUrl = "http://localhost:8095/buildServer/cc.xml";

        public AddBuildServerViewModel(INavigationService navigationService,
                                       IJobProviderFactory jobProviderFactory, IJobRepository jobRepository,
                                       ISchedulerAccessor schedulerAccessor)
        {
            this.navigationService = navigationService;
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.schedulerAccessor = schedulerAccessor;

            Disposables.Add(validateBuildServer);

            addBuildServerCommand = CreateCommand<string>(
                new ObservableCommand<string>(CanAdd), OnAddBuildServer);
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

        public ICommand AddBuildServerCommand
        {
            get { return addBuildServerCommand; }
        }

        private void OnAddBuildServer(string buildServerUrl)
        {
            IJobProvider provider = jobProviderFactory.Get(CruiseProvider.ProviderName);

            BuildServer buildServer = BuildServer.FromUri(provider.Name, new Uri(buildServerUrl, UriKind.Absolute));

            StartLoading("checking");

            validateBuildServer.Disposable = provider.ValidateBuildServer(buildServer)
                .SelectMany(jobRepository.AddBuildServer)
                .ObserveOn(schedulerAccessor.UserInterface)
                .Finally(StopLoading)
                .Subscribe(addedBuildServer => navigationService.Navigate(ViewUris.AddJobs(addedBuildServer)),
                           OnAddBuildServerFailed);
        }

        private void OnAddBuildServerFailed(Exception ex)
        {
            //throw new NotImplementedException("AddBuildServerViewModel.OnAddBuildServerFailed", ex);
        }

        private bool CanAdd(string value)
        {
            Uri tempUri;
            return !String.IsNullOrEmpty(value) &&
                Uri.TryCreate(value, UriKind.Absolute, out tempUri);
        }
    }
}