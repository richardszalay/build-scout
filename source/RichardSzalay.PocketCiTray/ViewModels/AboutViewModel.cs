using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;
using System.Globalization;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Xml.Linq;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private const string AuthorEmailAddress = "\"Richard Szalay\" <buildscout@richardszalay.com>";

        private readonly IEmailComposeTaskFacade emailComposeTask;
        private readonly IApplicationInformation applicationInformation;
        private readonly IApplicationMarketplaceFacade marketplaceFacade;
        private readonly INavigationService navigationService;

        public AboutViewModel(IEmailComposeTaskFacade emailComposeTask, IApplicationInformation applicationInformation,
            IApplicationMarketplaceFacade marketplaceFacade, INavigationService navigationService)
        {
            this.emailComposeTask = emailComposeTask;
            this.applicationInformation = applicationInformation;
            this.marketplaceFacade = marketplaceFacade;
            this.navigationService = navigationService;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ContactAuthorCommand = CreateCommand(new ObservableCommand(), OnContactAuthor);

            if (e.NavigationMode == NavigationMode.New)
            {
                Links = new List<CommandLink>
                {
                    new CommandLink(AboutStrings.ContactAuthor, CreateCommand(OnContactAuthor)),
                    new CommandLink(AboutStrings.CallToReview, CreateCommand(OnReview))
                };

                if (applicationInformation.IsTrialMode)
                {
                    Links.Add(new CommandLink(AboutStrings.CallToPurchase,
                        CreateCommand(new ObservableCommand(), OnPurchase)));
                }

                Links.Add(new CommandLink(AboutStrings.ViewChangeLog,
                    CreateCommand(new ObservableCommand(), OnViewChangeLog)));
            }
        }

        public override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // FIXME: Empty override to prevent command disposal
        }

        [NotifyProperty]
        public ICommand ContactAuthorCommand { get; set; }

        [NotifyProperty]
        public List<CommandLink> Links { get; set; }

        private void OnContactAuthor()
        {
            emailComposeTask.Show(AuthorEmailAddress, Strings.ApplicationTitle);
        }

        private void OnReview()
        {
            marketplaceFacade.ShowReview();
        }

        private void OnPurchase()
        {
            marketplaceFacade.ShowDetail();
        }

        private void OnViewChangeLog()
        {
            navigationService.Navigate(ViewUris.Help("ChangeLog"));
        }
    }
}
