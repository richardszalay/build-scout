using System;
using System.Globalization;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ApplicationSettings : IApplicationSettings
    {
        private readonly ISettingsFacade settings;

        private static readonly TimeSpan DefaultForegroundInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DefaultBackgroundInterval = TimeSpan.FromMinutes(15);


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
    }
}
