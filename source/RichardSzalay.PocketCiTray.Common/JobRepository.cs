using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace RichardSzalay.PocketCiTray
{
    public class JobRepository : IJobRepository
    {
        private static int nextBuildServerId = 0;
        private static int nextJobId = 0;

        private Dictionary<int, BuildServer> buildServerMap = new Dictionary<int, BuildServer>();
        private Dictionary<int, Job> jobMap = new Dictionary<int, Job>();
        private IScheduler scheduler;

        public JobRepository(ISchedulerAccessor schedulerAccessor)
        {
            scheduler = schedulerAccessor.Background;
        }

        public IObservable<BuildServer> AddBuildServer(BuildServer buildServer)
        {
            return Observable.ToAsync(() =>
            {
                buildServer.Id = Interlocked.Increment(ref nextBuildServerId);

                buildServerMap[buildServer.Id] = buildServer;

                return buildServer;
            }, scheduler)();
        }

        public IObservable<BuildServer> GetBuildServer(int buildServerId)
        {
            return Observable.ToAsync(() =>
            {
                return buildServerMap[buildServerId];
            }, scheduler)();
        }

        public IObservable<IEnumerable<Job>> AddJobs(IEnumerable<Job> jobs)
        {
            return Observable.ToAsync(() =>
            {
                foreach (Job job in jobs)
                {
                    job.Id = Interlocked.Increment(ref nextJobId);

                    jobMap[job.Id] = job;
                }

                return jobs;

            }, scheduler)();
        }

        public IObservable<ICollection<Job>> GetJobs()
        {
            return Observable.ToAsync(() => (ICollection<Job>)jobMap.Values.ToList(), scheduler)();
        }

        public IObservable<ICollection<Job>> UpdateAll(ICollection<Job> jobs)
        {
            return Observable.ToAsync(() =>
            {
                foreach (var job in jobs)
                    jobMap[job.Id] = job;

                return jobs;
            }, scheduler)();
        }

        public IObservable<ICollection<BuildServer>> GetBuildServers()
        {
            return Observable.ToAsync(() =>
            {
                return (ICollection<BuildServer>)buildServerMap.Values.ToList();
            }, scheduler)();
        }

        public IObservable<Job> GetJob(int jobId)
        {
            return Observable.ToAsync(() => jobMap[jobId], scheduler)();
        }
    }
}