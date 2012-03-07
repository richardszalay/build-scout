using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using RichardSzalay.PocketCiTray.Infrastructure;

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

        private void ChangeValue(string property, object value)
        {
            bool writeValue = false;

            if (writeThroughValues.ContainsKey(property))
            {
                if (!Object.Equals(writeThroughValues[property], value))
                {
                    writeValue = true;
                }
            }
            else if (readOnlyValues.ContainsKey(property))
            {
                if (!Object.Equals(readOnlyValues[property], value))
                {
                    writeValue = true;
                }
            }
            else
            {
                writeValue = true;
            }

            if (writeValue)
            {
                writeThroughValues[property] = value;
                OnNotifyPropertyChanged(property);
            }
        }

        public TimeSpan ForegroundUpdateInterval
        {
            get { return new TimeSpan(GetValue("ForegroundUpdateInterval", DefaultForegroundInterval.Ticks)); }
            set { ChangeValue("ForegroundUpdateInterval", value.Ticks); }
        }

        public bool RunUnderLockScreen
        {
            get { return GetValue("RunUnderLockScreen", false); }
            set { ChangeValue("RunUnderLockScreen", value); }
        }

        public TimeSpan BackgroundUpdateInterval
        {
            get { return new TimeSpan(GetValue("BackgroundUpdateInterval", DefaultBackgroundInterval.Ticks)); }
            set { ChangeValue("BackgroundUpdateInterval", value.Ticks); }
        }

        public bool BackgroundUpdateEnabled
        {
            get { return BackgroundUpdateInterval != TimeSpan.Zero; }
        }

        public bool FirstRun
        {
            get { return GetValue("FirstRun", true); }
            set { ChangeValue("FirstRun", value); }
        }

        public Uri SuccessTileUri
        {
            get { return new Uri(GetValue("SuccessTileUri", "Images/Tiles/Success.png"), UriKind.RelativeOrAbsolute); }
            set { ChangeValue("SuccessTileUri", value.OriginalString); }
        }

        public Uri FailureTileUri
        {
            get { return new Uri(GetValue("FailureTileUri", "Images/Tiles/Failed.png"), UriKind.RelativeOrAbsolute); }
            set { ChangeValue("FailureTileUri", value.OriginalString); }
        }

        public Uri UnavailableTileUri
        {
            get { return new Uri(GetValue("UnavailableTileUri", "Images/Tiles/Unavailable.png"), UriKind.RelativeOrAbsolute); }
            set { ChangeValue("UnavailableTileUri", value.OriginalString); }
        }

        public bool LoggingEnabled
        {
            get { return GetValue("LoggingEnabled", false); }
            set { ChangeValue("LoggingEnabled", value); }
        }

        public NotificationReason NotificationPreference
        {
            get { return (NotificationReason)GetValue("NotificationPreference", (int)NotificationReason.All); }
            set { ChangeValue("NotificationPreference", (int)value); }
        }


        public TimeSpan NotificationStart
        {
            get { return new TimeSpan(GetValue("NotificationStart", new TimeSpan(9, 0, 0).Ticks)); }
            set { ChangeValue("NotificationStart", value.Ticks); }
        }

        public TimeSpan NotificationEnd
        {
            get { return new TimeSpan(GetValue("NotificationEnd", new TimeSpan(17, 0, 0).Ticks)); }
            set { ChangeValue("NotificationEnd", value.Ticks); }
        }

        public DayOfWeek[] NotificationDays
        {
            get
            {
                int storedValue = GetValue("NotificationDays", DefaultDaysOfWeek);

                return UnshrinkDaysOfWeek(storedValue);
            }
            set { ChangeValue("NotificationDays", ShrinkDaysOfWeek(value)); }
        }

        public string SuccessColorResource
        {
            get { return GetValue("SuccessColorResource", "GreenAccentBrush"); }
            set { ChangeValue("SuccessColorResource", value); }
        }

        public string FailedColorResource
        {
            get { return GetValue("FailedColorResource", "RedAccentBrush"); }
            set { ChangeValue("FailedColorResource", value); }
        }

        public string UnavailableColorResource
        {
            get { return GetValue("UnavailableColorResource", "GrayAccentBrush"); }
            set { ChangeValue("UnavailableColorResource", value); }
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnNotifyPropertyChanged(string property)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(property));
            }
        }
    }
}
