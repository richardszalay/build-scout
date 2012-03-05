using System;
using System.Linq;
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
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Data;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace RichardSzalay.PocketCiTray.Tests.CommonTests.Services
{
    public class DbJobRepositorySpec
    {
        public class DbTest
        {
            private string filename;
            private DbJobRepository jobRepository;

            public DbJobRepository JobRepository
            {
                get
                {
                    if (jobRepository == null)
                    {
                        filename = Guid.NewGuid().ToString("N") + ".sdf";
                        string connectionString = "isostore:/" + filename;

                        jobRepository = new DbJobRepository(new Factory(filename),
                            new CredentialEncryptor(), new DateTimeOffsetClock());

                        jobRepository.Initialize();
                    }

                    return jobRepository;
                }
            }

            public void CleanUp()
            {
                IsolatedStorageFile.GetUserStoreForApplication()
                    .DeleteFile(filename);
            }

            private class Factory : IJobDataContextFactory
            {
                private readonly string filename;

                public Factory(string filename)
                {
                    this.filename = filename;
                }

                public IJobDataContext Create()
                {
                    string connectionString = "isostore:/" + filename;

                    return new JobDataContext(connectionString);
                }
            }
        }

        [TestClass]
        public class when_listing_build_servers_when_there_are_none : DbTest
        {
            private ICollection<BuildServer> buildServers;

            [ClassInitialize]
            public void because_of()
            {
                buildServers = JobRepository.GetBuildServers();
            }

            [ClassCleanup]
            public void clean_up()
            {
                base.CleanUp();
            }

            [TestMethod]
            public void it_should_return_an_empty_collection()
            {
                Assert.AreEqual(0, buildServers.Count);
            }
        }

        [TestClass]
        public class when_listing_build_servers_after_adding_one : DbTest
        {
            private ICollection<BuildServer> buildServers;

            [ClassInitialize]
            public void because_of()
            {
                JobRepository.AddBuildServer(new BuildServer()
                {
                    Name = "Test",
                    Provider = "provider",
                    Uri = new Uri("http://tempuri.org/", UriKind.Absolute),
                    Credential = null
                });

                buildServers = JobRepository.GetBuildServers();
            }

            [TestMethod]
            public void it_should_return_the_build_server_in_the_collection()
            {
                Assert.AreEqual(1, buildServers.Count);
            }

            [TestMethod]
            public void it_should_include_the_name_in_the_build_server()
            {
                Assert.AreEqual("Test", buildServers.First().Name);
            }

            [TestMethod]
            public void it_should_include_the_provider_in_the_build_server()
            {
                Assert.AreEqual("provider", buildServers.First().Provider);
            }

            [TestMethod]
            public void it_should_include_the_uri_in_the_build_server()
            {
                Assert.AreEqual(new Uri("http://tempuri.org/", UriKind.Absolute), buildServers.First().Uri);
            }

            [TestMethod]
            public void it_should_include_an_id()
            {
                Assert.AreNotEqual(0, buildServers.First().Id);
            }
        }

        [TestClass]
        public class when_saving_and_loading_a_build_server_with_credentials : DbTest
        {
            private BuildServer buildServer;

            [ClassInitialize]
            public void because_of()
            {
                var createdBuildServer = JobRepository.AddBuildServer(new BuildServer()
                {
                    Name = "Test",
                    Provider = "provider",
                    Uri = new Uri("http://tempuri.org/", UriKind.Absolute),
                    Credential = new NetworkCredential("username", "password")
                });

                buildServer = JobRepository.GetBuildServer(createdBuildServer.Id);
            }

            [TestMethod]
            public void it_should_return_the_build_server_with_the_original_username()
            {
                Assert.AreEqual("username", buildServer.Credential.UserName);
            }

            [TestMethod]
            public void it_should_return_the_build_server_with_the_original_password()
            {
                Assert.AreEqual("password", buildServer.Credential.Password);
            }
        }

        [TestClass]
        public class when_adding_jobs : DbTest
        {
            private DateTimeOffset lastBuildTime = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero);

            private ICollection<Job> jobs;
            private Job job;

            [ClassInitialize]
            public void because_of()
            {
                var createdBuildServer = JobRepository.AddBuildServer(new BuildServer()
                {
                    Name = "Test",
                    Provider = "provider",
                    Uri = new Uri("http://tempuri.org/", UriKind.Absolute),
                    Credential = new NetworkCredential("username", "password")
                });

                JobRepository.AddJobs(new Job[]
                {
                    new Job()
                    {
                        Name = "Job name",
                        NotificationPreference = NotificationReason.Failed | NotificationReason.Fixed,
                        RemoteId = "remote id",
                        WebUri = new Uri("http://tempuri.org/", UriKind.Absolute),
                        LastBuild = new Build
                        {
                            Label = "label",
                            Result = BuildResult.Success,
                            Change = BuildResultChange.Fixed,
                            Time = lastBuildTime
                        },
                        BuildServer =createdBuildServer
                    }
                });

                jobs = JobRepository.GetJobs();
                job = JobRepository.GetJob(jobs.First().Id);
            }

            [TestMethod]
            public void it_should_return_the_job_in_getjobs()
            {
                Assert.AreEqual(1, jobs.Count);
            }

            [TestMethod]
            public void it_should_return_the_job_with_the_correct_name()
            {
                Assert.AreEqual("Job name", job.Name);
                Assert.AreEqual("remote id", job.RemoteId);
                Assert.AreEqual(new Uri("http://tempuri.org/", UriKind.Absolute), job.WebUri);
                Assert.AreEqual("label", job.LastBuild.Label);
                Assert.AreEqual(BuildResult.Success, job.LastBuild.Result);
                Assert.AreEqual(lastBuildTime, job.LastBuild.Time);
            }
        }

        [TestClass]
        public class when_deleting_a_build_server : DbTest
        {
            private DateTimeOffset lastBuildTime = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero);

            private ICollection<BuildServer> buildServers;
            private ICollection<Job> jobs;

            [ClassInitialize]
            public void because_of()
            {
                var createdBuildServer = JobRepository.AddBuildServer(new BuildServer()
                {
                    Name = "Test",
                    Provider = "provider",
                    Uri = new Uri("http://tempuri.org/", UriKind.Absolute),
                    Credential = new NetworkCredential("username", "password")
                });

                JobRepository.AddJobs(new Job[]
                {
                    new Job()
                    {
                        Name = "Job name",
                        NotificationPreference = NotificationReason.Failed | NotificationReason.Fixed,
                        RemoteId = "remote id",
                        WebUri = new Uri("http://tempuri.org/", UriKind.Absolute),
                        LastBuild = new Build
                        {
                            Label = "label",
                            Result = BuildResult.Success,
                            Change = BuildResultChange.Fixed,
                            Time = lastBuildTime
                        },
                        BuildServer =createdBuildServer
                    }
                });

                JobRepository.DeleteBuildServer(createdBuildServer);

                buildServers = JobRepository.GetBuildServers();
                jobs = JobRepository.GetJobs();
            }

            [TestMethod]
            public void it_should_not_be_returned_from_getbuildservers()
            {
                Assert.AreEqual(0, buildServers.Count);
            }

            [TestMethod]
            public void it_should_delete_related_jobs()
            {
                Assert.AreEqual(0, jobs.Count);
            }
        }

        [TestClass]
        public class when_deleting_a_job : DbTest
        {
            private DateTimeOffset lastBuildTime = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero);

            private ICollection<BuildServer> buildServers;
            private ICollection<Job> jobs;

            [ClassInitialize]
            public void because_of()
            {
                var createdBuildServer = JobRepository.AddBuildServer(new BuildServer()
                {
                    Name = "Test",
                    Provider = "provider",
                    Uri = new Uri("http://tempuri.org/", UriKind.Absolute),
                    Credential = new NetworkCredential("username", "password")
                });

                var job = new Job()
                {
                    Name = "Job name",
                    NotificationPreference = NotificationReason.Failed | NotificationReason.Fixed,
                    RemoteId = "remote id",
                    WebUri = new Uri("http://tempuri.org/", UriKind.Absolute),
                    LastBuild = new Build
                    {
                        Label = "label",
                        Result = BuildResult.Success,
                        Change = BuildResultChange.Fixed,
                        Time = lastBuildTime
                    },
                    BuildServer = createdBuildServer
                };

                JobRepository.AddJobs(new Job[] { job });
                JobRepository.DeleteJob(JobRepository.GetJobs().First());

                jobs = JobRepository.GetJobs();
            }

            [TestMethod]
            public void it_should_not_be_returned_from_getjobs()
            {
                Assert.AreEqual(0, jobs.Count);
            }
        }
    }
}
