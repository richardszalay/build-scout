using System;
using System.Linq;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ApplicationTileService : IApplicationTileService
    {
        private readonly IApplicationSettings applicationSettings;
        private readonly IShellTileService shellTileService;

        public ApplicationTileService(IApplicationSettings applicationSettings, IShellTileService shellTileService)
        {
            this.applicationSettings = applicationSettings;
            this.shellTileService = shellTileService;
        }

        public void AddJobTile(Job job)
        {
            JobTile tile = new JobTile(job);

            shellTileService.Create(tile.NavigationUri, 
                tile.CreateTileData(new Job[] { job }, applicationSettings));
        }

        public void UpdateAll(ICollection<Job> jobs)
        {
            foreach(JobTileBase tile in this.GetAllTiles())
            {
                var tileData = tile.CreateTileData(jobs, applicationSettings);

                bool isOrphaned = (tileData == null);

                if (isOrphaned)
                {
                    shellTileService.Remove(tile.NavigationUri);
                }
                else
                {
                    shellTileService.Update(tile.NavigationUri, tileData);
                }
            }
        }

        private IEnumerable<JobTileBase> GetAllTiles()
        {
            return shellTileService.GetUris()
                .Select(CreateTileFromUri)
                .Where(t => t != null);
        }

        private JobTileBase CreateTileFromUri(Uri uri)
        {
            if (JobTile.MatchesUri(uri))
            {
                return new JobTile(uri);
            }
            else if (MainTile.MatchesUri(uri))
            {
                return new MainTile(uri);
            }

            return null;
        }

        public bool IsPinned(Job job)
        {
            return GetAllTiles().Any(tile =>
                {
                    JobTile jobTile = tile as JobTile;

                    return jobTile != null && jobTile.JobId == job.Id;
                });
        }


        public void RemoveJobTile(Job job)
        {
            var tile = GetAllTiles().OfType<JobTile>()
                .FirstOrDefault(t => t.JobId == job.Id);

            if (tile != null)
            {
                shellTileService.Remove(tile.NavigationUri);
            }
        }
    }
}
