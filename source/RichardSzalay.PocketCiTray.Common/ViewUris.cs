using System;

namespace RichardSzalay.PocketCiTray
{
    public static class ViewUris
    {
        public static Uri ListJobs
        {
            get { return ViewUri("/View/ListJobs.xaml"); }
        }

        public static Uri AddBuildServer
        {
            get { return ViewUri("/View/AddBuildServer.xaml"); }
        }

        public static Uri SelectBuildServer 
        {
            get { return ViewUri("/View/SelectBuildServer.xaml"); }
        }

        private static Uri ViewUri(string relativeUrl)
        {
            return new Uri(relativeUrl, UriKind.Relative);
        }

        public static Uri AddJobs(BuildServer buildServer)
        {
            return ViewUri(AddJobsBase.OriginalString + "?buildServerId=" + buildServer.Id.ToString());
        }

        public static Uri AddJobsBase
        {
            get { return ViewUri("/View/AddJobs.xaml"); }
        }

        public static Uri ViewJob(Job job)
        {
            return ViewUri("/View/ViewJob.xaml?jobId=" + job.Id.ToString());
        }

        public static Uri ViewJobBase
        {
            get { return ViewUri("/View/ViewJobs.xaml"); }
        }

        public static Uri Help(string key)
        {
            return ViewUri("/View/ViewHelp.xaml?key=" + key);
        }

        public static Uri EditSettings
        {
            get { return ViewUri("/View/EditSettings.xaml"); }
        }
    }
}