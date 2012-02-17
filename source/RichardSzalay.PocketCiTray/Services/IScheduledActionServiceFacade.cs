using System;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IScheduledActionServiceFacade
    {
        void Add(string name, string description);
        void Remove(string name);
        void LaunchForTest(string name, TimeSpan delay);
    }
}
