using System;
using System.Reactive;
using System.Collections.ObjectModel;

namespace RichardSzalay.PocketCiTray.Controllers
{
    public interface IJobController
    {
        IObservable<Unit> DeleteJob(Job job);

        ObservableCollection<Job> GetJobs();
    }
}
