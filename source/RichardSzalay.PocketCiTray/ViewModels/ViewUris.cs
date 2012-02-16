using System;

namespace RichardSzalay.PocketCiTray.ViewModels
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
            return ViewUri("/View/AddJobs.xaml?buildServerId=" + buildServer.Id.ToString());
        }
    }
}