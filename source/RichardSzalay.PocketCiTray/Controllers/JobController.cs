using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RichardSzalay.PocketCiTray.Services;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using RichardSzalay.PocketCiTray.Providers;

namespace RichardSzalay.PocketCiTray.Controllers
{
    public class JobController : IJobController
    {
        private readonly IJobRepository jobRepository;
        private readonly IJobProviderFactory jobProviderFactory;
        private readonly IApplicationTileService tileService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IMessageBoxFacade messageBoxFacade;

        public JobController(IJobRepository jobRepository, IJobProviderFactory jobProviderFactory,
            IApplicationTileService tileService, ISchedulerAccessor schedulerAccessor, 
            IMessageBoxFacade messageBoxFacade)
        {
            this.jobRepository = jobRepository;
            this.tileService = tileService;
            this.schedulerAccessor = schedulerAccessor;
            this.messageBoxFacade = messageBoxFacade;
            this.jobProviderFactory = jobProviderFactory;
        }

        public IObservable<Unit> DeleteJob(Job job)
        {
            var result = messageBoxFacade.Show(Strings.DeleteJobConfirmationMessage,
                Strings.DeleteJobConfirmationDescription, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                return Observable.ToAsync(() => DeleteJobInternal(job), schedulerAccessor.Background)()
                    .ObserveOn(schedulerAccessor.UserInterface);
            }
            else
            {
                return Observable.Empty<Unit>();
            }
        }

        public IObservable<ICollection<Job>> AddJobs(ICollection<Job> jobs)
        {
            var buildServer = jobs.First().BuildServer;

            var provider = jobProviderFactory.Get(buildServer.Provider);

            bool jobsAlreadyHaveStatuses = (provider.Features & JobProviderFeature.JobDiscoveryIncludesStatus) != 0;

            IObservable<IList<Job>> jobsWithStatuses = (jobsAlreadyHaveStatuses)
                ? Observable.Return((IList<Job>)jobs)
                : provider.UpdateAll(buildServer, jobs).ToList();

            return jobsWithStatuses
                .Select(jobRepository.AddJobs)
                .Do(_ => UpdateAllTiles())
                .Select(_ => jobs);
        }

        private void UpdateAllTiles()
        {
            var allJobs = jobRepository.GetJobs();

            tileService.UpdateAll(allJobs);
        }

        private void DeleteJobInternal(Job job)
        {
            jobRepository.DeleteJob(job);

            if (tileService.IsPinned(job))
            {
                tileService.RemoveJobTile(job);
            }

            this.UpdateAllTiles();
        }


        public ObservableCollection<Job> GetJobs()
        {
            var jobs = jobRepository.GetJobs();
            return new ObservableCollection<Job>(jobs);
        }
    }
}
