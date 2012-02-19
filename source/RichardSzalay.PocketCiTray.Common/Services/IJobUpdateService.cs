using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IJobUpdateService
    {
        void UpdateAll(TimeSpan timeout);

        event EventHandler Complete;

        void Cancel();
        event EventHandler Started;
        DateTimeOffset? LastUpdateTime { get; }
    }
}
