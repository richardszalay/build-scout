using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichardSzalay.PocketCiTray.AcceptanceTest.Context;
using System.Threading;

namespace RichardSzalay.PocketCiTray.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Mutex mutex;

            ConsoleApi.Hide();

            if (!IsOnlyInstanceRunning(out mutex))
                return;

            ConsoleApi.Show();

            try
            {
                var context = new BuildServerContext();

                context.BuildServers.Add(new CruiseCompatibleBuildServerStub("buildServer"));

                context.CurrentBuildServer.Jobs.Add(new JobBuilder()
                {
                    Name = "job 1",
                    RandomLastResult = true
                });

                Console.WriteLine("Build server started at: {0}", context.CurrentBuildServer.BaseUri.AbsoluteUri);

                Console.Read();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private static bool IsOnlyInstanceRunning(out Mutex mutex)
        {
            mutex = new Mutex(false, "Global\\PocketCiTray.TestConsole");

            try
            {
                return mutex.WaitOne(5000, false);
            }
            catch (AbandonedMutexException)
            {
                // Log the fact the mutex was abandoned in another process, it will still get aquired 
                return false;
            }
        }
    }
}
