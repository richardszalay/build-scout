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

namespace RichardSzalay.PocketCiTray
{
    public class JobTileBase
    {

    }

    public class MainTile : JobTileBase
    {
        public IEnumerable<Job> FilterApplicableJobs(IEnumerable<Job> jobs)
        {
            return jobs;
        }
    }

    public class JobTile : JobTileBase
    {
        private readonly int jobId;

        public JobTile(int jobId)
        {
            this.jobId = jobId;
        }

        public IEnumerable<Job> FilterApplicableJobs(IEnumerable<Job> jobs)
        {
            return jobs.Where(j => j.Id == jobId);
        }
    }
}
