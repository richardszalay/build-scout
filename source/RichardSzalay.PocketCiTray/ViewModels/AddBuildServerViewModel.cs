﻿using System;
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
using System.Net;

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
        private string buildServerUrl;
        private string selectedProvider;
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

            Providers = jobProviderFactory.GetProviders()
                .Select(p => new ProviderOption(p))
                .ToList();

            SelectedProvider = Providers.First();

            validateBuildServer = new SerialDisposable();
            Disposables.Add(validateBuildServer);

            AddBuildServerCommand = CreateCommand<string>(
                new ObservableCommand<string>(CanAdd), OnAddBuildServer);

            ShowAdvancedOptionsCommand = CreateCommand(new ObservableCommand(), OnShowAdvancedOptionsCommand);

            ViewHelpCommand = CreateCommand(new ObservableCommand(), OnViewHelp);

            Disposables.Add(Observable.FromEventPattern<NetworkNotificationEventArgs>(
                h => networkInterface.NetworkChanged += h,
                h => networkInterface.NetworkChanged -= h
                )
                .Select(_ => networkInterface.IsOnWifi)
                .Subscribe(OnNetworkInterfaceChanged));
        }

        [NotifyProperty]
        public string BuildServerUrl { get; set; }

        [NotifyProperty]
        public bool CanRestrictToNetwork { get; set; }

        [NotifyProperty]
        public string NetworkName { get; set; }

        [NotifyProperty]
        public ICommand AddBuildServerCommand { get; set; }
        
        [NotifyProperty]
        public ICommand ShowAdvancedOptionsCommand { get; set; }

        [NotifyProperty]
        public bool IsShowingAdvancedOptions { get; set; }

        [NotifyProperty]
        public string Username { get; set; }

        [NotifyProperty]
        public string Password { get; set; }

        [NotifyProperty]
        public ICommand ViewHelpCommand { get; set; }

        [NotifyProperty]
        public ProviderOption SelectedProvider { get; set; }

        [NotifyProperty]
        public ICollection<ProviderOption> Providers { get; set; }

        private void OnShowAdvancedOptionsCommand()
        {
            IsShowingAdvancedOptions = true;
        }

        private void OnAddBuildServer(string buildServerUrl)
        {
            IJobProvider provider = jobProviderFactory.Get(SelectedProvider.Provider);

            var credential = (!String.IsNullOrEmpty(Username))
                ? new NetworkCredential(Username, Password)
                : null;

            BuildServer buildServer = BuildServer.FromUri(provider.Name, 
                new Uri(buildServerUrl, UriKind.Absolute), credential);

            StartLoading(Strings.ValidatingBuildServerStatusMessage);

            validateBuildServer.Disposable = provider.ValidateBuildServer(buildServer)
                .Select(jobRepository.AddBuildServer)
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
            String message = (ex is WebException)
                ? WebExceptionService.GetDisplayMessage((WebException)ex)
                : ex.Message;

            messageBoxFacade.Show(message, Strings.ErrorValidatingBuildServer, MessageBoxButton.OK);
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

        public class ProviderOption
        {
            private readonly string provider;

            public ProviderOption(string provider)
            {
                this.provider = provider;
                this.DisplayName = ProviderStrings.ResourceManager.GetString(provider + "_Selector");
            }

            public string Provider { get { return provider; } }

            public string DisplayName
            {
                get;
                private set;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}