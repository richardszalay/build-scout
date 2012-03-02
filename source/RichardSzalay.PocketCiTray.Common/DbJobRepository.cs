using System;
using System.Linq;
using System.Collections.Generic;
using RichardSzalay.PocketCiTray.Data;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray
{
    internal class DbJobRepository : IJobRepository
    {
        private readonly Func<IJobDataContext> dataContextFactory;
        private readonly ICredentialEncryptor credentialEncryptor;
        private readonly IClock clock;

        public DbJobRepository(Func<IJobDataContext> dataContextFactory, ICredentialEncryptor credentialEncryptor,
            IClock clock)
        {
            this.dataContextFactory = dataContextFactory;
            this.credentialEncryptor = credentialEncryptor;
            this.clock = clock;
        }

        public BuildServer AddBuildServer(BuildServer buildServer)
        {
            using (var dataContext = dataContextFactory())
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
            using (var dataContext = dataContextFactory())
            {
                return dataContext.BuildServers.First(x => x.Id == buildServerId)
                    .ToBuildServer(credentialEncryptor);
            }
        }

        public IEnumerable<Job> AddJobs(IEnumerable<Job> jobs)
        {
            using (var dataContext = dataContextFactory())
            {
                var buildServer = jobs.First().BuildServer;

                var entities = jobs.Select(JobEntity.FromJob).ToList();
                
                dataContext.Jobs.InsertAllOnSubmit(entities);
                dataContext.SubmitChanges();

                Touch();

                return entities.Select(j => j.ToJob(buildServer)).ToList();
            }
        }

        public ICollection<Job> GetJobs()
        {
            var buildServers = GetBuildServers()
                .ToDictionary(s => s.Id);

            using (var dataContext = dataContextFactory())
            {
                return dataContext.Jobs.Select(j => j.ToJob(
                    buildServers[j.BuildServerId]))
                    .ToList();
            }
        }

        public ICollection<Job> UpdateAll(ICollection<Job> jobs)
        {
            using (var dataContext = dataContextFactory())
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
            using (var dataContext = dataContextFactory())
            {
                return dataContext.BuildServers
                    .Select(s => s.ToBuildServer(credentialEncryptor))
                    .ToList();
            }
        }

        public Job GetJob(int job)
        {
            using (var dataContext = dataContextFactory())
            {
                return dataContext.Jobs
                    .Where(x => x.Id == job)
                    .Select(j => j.ToJob(GetBuildServer(j.BuildServerId)))
                    .First();
            }
        }

        public bool DeleteJob(Job job)
        {
            using (var dataContext = dataContextFactory())
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
            using (var dataContext = dataContextFactory())
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
    }
}
