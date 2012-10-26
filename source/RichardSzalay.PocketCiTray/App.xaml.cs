using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Funq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.ViewModels;

using System.Diagnostics;
using Microsoft.Phone.Info;
using BugSense;
using System.Windows.Media;

namespace RichardSzalay.PocketCiTray
{
    public partial class App : Application
    {
        private WebBrowser fixForCapabilityDetection;

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            TiltEffect.TiltableItems.Add(typeof(MultiselectItem));
            
            BugSenseHandler.Instance.Init(this, "6726e103");

            UnhandledException += Application_UnhandledException;

            InitializeComponent();
            
            InitializePhoneApplication();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

#if DEBUG
            //WindowsPhoneTestFramework.Client.AutomationClient.Automation.Instance.Initialise();
#endif //DEBUG
        }

        private Container container;

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            container = ConfigureContainer();

            this.log = container.Resolve<ILog>();

            bootstrap = container.Resolve<Bootstrap>();
            bootstrap.Startup();

            EnableLoggingForDebug();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
            {
                container = ConfigureContainer();
            }

            this.log = container.Resolve<ILog>();

            bootstrap = container.Resolve<Bootstrap>();
            bootstrap.Continue();

            EnableLoggingForDebug();
        }

        [Conditional("DEBUG")]
        private void EnableLoggingForDebug()
        {
            container.Resolve<ILogManager>().Enable();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            bootstrap.Shutdown();
        }

        private Container ConfigureContainer()
        {
            var container = ApplicationDependencyConfiguration.Configure();

            container.Register<PhoneApplicationFrame>(l => RootFrame);

            ((ViewModelLocator)Resources["ViewModelLocator"]).Container = container;

            return container;
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            bootstrap.Shutdown();

#if DEBUG
            Debug.WriteLine("Max memory used: {0}", DeviceStatus.ApplicationPeakMemoryUsage);
#endif
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            log.Write("Navigation failed", e.Exception);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            log.Write("Unhandled exception", e.ExceptionObject);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;
        private Bootstrap bootstrap;
        private ILog log;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Background = new SolidColorBrush(Colors.Transparent);

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}