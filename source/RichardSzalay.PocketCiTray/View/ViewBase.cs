using System.Windows.Controls;
using Microsoft.Phone.Controls;
using RichardSzalay.PocketCiTray.ViewModels;
using WP7Contrib.View.Transitions.Animation;

namespace RichardSzalay.PocketCiTray.View
{
    public class ViewBase : PhoneApplicationPage //: AnimatedBasePage
    {
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            var vm = DataContext as ViewModelBase;

            if (vm != null)
            {
                vm.OnNavigatedFrom(e);
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var vm = DataContext as ViewModelBase;

            if (vm != null)
            {
                vm.OnNavigatedTo(e);
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as ViewModelBase;

            if (vm != null)
            {
                vm.OnBackKeyPress(e);
            }

            base.OnBackKeyPress(e);
        }

    }
}
