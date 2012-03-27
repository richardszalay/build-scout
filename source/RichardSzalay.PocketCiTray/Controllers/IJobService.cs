using System;
using System.Reactive;
namespace RichardSzalay.PocketCiTray.Controllers
{
    public interface IJobController
    {
        IObservable<Unit> DeleteJob(Job job);
    }
}
