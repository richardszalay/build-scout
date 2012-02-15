using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public interface IJobRepository
    {
        BuildServer AddBuildServer(BuildServer buildServer);
        BuildServer GetBuildServer(int buildServerId);
        void AddJobs(IEnumerable<Job> jobs);
        ICollection<Job> GetJobs();
        void UpdateAll(ICollection<Job> jobs);
    }
}