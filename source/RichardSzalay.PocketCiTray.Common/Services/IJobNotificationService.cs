using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IJobNotificationService
    {
        void Notify(ICollection<Job> updatedJobs);
    }

    public class JobNotificationService : IJobNotificationService
    {
        private readonly IApplicationSettings applicationSettings;
        private readonly IShellToastFacade shellToastFacade;
        private readonly ResourceManager resourceManager;
        private readonly IClock clock;

        private Dictionary<BuildResultChange, string> titleResouceKeys = new Dictionary<BuildResultChange, string>
        {
            { BuildResultChange.Fixed, "JobFixed" },
            { BuildResultChange.Failed, "JobFailed" },
            { BuildResultChange.Unavailable, "JobUnavailable" }
        };        

        public JobNotificationService(IApplicationSettings applicationSettings,
            IShellToastFacade shellToastFacade, IClock clock)
        {
            this.applicationSettings = applicationSettings;
            this.resourceManager = NotificationStrings.ResourceManager;
            this.shellToastFacade = shellToastFacade;
            this.clock = clock;
        }

        public void Notify(ICollection<Job> updatedJobs)
        {
            bool jobsHaveBeenUpdated = updatedJobs.Count > 0;
            bool notificationsAreDisabled = applicationSettings.NotificationPreference == NotificationReason.None;

            if (!jobsHaveBeenUpdated || notificationsAreDisabled || !IsValidTimeForNotifications(clock.UtcNow))
            {
                return;
            }

            var changeGroups = GetValidNotifications(updatedJobs)
                .Where(j => j.LastBuild.Change != BuildResultChange.None)
                .GroupBy(j => j.LastBuild.Change)
                .ToList();

            if (changeGroups.Count != 0)
            {
                var mostRelevantChange = changeGroups
                    .OrderByDescending(g => (int)g.Key)
                    .First();

                shellToastFacade.Show(
                    GetNotificationTitle(mostRelevantChange),
                    GetNotificationContent(mostRelevantChange),
                    GetNotificationUri(mostRelevantChange, changeGroups.Count)
                    );
            }
        }

        private bool IsValidTimeForNotifications(DateTimeOffset now)
        {
            return applicationSettings.NotificationDays.Contains(now.DayOfWeek) &&
                now.TimeOfDay >= applicationSettings.NotificationStart &&
                now.TimeOfDay < applicationSettings.NotificationEnd;
        }

        private Uri GetNotificationUri(IGrouping<BuildResultChange, Job> mostRelevantChange, int changeGroupCount)
        {
            if (changeGroupCount > 1 || mostRelevantChange.Count() > 1)
            {
                return ViewUris.ListJobs;
            }
            else
            {
                return ViewUris.ViewJob(mostRelevantChange.First());
            }
        }

        private string GetNotificationTitle(IGrouping<BuildResultChange, Job> mostRelevantChange)
        {
            string titleResourceKey = titleResouceKeys[mostRelevantChange.Key];

            return resourceManager.GetString(titleResourceKey);
        }

        private string GetNotificationContent(IGrouping<BuildResultChange, Job> mostRelevantChange)
        {
            int otherJobs = mostRelevantChange.Count() - 1;

            string descriptionKey = (otherJobs == 0) ? "JobName"
                : (otherJobs == 1) ? "JobNameWithOneOther"
                : "JobNameWithOthers";

            string descriptionFormat = resourceManager.GetString(descriptionKey);

            return String.Format(descriptionFormat, mostRelevantChange.First().Name, otherJobs);
        }

        private IEnumerable<Job> GetValidNotifications(IEnumerable<Job> updatedJobs)
        {
            return updatedJobs
                .Where(j => ShouldNotifyFor(applicationSettings.NotificationPreference, j.LastBuild.Change) &&
                    ShouldNotifyFor(j.NotificationPreference, j.LastBuild.Change));
        }

        private bool ShouldNotifyFor(NotificationReason preferences, BuildResultChange change)
        {
            // FIXME: This is a bit dodgy
            return ((int)change & (int)preferences) != 0;
        }
    }
}
