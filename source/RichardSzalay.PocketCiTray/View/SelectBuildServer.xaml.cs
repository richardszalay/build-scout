using RichardSzalay.PocketCiTray.Infrastructure;
using System.Windows;
using System.Linq;
using LinqToVisualTree;

namespace RichardSzalay.PocketCiTray.View
{
    public partial class SelectBuildServer
    {
        public SelectBuildServer()
        {
            InitializeComponent();

            ConfigureContinuumTransition((ContinuumTransition)Resources["ContinuumOutTransition"]);
            ConfigureContinuumTransition((ContinuumTransition)Resources["ContinuumInTransition"]);
        }

        private void ConfigureContinuumTransition(ContinuumTransition transition)
        {
            transition.ContinuumElement = BuildServerList;
            transition.ResolvingContinuumElement += OnResolvingContinuumElement;
        }

        void OnResolvingContinuumElement(object sender, ResolvingContinuumElementEventArgs e)
        {
            e.ContinuumElement = (FrameworkElement)e.ContinuumElement
                .Descendants()
                .FirstOrDefault(x => (string)x.GetValue(FrameworkElement.NameProperty) == "ServerName")
                ?? e.ContinuumElement;
        }
    }
}