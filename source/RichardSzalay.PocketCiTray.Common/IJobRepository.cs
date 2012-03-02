using System;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray
{
    public interface IJobRepository
    {
        BuildServer AddBuildServer(BuildServer buildServer);
        BuildServer GetBuildServer(int buildServerId);
        IEnumerable<Job> AddJobs(IEnumerable<Job> jobs);
        ICollection<Job> GetJobs();
        ICollection<Job> UpdateAll(ICollection<Job> jobs);
        ICollection<BuildServer> GetBuildServers();
        Job GetJob(int job);
        bool DeleteJob(Job job);
        bool DeleteBuildServer(BuildServer buildServer);
        DateTimeOffset LastUpdateDate { get; }
    }
}