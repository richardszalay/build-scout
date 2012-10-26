using RichardSzalay.PocketCiTray.Infrastructure;
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
            InitializeComponent();

            ConfigureContinuumTransition((ContinuumTransition)Resources["ContinuumOutTransition"]);
            ConfigureContinuumTransition((ContinuumTransition)Resources["ContinuumInTransition"]);
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
                    .FirstOrDefault(x => (string)x.GetValue(FrameworkElement.NameProperty) == "JobName")
                    ?? e.ContinuumElement;
            }
        }
    }
}