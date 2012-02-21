﻿using System;
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

                if (tileData != null)
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
    }
}