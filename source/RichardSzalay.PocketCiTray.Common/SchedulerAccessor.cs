using System;
using System.Reactive.Concurrency;

namespace RichardSzalay.PocketCiTray
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
            get
            {
                if (userInterface == null)
                {
                    throw new InvalidOperationException("UserInterface thread is not available");
                }

                return userInterface;
            }
        }

        public IScheduler Background
        {
            get { return background; }
        }
    }
}