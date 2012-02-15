using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class JobRepository : IJobRepository
    {
        private static int nextBuildServerId = 0;
        private static int nextJobId = 0;

        private Dictionary<int, BuildServer> buildServerMap = new Dictionary<int, BuildServer>();
        private Dictionary<int, Job> jobMap = new Dictionary<int, Job>();

        public BuildServer AddBuildServer(BuildServer buildServer)
        {
            buildServer.Id = Interlocked.Increment(ref nextBuildServerId);

            buildServerMap[buildServer.Id] = buildServer;

            return buildServer;
        }

        public BuildServer GetBuildServer(int buildServerId)
        {
            return buildServerMap[buildServerId];
        }

        public void AddJobs(IEnumerable<Job> jobs)
        {
            foreach(Job job in jobs)
            {
                job.Id = Interlocked.Increment(ref nextBuildServerId);

                jobMap[job.Id] = job;
            }
        }

        public ICollection<Job> GetJobs()
        {
            return jobMap.Values.ToList();
        }

        public void UpdateAll(ICollection<Job> jobs)
        {
            foreach (var job in jobs)
                jobMap[job.Id] = job;
        }
    }
}