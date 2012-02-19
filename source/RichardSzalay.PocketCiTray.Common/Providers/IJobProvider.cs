using System;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Providers
{
    public interface IJobProvider
    {
        IObservable<ICollection<Job>> GetJobsObservableAsync(BuildServer buildServer);
        IObservable<BuildServer> ValidateBuildServer(BuildServer buildServer);
        IObservable<Job> UpdateAll(BuildServer buildServer, IEnumerable<Job> jobs);
        String Name { get; }
    }
}