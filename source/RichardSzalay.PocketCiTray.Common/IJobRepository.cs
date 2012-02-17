using System;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray
{
    public interface IJobRepository
    {
        IObservable<BuildServer> AddBuildServer(BuildServer buildServer);
        IObservable<BuildServer> GetBuildServer(int buildServerId);
        IObservable<IEnumerable<Job>> AddJobs(IEnumerable<Job> jobs);
        IObservable<ICollection<Job>> GetJobs();
        IObservable<ICollection<Job>> UpdateAll(ICollection<Job> jobs);
        IObservable<ICollection<BuildServer>> GetBuildServers();
        IObservable<Job> GetJob(int job);
    }
}