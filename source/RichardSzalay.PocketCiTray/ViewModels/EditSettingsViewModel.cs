using System;
using System.Collections.Generic;
using System.Windows.Navigation;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;
using System.Xml.Linq;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class EditSettingsViewModel : ViewModelBase
    {
        private readonly IApplicationSettings applicationSettings;
        private readonly IDeviceInformationService deviceInformationService;

        public EditSettingsViewModel(IApplicationSettings applicationSettings, IDeviceInformationService deviceInformationService)
        {
            this.applicationSettings = applicationSettings;
            this.deviceInformationService = deviceInformationService;
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
            switch (applicationSettings.NotificationPreference)
            {
                case NotificationReason.All:
                    return SettingsStrings.NotificationAllChanges;
                case NotificationReason.None:
                    return SettingsStrings.Disabled;
                default:
                    var changes = new List<string>();

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

            if (deviceInformationService.IsLowEndDevice)
            {
                string interval = new UpdateInterval(applicationSettings.ForegroundUpdateInterval).Display;

                return String.Format(SettingsStrings.EveryInterval, interval);
            }
            else
            {
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

                return String.Format(SettingsStrings.ScheduleDescriptionFormat, foregroundDescription,
                                     backgroundDescription);
            }
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
