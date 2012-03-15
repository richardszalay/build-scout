using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RichardSzalay.PocketCiTray.Services;

namespace RichardSzalay.PocketCiTray.Extensions
{
    public static class JobExtensions
    {
        public static Job MakeUnavailable(this Job job, Exception ex, DateTimeOffset timestamp)
        {
            job.LastBuild = new Build
            {
                Time = timestamp,
                Result = BuildResult.Unavailable,
                Label = WebExceptionService.GetDisplayMessage(ex)
            };

            return job;
        }
    }
}
