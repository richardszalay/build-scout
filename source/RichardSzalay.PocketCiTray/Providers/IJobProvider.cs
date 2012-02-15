using System;
using System.Collections.Generic;
using System.Linq;
using RichardSzalay.PocketCiTray.ViewModels;

namespace RichardSzalay.PocketCiTray.Providers
{
    public interface IJobProvider
    {
        IObservable<ICollection<Job>> GetJobsObservableAsync(BuildServer buildServer);
        IObservable<BuildServer> ValidateBuildServer(BuildServer buildServer);
        IObservable<ICollection<Job>> UpdateAll(BuildServer buildServer, IEnumerable<Job> jobs);
    }
}