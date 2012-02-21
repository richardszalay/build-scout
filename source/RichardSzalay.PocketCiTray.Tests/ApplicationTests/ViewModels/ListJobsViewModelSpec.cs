using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.PocketCiTray.ViewModels;
using Funq;
using RichardSzalay.PocketCiTray.Tests.Infrastructure;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Tests.Mocks;
using System.Windows.Navigation;

namespace RichardSzalay.PocketCiTray.Tests.ApplicationTests.ViewModels
{
    public class ListJobsViewModelSpec
    {
        [TestClass]
        public class when_navigated_to
        {
            private MockJobUpdateService jobUpdateService;
            private FakeJobRepository jobRepository;

            [ClassInitialize]
            public void because_of()
            {
                Container container = TestDependencyConfiguration.Configure();

                this.jobUpdateService = new MockJobUpdateService();
                container.Register<IJobUpdateService>(jobUpdateService);

                jobRepository = new FakeJobRepository();
                container.Register<IJobRepository>(jobRepository);

                ListJobsViewModel listJobsViewModel = container.Resolve<ListJobsViewModel>();

                listJobsViewModel.OnNavigatedTo(new NavigationEventArgs(null, 
                    new Uri("/", UriKind.Relative)));
            }

            [TestMethod]
            public void it_should_refresh_jobs()
            {
                Assert.AreEqual(1, jobRepository.GetJobsCalls);
            }
        }

        [TestClass]
        public class when_jobs_finish_loading
        {
            private MockJobUpdateService jobUpdateService;
            private FakeJobRepository jobRepository;

            [ClassInitialize]
            public void because_of()
            {
                Container container = TestDependencyConfiguration.Configure();

                this.jobUpdateService = new MockJobUpdateService();
                container.Register<IJobUpdateService>(jobUpdateService);

                jobRepository = new FakeJobRepository();
                container.Register<IJobRepository>(jobRepository);

                jobRepository.AddJobs(new Job[]
                {
                    new Job
                    {
                        Name = "Test"
                    }
                });

                ListJobsViewModel listJobsViewModel = container.Resolve<ListJobsViewModel>();

                listJobsViewModel.OnNavigatedTo(new NavigationEventArgs(null,
                    new Uri("/", UriKind.Relative)));
            }

            [TestMethod]
            public void it_should_refresh_jobs()
            {
                Assert.AreEqual(1, jobRepository.GetJobsCalls);
            }
        }

        [TestClass]
        public class when_refreshing_statuses
        {
            private MockJobUpdateService jobUpdateService;
            private FakeJobRepository jobRepository;

            [ClassInitialize]
            public void because_of()
            {
                Container container = TestDependencyConfiguration.Configure();

                this.jobUpdateService = new MockJobUpdateService();
                container.Register<IJobUpdateService>(jobUpdateService);

                jobRepository = new FakeJobRepository();
                container.Register<IJobRepository>(jobRepository);

                ListJobsViewModel listJobsViewModel = container.Resolve<ListJobsViewModel>();

                listJobsViewModel.OnNavigatedTo(new NavigationEventArgs(null,
                    new Uri("/", UriKind.Relative)));

                listJobsViewModel.UpdateStatusesCommand.Execute(null);
            }

            [TestMethod]
            public void it_should_refresh_jobs()
            {
                Assert.IsTrue(jobUpdateService.UpdateRequested);
            }
        }
    }
}
