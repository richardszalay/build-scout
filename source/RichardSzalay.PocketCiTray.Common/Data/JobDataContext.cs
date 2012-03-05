using System;
using System.Data.Linq;
using Microsoft.Phone.Data.Linq;
using System.Data.Linq.Mapping;
using RichardSzalay.PocketCiTray.Services;

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

    public class MutexJobDataContext : IJobDataContext
    {
        private static readonly TimeSpan JobDataMutexTimeout = TimeSpan.FromSeconds(10);

        private readonly IJobDataContext delegateContext;
        private IDisposable mutex;

        public MutexJobDataContext(IDisposable mutex, IJobDataContext delegateContext)
        {
            this.mutex = mutex;
            this.delegateContext = delegateContext;
        }

        public void Dispose()
        {
            if (mutex != null)
            {
                mutex.Dispose();
                mutex = null;
            }
        }

        public Table<JobEntity> Jobs { get { return delegateContext.Jobs; ; } }
        public Table<BuildServerEntity> BuildServers { get { return delegateContext.BuildServers; } }

        DatabaseSchemaUpdater IJobDataContext.CreateDatabaseSchemaUpdater()
        {
            return delegateContext.CreateDatabaseSchemaUpdater();
        }

        public static MutexJobDataContext Create(IMutexService mutexService, IJobDataContext delegateContext)
        {
            return new MutexJobDataContext(
                mutexService.GetOwned(MutexNames.JobDb, JobDataMutexTimeout),
                delegateContext);            
        }


        public void SubmitChanges()
        {
            delegateContext.SubmitChanges();
        }

        public bool DatabaseExists()
        {
            return delegateContext.DatabaseExists();
        }

        public void CreateDatabase()
        {
            delegateContext.CreateDatabase();
        }
    }

    public interface IJobDataContextFactory
    {
        IJobDataContext Create();
    }

    public class JobDataContextFactory : IJobDataContextFactory
    {
        private readonly IMutexService mutexService;

        public JobDataContextFactory(IMutexService mutexService)
        {
            this.mutexService = mutexService;
        }

        public IJobDataContext Create()
        {
            return MutexJobDataContext.Create(
                mutexService, new JobDataContext(JobDbConnectionString));
        }

        private const string JobDbConnectionString = "isostore:/jobs.sdf";
    }
}
