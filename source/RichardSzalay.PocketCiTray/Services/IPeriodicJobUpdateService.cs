using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IPeriodicJobUpdateService
    {
        void Start(TimeSpan dueTime, TimeSpan period);

        void Stop();
        void UnregisterBackgroundTask();
        void RegisterBackgroundTask();
    }
}
