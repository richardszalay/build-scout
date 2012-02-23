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
        private const string HelpUriTemplate = "/Content/Help/{0}.html";

        private INavigationService navigationService;

        public ViewHelpViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var query = e.Uri.GetQueryValues();

            if (!query.ContainsKey("key"))
            {
                navigationService.GoBack();
                return;
            }

            string helpKey = query["key"];

            this.HelpUri = new Uri(String.Format(HelpUriTemplate, helpKey));
        }

        [NotifyProperty]
        public Uri HelpUri { get; set; }
    }
}
