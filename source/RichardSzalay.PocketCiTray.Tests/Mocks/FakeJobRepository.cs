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

        public IObservable<BuildServer> AddBuildServer(BuildServer buildServer)
        {
            AddBuildServerCalls.Add(buildServer);

            buildServer.Id = buildServerId++;

            buildServers.Add(buildServer.Id, buildServer);

            return Observable.Return(buildServer);
        }

        public IObservable<BuildServer> GetBuildServer(int buildServerId)
        {
            GetBuildServerCalls.Add(buildServerId);

            BuildServer server;

            if (buildServers.TryGetValue(buildServerId, out server))
            {
                return Observable.Return(server);
            }

            return Observable.Throw<BuildServer>(new ArgumentException("Invalid build server id"));
        }

        public IObservable<System.Collections.Generic.IEnumerable<Job>> AddJobs(System.Collections.Generic.IEnumerable<Job> jobs)
        {
            AddJobsCalls.Add(jobs.ToList());

            foreach (Job job in jobs)
            {
                job.Id = jobId++;

                this.jobs[job.Id] = job;
            }

            return Observable.Return(jobs);
        }

        public IObservable<System.Collections.Generic.ICollection<Job>> GetJobs()
        {
            GetJobsCalls++;

            return Observable.Return((ICollection<Job>)jobs.Values);
        }

        public IObservable<System.Collections.Generic.ICollection<Job>> UpdateAll(System.Collections.Generic.ICollection<Job> jobs)
        {
            UpdateAllCalls.Add(jobs);

            foreach (Job job in jobs)
            {
                job.Id = jobId++;

                this.jobs[job.Id] = job;
            }

            return Observable.Return(jobs);
        }

        public IObservable<System.Collections.Generic.ICollection<BuildServer>> GetBuildServers()
        {
            GetBuildServersCalls++;

            return Observable.Return((ICollection<BuildServer>)buildServers.Values);
        }

        public IObservable<Job> GetJob(int jobId)
        {
            GetJobCalls.Add(jobId);

            Job job;

            if (jobs.TryGetValue(jobId, out job))
            {
                return Observable.Return(job);
            }

            return Observable.Throw<Job>(new ArgumentException("Invalid job id"));
        }

        public IObservable<bool> DeleteJob(Job job)
        {
            DeleteJobCalls.Add(job);

            return Observable.Return(jobs.Remove(job.Id));
        }

        public IObservable<bool> DeleteBuildServer(BuildServer buildServer)
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
    }
}
