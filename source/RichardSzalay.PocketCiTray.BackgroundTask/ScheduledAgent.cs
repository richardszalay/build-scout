using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Funq;
using Microsoft.Phone.Scheduler;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.BackgroundTask
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            Container container = CommonDependencyConfiguration.Configure();

            var log = new LoggingService(Logging.ApplicationLogName);

            container.Register<ILog>(log);
            container.Register<ILogManager>(log);

            if (!container.Resolve<IMutexService>().WaitOne(MutexNames.ForegroundApplication))
            {
                Debug.WriteLine("Foreground process is running, aborting agent task");
                NotifyComplete();
                return;
            }

            var jobUpdateService = container.Resolve<IJobUpdateService>();

            jobUpdateService.Complete += (s, e) => NotifyComplete();
            jobUpdateService.UpdateAll(UpdateTimeout);
        }

        private static readonly TimeSpan UpdateTimeout = TimeSpan.FromSeconds(10);
    }
}