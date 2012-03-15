using System;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using RichardSzalay.PocketCiTray.Infrastructure;
using RichardSzalay.PocketCiTray.ViewModels;

namespace RichardSzalay.PocketCiTray.View
{
    public partial class AddJobs
    {
        public AddJobs()
        {
            InitializeComponent();

            ((ContinuumTransition)Resources["ContinuumInTransition"]).ContinuumElement = ApplicationTitle;
            ((ContinuumTransition)Resources["ContinuumOutTransition"]).ContinuumElement = ApplicationTitle;
        }
    }
}