using RichardSzalay.PocketCiTray.Infrastructure;
using WP7Contrib.View.Transitions.Animation;
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using LinqToVisualTree;
using System.Windows.Controls;

namespace RichardSzalay.PocketCiTray.View
{
    public partial class ListJobs
    {
        public ListJobs()
        {
            Debug.WriteLine("[{0:hh:mm:ss.fff}] Begin ListJobs.ctor", DateTimeOffset.UtcNow);

            InitializeComponent();

            ConfigureContinuumTransition((ContinuumTransition)Resources["ContinuumOutTransition"]);
            ConfigureContinuumTransition((ContinuumTransition)Resources["ContinuumInTransition"]);

            Debug.WriteLine("[{0:hh:mm:ss.fff}] End ListJobs.ctor", DateTimeOffset.UtcNow);
        }

        private void ConfigureContinuumTransition(ContinuumTransition transition)
        {
            transition.ContinuumElement = JobsList;
            transition.ResolvingContinuumElement += OnResolvingContinuumElement;
        }

        void OnResolvingContinuumElement(object sender, ResolvingContinuumElementEventArgs e)
        {
            if (e.ContinuumElement is ListBoxItem)
            {
                e.ContinuumElement = (FrameworkElement)e.ContinuumElement
                    .Descendants()
                    .First(x => (string)x.GetValue(FrameworkElement.NameProperty) == "JobName");
            }
        }
    }
}