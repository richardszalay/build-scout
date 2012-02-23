using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ViewHelpViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IHelpService helpService;

        public ViewHelpViewModel(INavigationService navigationService, IHelpService helpService)
        {
            this.navigationService = navigationService;
            this.helpService = helpService;
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string helpKey = GetHelpKey(e.Uri);

            if (String.IsNullOrEmpty(helpKey))
            {
                navigationService.GoBack();
            }

            this.HelpUri = helpService.GetHelpUri(helpKey);
        }

        private string GetHelpKey(Uri uri)
        {
            var query = uri.GetQueryValues();

            if (!query.ContainsKey("key"))
            {
                return null;
            }

            return query["key"];
        }

        [NotifyProperty]
        public Uri HelpUri { get; set; }
    }
}
