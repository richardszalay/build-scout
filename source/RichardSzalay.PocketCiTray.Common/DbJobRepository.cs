using System;
using System.Linq;
using System.Collections.Generic;
using RichardSzalay.PocketCiTray.Data;
using RichardSzalay.PocketCiTray.Services;
using Microsoft.Phone.Data.Linq;

namespace RichardSzalay.PocketCiTray
{
    public class DbJobRepository : IJobRepository
    {
        private readonly IJobDataContextFactory dataContextFactory;
        private readonly ICredentialEncryptor credentialEncryptor;
        private readonly IClock clock;

        public DbJobRepository(IJobDataContextFactory dataContextFactory, ICredentialEncryptor credentialEncryptor,
            IClock clock)
        {
            this.dataContextFactory = dataContextFactory;
            this.credentialEncryptor = credentialEncryptor;
            this.clock = clock;
        }

        public BuildServer AddBuildServer(BuildServer buildServer)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                var entity = BuildServerEntity.FromBuildServer(buildServer, credentialEncryptor);
                dataContext.BuildServers.InsertOnSubmit(entity);

                dataContext.SubmitChanges();

                Touch();

                return entity.ToBuildServer(credentialEncryptor);
            }
        }

        public BuildServer GetBuildServer(int buildServerId)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                return dataContext.BuildServers.First(x => x.Id == buildServerId)
                    .ToBuildServer(credentialEncryptor);
            }
        }

        public IEnumerable<Job> AddJobs(IEnumerable<Job> jobs)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                var buildServerEntity = dataContext.BuildServers.First(x => x.Id == jobs.First().BuildServer.Id);

                var entities = jobs.Select(j => JobEntity.FromJob(j)).ToList();

                buildServerEntity.Jobs.AddRange(entities);
                
                dataContext.Jobs.InsertAllOnSubmit(entities);
                dataContext.SubmitChanges();

                Touch();

                var buildServer = jobs.First().BuildServer;

                return entities.Select(j => j.ToJob(buildServer)).ToList();
            }
        }

        public ICollection<Job> GetJobs()
        {
            var buildServers = GetBuildServers()
                .ToDictionary(s => s.Id);

            using (var dataContext = dataContextFactory.Create())
            {
                return dataContext.Jobs
                    .Select(j => j.ToJob(buildServers[j.BuildServerId]))
                    .ToList()
                    .OrderBy(j => j.Name)
                    .ToList();
            }
        }

        public ICollection<Job> UpdateAll(ICollection<Job> jobs)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                var allJobs = dataContext.Jobs.ToList();

                var updatedJobs = allJobs.Join(jobs, j => j.Id, j => j.Id, (ent, j) =>
                {
                    ent.Update(j);
                    return ent;
                })
                .ToList();

                dataContext.SubmitChanges();

                Touch();

                return jobs;
            }
        }

        public ICollection<BuildServer> GetBuildServers()
        {
            return GetBuildServerEntities()
                .Select(s => s.ToBuildServer(credentialEncryptor))
                .ToList();
        }

        private ICollection<BuildServerEntity> GetBuildServerEntities()
        {
            using (var dataContext = dataContextFactory.Create())
            {
                return dataContext.BuildServers
                    .ToList();
            }
        }

        public Job GetJob(int job)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                return dataContext.Jobs
                    .Where(x => x.Id == job)
                    .Select(j => j.ToJob(GetBuildServer(j.BuildServerId)))
                    .First();
            }
        }

        public bool DeleteJob(Job job)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                dataContext.Jobs.DeleteOnSubmit(
                    dataContext.Jobs.First(j => j.Id == job.Id));

                dataContext.SubmitChanges();

                Touch();

                return true;
            }
        }

        public bool DeleteBuildServer(BuildServer buildServer)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                dataContext.BuildServers.DeleteOnSubmit(
                    dataContext.BuildServers.First(s => s.Id == buildServer.Id));

                dataContext.SubmitChanges();

                Touch();

                return true;
            }
        }

        private void Touch()
        {
            // TODO: THis still needs to be on a job service or something
            this.LastUpdateDate = clock.UtcNow;
        }

        public DateTimeOffset LastUpdateDate
        {
            get;
            private set;
        }


        public ICollection<Job> GetJobs(BuildServer buildServer)
        {
            using (var dataContext = dataContextFactory.Create())
            {
                return dataContext.Jobs
                    .Where(j => j.BuildServerId == buildServer.Id)
                    .Select(j => j.ToJob(buildServer))
                    .ToList();
            }
        }

        private const int DbVersion = 1;

        public void Initialize()
        {
            using (var dataContext = dataContextFactory.Create())
            {
                if (!dataContext.DatabaseExists())
                {
                    dataContext.CreateDatabase();

                    DatabaseSchemaUpdater dbUpdater = dataContext.CreateDatabaseSchemaUpdater();
                    dbUpdater.DatabaseSchemaVersion = DbVersion;
                    dbUpdater.Execute();
                }
            }
        }
    }
}
