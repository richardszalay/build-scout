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
using System.Collections.Generic;
using System.Reactive.Linq;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class FakeJobRepository : IJobRepository
    {
        private Dictionary<int, BuildServer> buildServers = new Dictionary<int, BuildServer>();
        private Dictionary<int, Job> jobs = new Dictionary<int, Job>();

        private int buildServerId = 0;
        private int jobId = 0;

        public BuildServer AddBuildServer(BuildServer buildServer)
        {
            AddBuildServerCalls.Add(buildServer);

            buildServer.Id = buildServerId++;

            buildServers.Add(buildServer.Id, buildServer);

            return buildServer;
        }

        public BuildServer GetBuildServer(int buildServerId)
        {
            GetBuildServerCalls.Add(buildServerId);

            BuildServer server;

            if (buildServers.TryGetValue(buildServerId, out server))
            {
                return server;
            }

            throw new ArgumentException("Invalid build server id");
        }

        public IEnumerable<Job> AddJobs(IEnumerable<Job> jobs)
        {
            AddJobsCalls.Add(jobs.ToList());

            foreach (Job job in jobs)
            {
                job.Id = jobId++;

                this.jobs[job.Id] = job;
            }

            return jobs;
        }

        public ICollection<Job> GetJobs()
        {
            GetJobsCalls++;

            return jobs.Values;
        }

        public ICollection<Job> UpdateAll(ICollection<Job> jobs)
        {
            UpdateAllCalls.Add(jobs);

            foreach (Job job in jobs)
            {
                job.Id = jobId++;

                this.jobs[job.Id] = job;
            }

            return jobs;
        }

        public ICollection<BuildServer> GetBuildServers()
        {
            GetBuildServersCalls++;

            return buildServers.Values;
        }

        public Job GetJob(int jobId)
        {
            GetJobCalls.Add(jobId);

            Job job;

            if (jobs.TryGetValue(jobId, out job))
            {
                return job;
            }

            throw new ArgumentException("Invalid job id");
        }

        public bool DeleteJob(Job job)
        {
            DeleteJobCalls.Add(job);

            return jobs.Remove(job.Id);
        }

        public bool DeleteBuildServer(BuildServer buildServer)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset LastUpdateDate { get; set; }

        public List<int> GetJobCalls = new List<int>();
        public int GetJobsCalls = 0;
        public int GetBuildServersCalls = 0;
        public List<int> GetBuildServerCalls = new List<int>();
        public List<BuildServer> AddBuildServerCalls = new List<BuildServer>();
        public List<ICollection<Job>> AddJobsCalls = new List<ICollection<Job>>();
        public List<ICollection<Job>> UpdateAllCalls = new List<ICollection<Job>>();
        public List<Job> DeleteJobCalls = new List<Job>();


        public ICollection<Job> GetJobs(BuildServer buildServer)
        {
            throw new NotImplementedException();
        }


        public void Initialize()
        {
        }
    }
}
