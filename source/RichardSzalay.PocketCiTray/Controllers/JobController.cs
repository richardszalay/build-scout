using System;
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

namespace RichardSzalay.PocketCiTray.Controllers
{
    public class JobController : IJobController
    {
        private readonly IJobRepository jobRepository;
        private readonly IApplicationTileService tileService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IMessageBoxFacade messageBoxFacade;

        public JobController(IJobRepository jobRepository, IApplicationTileService tileService,
            ISchedulerAccessor schedulerAccessor, IMessageBoxFacade messageBoxFacade)
        {
            this.jobRepository = jobRepository;
            this.tileService = tileService;
            this.schedulerAccessor = schedulerAccessor;
            this.messageBoxFacade = messageBoxFacade;
        }

        public IObservable<Unit> DeleteJob(Job job)
        {
            var result = messageBoxFacade.Show(Strings.DeleteJobConfirmationMessage,
                Strings.DeleteJobConfirmationDescription, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                return Observable.ToAsync(() => 
                    {
                        jobRepository.DeleteJob(job);

                        if (tileService.IsPinned(job))
                        {
                            tileService.RemoveJobTile(job);
                        }

                    }, schedulerAccessor.Background)()
                    .ObserveOn(schedulerAccessor.UserInterface);
            }
            else
            {
                return Observable.Empty<Unit>();
            }
        }

        private void DeleteJobInternal(Job job)
        {
        }
    }
}
