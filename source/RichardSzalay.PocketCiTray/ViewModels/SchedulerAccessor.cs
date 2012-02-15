using System.Reactive.Concurrency;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class SchedulerAccessor : ISchedulerAccessor
    {
        private readonly IScheduler userInterface;
        private readonly IScheduler background;

        public SchedulerAccessor(IScheduler userInterface, IScheduler background)
        {
            this.userInterface = userInterface;
            this.background = background;
        }

        public IScheduler UserInterface
        {
            get { return userInterface; }
        }

        public IScheduler Background
        {
            get { return background; }
        }
    }
}