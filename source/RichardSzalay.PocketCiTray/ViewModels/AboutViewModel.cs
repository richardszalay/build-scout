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

        public AboutViewModel(IEmailComposeTaskFacade emailComposeTask)
        {
            this.emailComposeTask = emailComposeTask;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ContactAuthorCommand = CreateCommand(new ObservableCommand(), OnContactAuthor);
        }

        [NotifyProperty]
        public ICommand ContactAuthorCommand { get; set; }

        private void OnContactAuthor()
        {
            emailComposeTask.Show(AuthorEmailAddress, Strings.ApplicationTitle);
        }
    }
}
