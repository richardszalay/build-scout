using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public interface ISchedulerAccessor
    {
        IScheduler UserInterface { get; }
        IScheduler Background { get; }
    }
}
