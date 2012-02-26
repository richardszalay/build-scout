using System;
using System.Linq;
using System.Globalization;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ApplicationSettings : IApplicationSettings
    {
        private readonly ISettingsFacade settings;

        private static readonly TimeSpan DefaultForegroundInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan DefaultBackgroundInterval = TimeSpan.FromMinutes(30);


        public ApplicationSettings(ISettingsFacade settings)
        {
            this.settings = settings;
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
            get
            {
                return settings.ContainsKey(ApplicationUpdateIntervalKey)
                           ? TimeSpan.Parse((string)settings[ApplicationUpdateIntervalKey], CultureInfo.InvariantCulture)
                           : DefaultForegroundInterval;
            }
            set { settings[ApplicationUpdateIntervalKey] = value.ToString(); }
        }

        public TimeSpan BackgroundUpdateInterval
        {
            get
            {
                return settings.ContainsKey(BackgroundUpdateIntervalKey)
                           ? TimeSpan.Parse((string)settings[BackgroundUpdateIntervalKey], CultureInfo.InvariantCulture)
                           : DefaultBackgroundInterval;
            }
            set { settings[BackgroundUpdateIntervalKey] = value.ToString(); }
        }

        public bool BackgroundUpdateEnabled
        {
            get
            {
                return settings.ContainsKey(BackgroundUpdateEnabledKey)
                           ? (bool)settings[BackgroundUpdateEnabledKey]
                           : false;
            }
            set { settings[BackgroundUpdateEnabledKey] = value; }
        }

        public bool FirstRun
        {
            get
            {
                return settings.ContainsKey(FirstRunKey)
                           ? (bool)settings[FirstRunKey]
                           : true;
            }
            set { settings[FirstRunKey] = value; }
        }

        public Uri SuccessTileUri
        {
            get
            {
                return settings.ContainsKey(SuccessTileUriKey)
                           ? new Uri((string)settings[SuccessTileUriKey], UriKind.Relative)
                           : new Uri("Images/Tiles/Success.png", UriKind.Relative);
            }
            set { settings[SuccessTileUriKey] = value.OriginalString; }
        }

        public Uri FailureTileUri
        {
            get
            {
                return settings.ContainsKey(FailureTileUriKey)
                           ? new Uri((string)settings[FailureTileUriKey], UriKind.Relative)
                           : new Uri("Images/Tiles/Failed.png", UriKind.Relative);
            }
            set { settings[FailureTileUriKey] = value.OriginalString; }
        }

        public Uri UnavailableTileUri
        {
            get
            {
                return settings.ContainsKey(UnavailableTileUriKey)
                           ? new Uri((string)settings[UnavailableTileUriKey], UriKind.Relative)
                           : new Uri("Images/Tiles/Unavailable.png", UriKind.Relative);
            }
            set { settings[UnavailableTileUriKey] = value.OriginalString; }
        }

        public event EventHandler Updated;

        public void Save()
        {
            var updated = this.Updated;

            if (updated != null)
            {
                updated(this, EventArgs.Empty);
            }
        }


        public bool LoggingEnabled
        {
            get
            {
                return GetValue<bool>(LoggingEnabledKey, false);
            }
            set { settings[LoggingEnabledKey] = value; }
        }

        private T GetValue<T>(string key, T defaultValue)
        {
            object value;

            if (settings.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return defaultValue;
        }


        public NotificationReason NotificationPreference
        {
            get
            {
                return (NotificationReason)GetValue(NotificationPreferenceKey, (int)NotificationReason.All);
            }
            set { settings[NotificationPreferenceKey] = value; }
        }


        public TimeSpan NotificationStart
        {
            get
            {
                return new TimeSpan(GetValue(NotificationStartKey, new TimeSpan(9, 0, 0).Ticks));
            }
            set
            {
                settings[NotificationStartKey] = value;
            }
        }

        public TimeSpan NotificationEnd
        {
            get
            {
                return new TimeSpan(GetValue(NotificationEndKey, new TimeSpan(17, 0, 0).Ticks));
            }
            set
            {
                settings[NotificationEndKey] = value;
            }
        }

        public DayOfWeek[] NotificationDays
        {
            get
            {
                int storedValue = GetValue(NotificationDaysKey, DefaultDaysOfWeek);

                return UnshrinkDaysOfWeek(storedValue);
            }
            set
            {
                settings[NotificationDaysKey] = ShrinkDaysOfWeek(value);
            }
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
