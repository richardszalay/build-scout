using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IJobUpdateService
    {
        void UpdateAll();

        event EventHandler Complete;

        void Cancel();
        event EventHandler Started;
        DateTimeOffset? LastUpdateTime { get; }
    }
}
