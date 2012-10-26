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

            // TODO: Move all this to a bootstrap

            var log = new ThreadSafeLoggingService(Logging.AgentLogName);

            container.Register<ILog>(log);
            container.Register<ILogManager>(log);

            if (!container.Resolve<IMutexService>().WaitOne(MutexNames.ForegroundApplication))
            {
#if DEBUG
                Debug.WriteLine("Foreground process is running, aborting agent task");
#endif
                NotifyComplete();
                return;
            }

#if !DEBUG
            if (container.Resolve<IApplicationSettings>().LoggingEnabled)
#endif
            {
                container.Resolve<ILogManager>().Enable();
            }

            var clock = container.Resolve<IClock>();
            var applicationSettings = container.Resolve<IApplicationSettings>();
            var jobUpdateService = container.Resolve<IJobUpdateService>();

            TimeSpan timeSpan = applicationSettings.BackgroundUpdateInterval;

            if (timeSpan == MinimumBackgroundUpdateInterval)
            {
                timeSpan = TimeSpan.Zero;
            }

            TimeSpan nextRun = PeriodicTaskHelper.GetNextRunTime(
                jobUpdateService.LastUpdateTime, timeSpan, clock.UtcNow);

            // TODO: Should this allow up to BackgroundUpdateInterval / 2 to better round out scheduling weirdness?
            if (nextRun == TimeSpan.Zero)
            {
                jobUpdateService.Complete += (s, e) =>
                {
                    log.Disable();
                    NotifyComplete();
                };
                        
                jobUpdateService.UpdateAll(UpdateTimeout);
            }
            else
            {
                log.Write("Next background update not due for {0}. Skipping.", timeSpan.ToString());
                log.Disable();
                NotifyComplete();
            }

#if DEBUG_AGENT
            ScheduledActionService.LaunchForTest("BuildScout.BackgroundUpdateAgent", TimeSpan.FromSeconds(10));
#endif
        }

        private static readonly TimeSpan UpdateTimeout = TimeSpan.FromSeconds(10);

        private readonly TimeSpan MinimumBackgroundUpdateInterval = TimeSpan.FromSeconds(30);
    }
}