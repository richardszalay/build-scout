using System;
using System.Threading;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Services
{
    public class MutexService : IMutexService
    {
        public bool WaitOne(string name)
        {
            return WaitOne(name, TimeSpan.FromMilliseconds(100));
        }

        public bool WaitOne(string name, TimeSpan timeout)
        {
            Mutex mutex = new Mutex(false, GetGlobalMutexName(name));

            if (mutex.WaitOne(5))
            {
                mutex.ReleaseMutex();
                return true;
            }

            return false;
        }

        public Mutex GetOwned(string name, TimeSpan timeout)
        {
            Mutex mutex = new Mutex(false, GetGlobalMutexName(name));

            if (mutex.WaitOne(timeout))
                return mutex;

            return null;
        }

        private string GetGlobalMutexName(string name)
        {
            return "Global\\" + name;
        }

        public void ReleaseMutex(Mutex mutex)
        {
            mutex.ReleaseMutex();
        }
    }
}