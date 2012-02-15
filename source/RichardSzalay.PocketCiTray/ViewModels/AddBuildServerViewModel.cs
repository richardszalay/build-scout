using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Providers.Cruise;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class AddBuildServerViewModel : ViewModelBase
    {
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IJobRepository jobRepository;
        private readonly INavigationService navigationService;
        private readonly IScheduler uiScheduler;
        private readonly SerialDisposable validateBuildServer = new SerialDisposable();
        private ICommand addBuildServerCommand;
        private string buildServerUrl = "http://localhost:8095/buildServer/cc.xml";

        public AddBuildServerViewModel(INavigationService navigationService,
                                       IJobProviderFactory jobProviderFactory, IJobRepository jobRepository,
                                       IScheduler uiScheduler)
        {
            this.navigationService = navigationService;
            this.jobProviderFactory = jobProviderFactory;
            this.jobRepository = jobRepository;
            this.uiScheduler = uiScheduler;

            Disposables.Add(validateBuildServer);

            addBuildServerCommand = CreateCommand(
                new ObservableCommand(CanAdd()), OnAddBuildServer);
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

        private void OnAddBuildServer()
        {
            IJobProvider provider = jobProviderFactory.Get(CruiseProvider.ProviderName);

            BuildServer buildServer = BuildServer.FromUri(new Uri(buildServerUrl, UriKind.Absolute));

            validateBuildServer.Disposable = provider.ValidateBuildServer(buildServer)
                .Select(jobRepository.AddBuildServer)
                .ObserveOn(uiScheduler)
                .Subscribe(addedBuildServer => navigationService.Navigate(ViewUris.AddJobs(addedBuildServer)),
                           OnAddBuildServerFailed);
        }

        private void OnAddBuildServerFailed(Exception ex)
        {
            //throw new NotImplementedException("AddBuildServerViewModel.OnAddBuildServerFailed", ex);
        }

        private IObservable<bool> CanAdd()
        {
            Uri tempUri;
            return this.GetPropertyValues(x => x.BuildServerUrl)
                .Select(s => s.Length > 0 && Uri.TryCreate(s, UriKind.Absolute, out tempUri))
                .StartWith(false);
        }
    }
}