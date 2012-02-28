using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ApplicationSettings : IApplicationSettings
    {
        private readonly ISettingsService settingsService;

        private IDictionary<string, object> readOnlyValues = new Dictionary<string, object>();
        private IDictionary<string, object> writeThroughValues = new Dictionary<string, object>();

        private static readonly TimeSpan DefaultForegroundInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan DefaultBackgroundInterval = TimeSpan.FromMinutes(30);


        public ApplicationSettings(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.Refresh();
        }

        private const string ApplicationUpdateIntervalKey = "ApplicationSettings.ApplicationUpdateInterval";
        private const string BackgroundUpdateIntervalKey = "ApplicationSettings.BackgroundUpdateInterval";
        private const string BackgroundUpdateEnabledKey = "ApplicationSettings.BackgroundUpdateEnabled";
        private const string LoggingEnabledKey = "ApplicationSettings.LoggingEnabledKey";
        private const string FirstRunKey = "ApplicationSettings.FirstRun";

        private const string SuccessTileUriKey = "ApplicationSettings.SuccessTileUriKey";
        private const string FailureTileUriKey = "ApplicationSettings.FailureTileUriKey";
        private const string UnavailableTileUriKey = "ApplicationSettings.UnavailableTileUriKey";
        
        private const string NotificationPreferenceKey = "ApplicationSettings.NotificationPreference";
        private const string NotificationStartKey = "ApplicationSettings.NotificationStart";
        private const string NotificationEndKey = "ApplicationSettings.NotificationEnd";
        private const string NotificationDaysKey = "ApplicationSettings.NotificationDays";

        public TimeSpan ApplicationUpdateInterval
        {
            get { return new TimeSpan(GetValue(ApplicationUpdateIntervalKey, DefaultForegroundInterval.Ticks)); }
            set { writeThroughValues[ApplicationUpdateIntervalKey] = value.Ticks; }
        }

        public bool RunUnderLockScreen
        {
            get { return GetValue("RunUnderLockScreen", false); }
            set { writeThroughValues["RunUnderLockScreen"] = value; }
        }

        public TimeSpan BackgroundUpdateInterval
        {
            get { return new TimeSpan(GetValue(BackgroundUpdateIntervalKey, DefaultBackgroundInterval.Ticks)); }
            set { writeThroughValues[BackgroundUpdateIntervalKey] = value.Ticks; }
        }

        public bool BackgroundUpdateEnabled
        {
            get { return BackgroundUpdateInterval != TimeSpan.Zero; }
        }

        public bool FirstRun
        {
            get { return GetValue(FirstRunKey, false); }
            set { writeThroughValues[FirstRunKey] = value; }
        }

        public Uri SuccessTileUri
        {
            get { return new Uri(GetValue(FirstRunKey, "Images/Tiles/Success.png"), UriKind.Relative); }
            set { writeThroughValues[SuccessTileUriKey] = value.OriginalString; }
        }

        public Uri FailureTileUri
        {
            get { return new Uri(GetValue(FirstRunKey, "Images/Tiles/Failed.png"), UriKind.Relative); }
            set { writeThroughValues[FailureTileUriKey] = value.OriginalString; }
        }

        public Uri UnavailableTileUri
        {
            get { return new Uri(GetValue(FirstRunKey, "Images/Tiles/Unavailable.png"), UriKind.Relative); }
            set { writeThroughValues[UnavailableTileUriKey] = value.OriginalString; }
        }

        public event EventHandler Updated;

        public void Save()
        {
            settingsService.SaveSettings(writeThroughValues);

            var updated = this.Updated;

            if (updated != null)
            {
                updated(this, EventArgs.Empty);
            }
        }

        public void Refresh()
        {
            this.readOnlyValues = this.settingsService.GetSettings();
            this.writeThroughValues = new Dictionary<string, object>();
        }

        public bool LoggingEnabled
        {
            get { return GetValue(FirstRunKey, false); }
            set { writeThroughValues[LoggingEnabledKey] = value; }
        }

        private T GetValue<T>(string key, T defaultValue)
        {
            object value;

            if (writeThroughValues.TryGetValue(key, out value))
            {
                return (T)value;
            }

            if (readOnlyValues.TryGetValue(key, out value))
            {
                return (T)value;
            }

            return defaultValue;
        }


        public NotificationReason NotificationPreference
        {
            get { return (NotificationReason)GetValue(NotificationPreferenceKey, (int)NotificationReason.All); }
            set { writeThroughValues[NotificationPreferenceKey] = (int)value; }
        }


        public TimeSpan NotificationStart
        {
            get { return new TimeSpan(GetValue(NotificationStartKey, new TimeSpan(9, 0, 0).Ticks)); }
            set { writeThroughValues[NotificationStartKey] = value.Ticks; }
        }

        public TimeSpan NotificationEnd
        {
            get { return new TimeSpan(GetValue(NotificationEndKey, new TimeSpan(17, 0, 0).Ticks)); }
            set { writeThroughValues[NotificationEndKey] = value.Ticks; }
        }

        public DayOfWeek[] NotificationDays
        {
            get
            {
                int storedValue = GetValue(NotificationDaysKey, DefaultDaysOfWeek);

                return UnshrinkDaysOfWeek(storedValue);
            }
            set { writeThroughValues[NotificationDaysKey] = ShrinkDaysOfWeek(value); }
        }

        public string SuccessColorResource
        {
            get { return GetValue("SuccessColorResource", "GreenAccentBrush"); }
            set { writeThroughValues["SuccessColorResource"] = value; }
        }

        public string FailedColorResource
        {
            get { return GetValue("FailedColorResource", "RedAccentBrush"); }
            set { writeThroughValues["FailedColorResource"] = value; }
        }

        public string UnavailableColorResource
        {
            get { return GetValue("UnavailableColorResource", "GrayAccentBrush"); }
            set { writeThroughValues["UnavailableColorResource"] = value; }
        }

        public static int ShrinkDaysOfWeek(params DayOfWeek[] daysOfWeek)
        {
            return daysOfWeek.Aggregate(0, (v, d) => v | (1 << (int)d));
        }

        private static int DefaultDaysOfWeek = ShrinkDaysOfWeek(DayOfWeek.Monday, DayOfWeek.Tuesday,
            DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);

        public static DayOfWeek[] UnshrinkDaysOfWeek(int value)
        {
            var allDays = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                    DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

            return allDays
                .Where(day => (value & (1 << (int)day)) != 0)
                .ToArray();
        }
    }
}
