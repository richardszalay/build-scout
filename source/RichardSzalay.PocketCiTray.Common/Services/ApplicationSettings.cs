using System;
using System.Globalization;

namespace RichardSzalay.PocketCiTray.Services
{
    public class ApplicationSettings : IApplicationSettings
    {
        private readonly ISettingsFacade settings;

        public ApplicationSettings(ISettingsFacade settings)
        {
            this.settings = settings;
        }

        private const string ApplicationUpdateIntervalKey = "ApplicationSettings.ApplicationUpdateInterval";
        private const string BackgroundUpdateIntervalKey = "ApplicationSettings.BackgroundUpdateInterval";

        public TimeSpan ApplicationUpdateInterval
        {
            get
            {
                return settings.ContainsKey(ApplicationUpdateIntervalKey)
                           ? TimeSpan.Parse((string)settings[ApplicationUpdateIntervalKey], CultureInfo.InvariantCulture)
                           : TimeSpan.Zero;
            }
            set { settings[ApplicationUpdateIntervalKey] = value.ToString(); }
        }

        public TimeSpan BackgroundUpdateInterval
        {
            get
            {
                return settings.ContainsKey(BackgroundUpdateIntervalKey)
                           ? TimeSpan.Parse((string) settings[BackgroundUpdateIntervalKey], CultureInfo.InvariantCulture)
                           : TimeSpan.Zero;
            }
            set { settings[BackgroundUpdateIntervalKey] = value.ToString(); }
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
    }
}
