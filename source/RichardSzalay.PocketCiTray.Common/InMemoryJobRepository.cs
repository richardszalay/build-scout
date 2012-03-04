using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray
{
    public class InMemoryJobRepository : IJobRepository
    {
        private readonly IClock clock;
        private static int nextBuildServerId = 0;
        private static int nextJobId = 0;

        private readonly Dictionary<int, BuildServer> buildServerMap = new Dictionary<int, BuildServer>();
        private readonly Dictionary<int, Job> jobMap = new Dictionary<int, Job>();

        public InMemoryJobRepository(IClock clock)
        {
            this.clock = clock;

            this.Touch();
        }

        public BuildServer AddBuildServer(BuildServer buildServer)
        {
            Touch();

            buildServer.Id = Interlocked.Increment(ref nextBuildServerId);

            buildServerMap[buildServer.Id] = buildServer;

            return buildServer;
        }

        public BuildServer GetBuildServer(int buildServerId)
        {
            return buildServerMap[buildServerId];
        }

        public IEnumerable<Job> AddJobs(IEnumerable<Job> jobs)
        {
            Touch();

            foreach (Job job in jobs)
            {
                job.Id = Interlocked.Increment(ref nextJobId);

                jobMap[job.Id] = job;
            }

            return jobs;
        }

        public ICollection<Job> GetJobs()
        {
            return jobMap.Values.ToList();
        }

        public ICollection<Job> UpdateAll(ICollection<Job> jobs)
        {
            Touch();

            foreach (var job in jobs)
                jobMap[job.Id] = job;

            return jobs;
        }

        public ICollection<BuildServer> GetBuildServers()
        {
            return buildServerMap.Values.ToList();
        }

        public Job GetJob(int jobId)
        {
            return jobMap[jobId];
        }

        public bool DeleteJob(Job job)
        {
            Touch();
            return jobMap.Remove(job.Id);
        }

        public bool DeleteBuildServer(BuildServer buildServer)
        {
            if (buildServerMap.Remove(buildServer.Id))
            {
                foreach (Job job in jobMap.Values.ToList().Where(j => j.BuildServer.Id == buildServer.Id))
                {
                    jobMap.Remove(job.Id);
                }

                Touch();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Touch()
        {
            this.LastUpdateDate = clock.UtcNow;
        }

        public DateTimeOffset LastUpdateDate { get; private set; }


        public ICollection<Job> GetJobs(BuildServer buildServer)
        {
            return jobMap.Values
                .Where(j => j.BuildServer == buildServer)
                .ToList();
        }


        public void Initialize()
        {
        }
    }
}