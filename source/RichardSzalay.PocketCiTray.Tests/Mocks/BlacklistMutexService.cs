using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RichardSzalay.PocketCiTray.Services;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Disposables;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class BlacklistMutexService : IMutexService
    {
        private Dictionary<string, Mutex> takenMutexes = new Dictionary<string, Mutex>();

        private List<string> releasedMutexes;

        public BlacklistMutexService(params string[] takenMutexes)
        {
            this.takenMutexes = takenMutexes.ToDictionary(n => n, n => new Mutex(false, n));
            this.releasedMutexes = new List<string>();
        }

        public bool WaitOne(string name)
        {
            return WaitOne(name, TimeSpan.Zero);
        }

        public bool WaitOne(string name, TimeSpan timeout)
        {
            if (this.takenMutexes.ContainsKey(name))
            {
                return false;
            }

            return true;
        }

        public IDisposable GetOwned(string name, TimeSpan timeout)
        {
            var mutex =new Mutex(false, name);

            takenMutexes.Add(name, mutex);

            return Disposable.Create(() => ReleaseMutex(mutex));
        }

        public IEnumerable<string> TakenMutexes
        {
            get { return takenMutexes.Select(kvp => kvp.Key).ToList(); }
        }

        public IEnumerable<string> ReleasedMutexes
        {
            get { return releasedMutexes; }
        }


        private void ReleaseMutex(Mutex mutex)
        {
            var key = this.takenMutexes.Where(x => x.Value == mutex)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (key == null)
            {
                throw new ArgumentException("Mutex was not obtained from this service");
            }

            takenMutexes.Remove(key);
            releasedMutexes.Add(key);

        }
    }
}
