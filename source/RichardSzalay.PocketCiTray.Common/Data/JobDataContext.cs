using System;
using System.Data.Linq;

namespace RichardSzalay.PocketCiTray.Data
{
    internal interface IJobDataContext : IDisposable
    {
        Table<JobEntity> Jobs { get; set; }
        Table<BuildServerEntity> BuildServers { get; set; }

        void SubmitChanges();
    }

    internal class JobDataContext : DataContext, IJobDataContext
    {
        public JobDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public Table<JobEntity> Jobs { get; set; }

        public Table<BuildServerEntity> BuildServers { get; set; }
    }
}
