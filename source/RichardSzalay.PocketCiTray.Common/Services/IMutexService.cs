using System;
using System.Threading;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IMutexService
    {
        bool WaitOne(string name);
        bool WaitOne(string name, TimeSpan timeout);
        Mutex GetOwned(string name, TimeSpan timeout);

        void ReleaseMutex(Mutex mutex);
    }
}
