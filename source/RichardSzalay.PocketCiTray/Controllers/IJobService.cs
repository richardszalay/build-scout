using System;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Controllers
{
    public interface IJobController
    {
        IObservable<bool> DeleteJob(Job job);

        IObservable<ICollection<Job>> AddJobs(ICollection<Job> jobs);

        ObservableCollection<Job> GetJobs();

        void DeleteBuildServer(BuildServer buildServer);
    }
}
