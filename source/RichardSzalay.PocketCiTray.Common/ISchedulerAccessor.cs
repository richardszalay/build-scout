using System.Reactive.Concurrency;

namespace RichardSzalay.PocketCiTray
{
    public interface ISchedulerAccessor
    {
        IScheduler UserInterface { get; }
        IScheduler Background { get; }
    }
}
