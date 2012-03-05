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

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class EditSettingsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationSettings applicationSettings;
        private readonly IApplicationResourceFacade applicationResources;
        private readonly ISettingsApplier settingsApplier;

        private SerialDisposable updateDisposable;

        private bool isActive = false;

        private List<String> changedProperties;

        public EditSettingsViewModel(INavigationService navigationService, ISchedulerAccessor schedulerAccessor, 
            IApplicationSettings applicationSettings, IApplicationResourceFacade applicationResources,
            ISettingsApplier settingsApplier)
        {
            this.navigationService = navigationService;
            this.schedulerAccessor = schedulerAccessor;
            this.applicationSettings = applicationSettings;
            this.applicationResources = applicationResources;
            this.settingsApplier = settingsApplier;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            isActive = true;

            bool isReturningFromSelectorPage = (e.NavigationMode == NavigationMode.Back && e.IsNavigationInitiator);

            if (isReturningFromSelectorPage)
            {
                return;
            }

            changedProperties = new List<string>();

            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

            updateDisposable = new SerialDisposable();

            NotificationDays = applicationSettings.NotificationDays
                .Select(d => dayNames[(int) d%dayNames.Length])
                .ToList();

            NotificationStart = DateTime.Today + applicationSettings.NotificationStart;
            NotificationEnd = DateTime.Today + applicationSettings.NotificationEnd;

            ForegroundUpdateOptions = new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(10),
                TimeSpan.FromMinutes(15)
            }.Select(CreateUpdateInterval).ToList();

            BackgroundUpdateOptions = new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromMinutes(30),
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(6),
                TimeSpan.FromDays(1)
            }.Select(CreateUpdateInterval).ToList();

            ColourOptions = AvailableColourResourceNames
                .Select(CreateResourceColor).ToList();

            SuccessfulColor = ColourOptions.First(x => x.Key == applicationSettings.SuccessColorResource);
            FailedColor = ColourOptions.First(x => x.Key == applicationSettings.FailedColorResource);
            UnavailableColor = ColourOptions.First(x => x.Key == applicationSettings.UnavailableColorResource);

            applicationSettings.PropertyChanged += OnApplicationSettingChanged;
        }

        [NotifyProperty]
        public ResourceColor UnavailableColor { get; set; }

        [NotifyProperty]
        public ResourceColor FailedColor { get; set; }

        [NotifyProperty]
        public ResourceColor SuccessfulColor { get; set; }

        public override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        public override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (!isActive)
            {
                return;
            }

            StartLoading(Strings.UpdatingStatusMessage);

            this.ApplyChanges();
            applicationSettings.Save();

            settingsApplier.RebuildSharedResources(applicationSettings);
            settingsApplier.ApplyToSession(applicationSettings);

            StopLoading();

            isActive = false;
        }

        private void OnApplicationSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!changedProperties.Contains(e.PropertyName))
            {
                changedProperties.Add(e.PropertyName);
            }
        }

        private bool ShouldRebuildSharedResources()
        {
            return changedProperties.Contains("SuccessColorResource") ||
                changedProperties.Contains("FailedColorResource") ||
                changedProperties.Contains("UnavailableColorResource");
        }

        private void ApplyChanges()
        {
            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

            applicationSettings.NotificationStart = NotificationStart.TimeOfDay;
            applicationSettings.NotificationEnd = NotificationEnd.TimeOfDay;
            applicationSettings.NotificationDays = NotificationDays
                .Select(s => (DayOfWeek) Array.IndexOf(dayNames, s))
                .ToArray();

            applicationSettings.SuccessColorResource = SuccessfulColor.Key;
            applicationSettings.FailedColorResource = FailedColor.Key;
            applicationSettings.UnavailableColorResource = UnavailableColor.Key;
        }

        public IApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
        }

        [NotifyProperty]
        public List<ResourceColor> ColourOptions { get; set; }

        [NotifyProperty]
        public IList<string> NotificationDays { get; set; }

        [NotifyProperty]
        public DateTime NotificationStart { get; set; }

        [NotifyProperty]
        public DateTime NotificationEnd { get; set; }

        public UpdateInterval ForegroundUpdateInterval
        {
            get { return ForegroundUpdateOptions.First(x => x.Interval == applicationSettings.ApplicationUpdateInterval); }
            set { applicationSettings.ApplicationUpdateInterval = value.Interval; }
        }

        public UpdateInterval BackgroundUpdateInterval
        {
            get { return BackgroundUpdateOptions.First(x => x.Interval == applicationSettings.BackgroundUpdateInterval); }
            set { applicationSettings.BackgroundUpdateInterval = value.Interval; }
        }

        [NotifyProperty(AlsoNotifyFor=new[] { "ForegroundUpdateInterval" })]
        public ICollection<UpdateInterval> ForegroundUpdateOptions { get; private set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "BackgroundUpdateInterval" })]
        public ICollection<UpdateInterval> BackgroundUpdateOptions { get; private set; }

        public bool NotifySuccess
        {
            get { return GetNotificationFlag(NotificationReason.Fixed); }
            set { SetNotificationFlag(NotificationReason.Fixed, value); OnPropertyChanged("NotifySuccess"); }
        }

        public bool NotifyFailed
        {
            get { return GetNotificationFlag(NotificationReason.Failed); }
            set { SetNotificationFlag(NotificationReason.Failed, value); OnPropertyChanged("NotifyFailed"); }
        }

        public bool NotifyUnavailable
        {
            get { return GetNotificationFlag(NotificationReason.Unavailable); }
            set { SetNotificationFlag(NotificationReason.Unavailable, value); OnPropertyChanged("NotifyUnavailable"); }
        }

        private UpdateInterval CreateUpdateInterval(TimeSpan timeSpan)
        {
            return new UpdateInterval(timeSpan);
        }

        private ResourceColor CreateResourceColor(string resource)
        {
            return new ResourceColor(resource, Strings.ResourceManager.GetString(resource), 
                applicationResources.GetResource<SolidColorBrush>(resource));
        }

        private bool GetNotificationFlag(NotificationReason reason)
        {
            return (applicationSettings.NotificationPreference & reason) != 0;
        }

        private void SetNotificationFlag(NotificationReason reason, bool value)
        {
            if (value)
            {
                applicationSettings.NotificationPreference |= reason;
            }
            else
            {
                applicationSettings.NotificationPreference = (applicationSettings.NotificationPreference | reason) ^ reason;
            }
        }

        private static readonly string[] AvailableColourResourceNames = new[]
        {
            "PhoneAccentBrush", "MagentaAccentBrush", "PurpleAccentBrush",
            "TealAccentBrush", "LimeAccentBrush", "BrownAccentBrush", 
            "PinkAccentBrush", "MangoAccentBrush", "BlueAccentBrush", 
            "RedAccentBrush", "GreenAccentBrush", "GrayAccentBrush" 
        };
    }

    public class UpdateInterval
    {
        public UpdateInterval(TimeSpan interval)
        {
            this.Interval = interval;
            this.Display = GetDisplay(interval);
        }

        public TimeSpan Interval { get; private set; }

        public string Display { get; private set; }

        private static string GetDisplay(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
            {
                return Strings.UpdateIntervalNever;
            }
            else if (timeSpan.TotalHours < 1)
            {
                return timeSpan.Minutes == 1
                    ? Strings.UpdateIntervalMinute
                    : String.Format(Strings.UpdateIntervalMinutes, timeSpan.Minutes);
            }
            else if (timeSpan.TotalDays < 1)
            {
                return timeSpan.Hours == 1
                    ? Strings.UpdateIntervalHour
                    : String.Format(Strings.UpdateIntervalHours, timeSpan.Hours);
            }
            else
            {
                return timeSpan.Days == 1
                    ? Strings.UpdateIntervalDay
                    : String.Format(Strings.UpdateIntervalDays, timeSpan.Days);
            }
        }

        public override bool Equals(object obj)
        {
            var updateInterval = obj as UpdateInterval;

            if (updateInterval == null)
            {
                return false;
            }

            return updateInterval.Interval == this.Interval;
        }

        public override int GetHashCode()
        {
            return Interval.GetHashCode();
        }
    }

    public class ResourceColor
    {
        public ResourceColor(string key, string name, SolidColorBrush brush)
        {
            this.Key = key;
            this.Brush = brush;
            this.Name = name;
        }

        public SolidColorBrush Brush { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ResourceColor)) return false;
            return Equals((ResourceColor) obj);
        }

        public bool Equals(ResourceColor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        private enum SettingChange
        {
        }
    }
}
