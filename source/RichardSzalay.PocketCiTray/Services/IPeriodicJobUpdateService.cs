using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IPeriodicJobUpdateService
    {
        bool CanRegisterBackgroundTask { get; }

        void Start(TimeSpan dueTime, TimeSpan period);

        void Stop();
        void UnregisterBackgroundTask();
        void RegisterBackgroundTask();
    }
}
