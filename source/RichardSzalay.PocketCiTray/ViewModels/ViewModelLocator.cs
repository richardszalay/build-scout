﻿using System;
using System.IO.IsolatedStorage;
using System.Net.Browser;
using System.Reactive.Concurrency;
using Funq;
using RichardSzalay.PocketCiTray.Providers;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class ViewModelLocator
    {
        public ListJobsViewModel ListJobsViewModel
        {
            get { return Container.Resolve<ListJobsViewModel>(); }
        }

        public ViewJobViewModel ViewJobViewModel
        {
            get { return Container.Resolve<ViewJobViewModel>(); }
        }

        public SelectBuildServerViewModel SelectBuildServerViewModel
        {
            get { return Container.Resolve<SelectBuildServerViewModel>(); }
        }

        public AddBuildServerViewModel AddBuildServerViewModel
        {
            get { return Container.Resolve<AddBuildServerViewModel>(); }
        }

        public AddJobsViewModel AddJobsViewModel
        {
            get { return Container.Resolve<AddJobsViewModel>(); }
        }

        public ViewHelpViewModel ViewHelpViewModel
        {
            get { return Container.Resolve<ViewHelpViewModel>(); }
        }

        public Container Container { get; set; }
    }
}
