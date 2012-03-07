using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;
using System.Globalization;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Xml.Linq;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class EditSettingsViewModel : ViewModelBase
    {
        private readonly IApplicationSettings applicationSettings;

        public EditSettingsViewModel(IApplicationSettings applicationSettings)
        {
            this.applicationSettings = applicationSettings;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.Links = new List<PageLink>()
            {
                new PageLink(SettingsStrings.ScheduleTitle, 
                    BuildScheduleDescription(), ViewUris.EditScheduleSettings),
                new PageLink(SettingsStrings.NotificationTitle, 
                    BuildNotificationDescription(), ViewUris.EditNotificationSettings),
                new PageLink(SettingsStrings.ColoursTitle, 
                    SettingsStrings.ColoursDescription, ViewUris.EditColourSettings),
                new PageLink(AboutStrings.AboutTitle, 
                    GetAboutDescription(), ViewUris.About),
            };
        }

        private string GetAboutDescription()
        {
            return String.Format(AboutStrings.VersionFormat, GetApplicationVersion());
        }

        private string BuildNotificationDescription()
        {
            if (applicationSettings.NotificationPreference == NotificationReason.All)
            {
                return SettingsStrings.NotificationAllChanges;
            }
            else if (applicationSettings.NotificationPreference == NotificationReason.None)
            {
                return SettingsStrings.Disabled;
            }
            else
            {
                List<String> changes = new List<string>();

                if ((applicationSettings.NotificationPreference & NotificationReason.Fixed) != 0)
                {
                    changes.Add(SettingsStrings.NotificationFixed);
                }

                if ((applicationSettings.NotificationPreference & NotificationReason.Failed) != 0)
                {
                    changes.Add(SettingsStrings.NotificationFailed);
                }

                if ((applicationSettings.NotificationPreference & NotificationReason.Unavailable) != 0)
                {
                    changes.Add(SettingsStrings.NotificationUnavailable);
                }

                return String.Join(SettingsStrings.NotificationSeparator, changes);
            }
        }

        private string BuildScheduleDescription()
        {
            string foregroundDescription = null;

            if (applicationSettings.ForegroundUpdateInterval == TimeSpan.Zero)
            {
                foregroundDescription = SettingsStrings.Disabled;
            }
            else
            {
                string interval = new UpdateInterval(applicationSettings.ForegroundUpdateInterval).Display;

                foregroundDescription = String.Format(SettingsStrings.EveryInterval, interval);
            }

            string backgroundDescription = (applicationSettings.BackgroundUpdateInterval == TimeSpan.Zero)
                ? SettingsStrings.Disabled
                : SettingsStrings.Enabled;

            return String.Format(SettingsStrings.ScheduleDescriptionFormat, foregroundDescription, backgroundDescription);
        }

        private string GetApplicationVersion()
        {
            // TODO: My eyes, the goggles do nothing
            return XDocument.Load("WMAppManifest.xml")
                .Root.Element("App").Attribute("Version").Value;
            ;
        }

        [NotifyProperty]
        public IList<PageLink> Links { get; set; }
    }
}
