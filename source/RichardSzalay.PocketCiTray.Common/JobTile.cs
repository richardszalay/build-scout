using System;
using System.Collections.Generic;
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
using Microsoft.Phone.Shell;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Extensions.Extensions;

namespace RichardSzalay.PocketCiTray
{
    public abstract class JobTileBase
    {
        private Uri navigationUri;

        protected JobTileBase(Uri navigationUri)
        {
            this.navigationUri = navigationUri;
        }

        public abstract StandardTileData CreateTileData(IEnumerable<Job> jobs, IApplicationSettings settings);

        public Uri NavigationUri
        {
            get { return navigationUri; }
        }

        internal void UpdateBackground(StandardTileData data, IEnumerable<Job> filteredJobs, IApplicationSettings applicationSettings)
        {
            var results = filteredJobs
                    .GroupBy(x => x.LastBuild == null ? BuildResult.Unavailable : x.LastBuild.Result)
                    .ToDictionary(x => x.Key, x => x.Count());

            Func<BuildResult, int> statusCount = (BuildResult result) => 
                results.ContainsKey(result) ? results[result] : 0;

            int unavailableCount = statusCount(BuildResult.Unavailable);
            int failedCount = statusCount(BuildResult.Failed);
            int errorCount = unavailableCount + failedCount;

            Uri imageUri = (unavailableCount > 0) ? applicationSettings.UnavailableTileUri :
                (errorCount > 0) ? applicationSettings.FailureTileUri
                : applicationSettings.SuccessTileUri;

            data.BackgroundImage = imageUri;
            data.Count = errorCount;
        }
    }

    public class MainTile : JobTileBase
    {
        public MainTile(Uri uri)
            : base(uri)
        {
        }

        public override StandardTileData CreateTileData(IEnumerable<Job> jobs, IApplicationSettings applicationSettings)
        {
            var data = new StandardTileData
            {
                Title = CommonStrings.ApplicationTitle
            };

            UpdateBackground(data, jobs, applicationSettings);

            return data;
        }

        public static bool MatchesUri(Uri navigationUri)
        {
            return navigationUri.MakeAbsolute().AbsolutePath ==
                BaseNavigationUri.MakeAbsolute().AbsolutePath;
        }

        private static readonly Uri BaseNavigationUri = new Uri("/", UriKind.Relative);
    }

    public class JobTile : JobTileBase
    {
        private readonly int jobId;

        public JobTile(Job job)
            : base(GetNavigationUri(job))
        {
            this.jobId = job.Id;
        }

        public JobTile(Uri navigationUri)
            : base(navigationUri)
        {
            this.jobId = Int32.Parse(navigationUri.GetQueryValues()["jobId"]);
        }

        public override StandardTileData CreateTileData(IEnumerable<Job> jobs, IApplicationSettings applicationSettings)
        {
            var filteredJobs = jobs.Where(j => j.Id == jobId);
            var thisJob = jobs.FirstOrDefault();

            if (thisJob == null)
            {
                return null;
            }

            var data = new StandardTileData
            {
                Title = thisJob.Name
            };

            base.UpdateBackground(data, filteredJobs, applicationSettings);

            data.Count = 0;

            return data;
        }

        public static bool MatchesUri(Uri navigationUri)
        {
            return navigationUri.MakeAbsolute().AbsolutePath ==
                BaseNavigationUri.MakeAbsolute().AbsolutePath;
        }

        private static Uri GetNavigationUri(Job job)
        {
            return ViewUris.ViewJob(job);
        }

        private static readonly Uri BaseNavigationUri = new Uri("/View/ViewJob.xaml", UriKind.Relative);

        public int JobId { get { return jobId; } }
    }
}
