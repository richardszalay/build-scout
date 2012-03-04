using System;
using System.Data.Linq;
using Microsoft.Phone.Data.Linq;
using System.Data.Linq.Mapping;

namespace RichardSzalay.PocketCiTray.Data
{
    public interface IJobDataContext : IDisposable
    {
        Table<JobEntity> Jobs { get; }
        Table<BuildServerEntity> BuildServers { get; }

        void SubmitChanges();

        bool DatabaseExists();

        void CreateDatabase();

        DatabaseSchemaUpdater CreateDatabaseSchemaUpdater();
    }

    public class JobDataContext : DataContext, IJobDataContext
    {
        public JobDataContext(string connectionString)
            : base(connectionString)
        {
        }

        // Public for auto-injection purposes
        public Table<JobEntity> jobs;
        public Table<BuildServerEntity> buildServers;

        public Table<JobEntity> Jobs { get { return jobs; } }
        public Table<BuildServerEntity> BuildServers { get { return buildServers; } }

        DatabaseSchemaUpdater IJobDataContext.CreateDatabaseSchemaUpdater()
        {
            return this.CreateDatabaseSchemaUpdater();
        }
    }
}
