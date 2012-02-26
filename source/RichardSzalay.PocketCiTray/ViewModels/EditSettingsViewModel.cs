using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using RichardSzalay.PocketCiTray.Extensions.Extensions;
using RichardSzalay.PocketCiTray.Services;
using RichardSzalay.PocketCiTray.Infrastructure;
using System.Globalization;

namespace RichardSzalay.PocketCiTray.ViewModels
{
    public class EditSettingsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationSettings applicationSettings;

        public EditSettingsViewModel(INavigationService navigationService, ISchedulerAccessor schedulerAccessor, 
            IApplicationSettings applicationSettings)
        {
            this.navigationService = navigationService;
            this.schedulerAccessor = schedulerAccessor;
            this.applicationSettings = applicationSettings;
        }

        public override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

            NotificationDays = new ObservableCollection<string>(applicationSettings.NotificationDays
                .Select(d => dayNames[(int)d % dayNames.Length]));

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
        }

        public override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            applicationSettings.Save();
        }

        public IApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
        }

        [NotifyProperty]
        public ObservableCollection<string> NotificationDays { get; private set; }

        [NotifyProperty]
        public DateTime NotificationStart { get; private set; }        

        [NotifyProperty]
        public DateTime NotificationEnd { get; private set; }

        public UpdateInterval ForegroundUpdateInterval
        {
            get { return ForegroundUpdateOptions.First(x => x.Interval == applicationSettings.ApplicationUpdateInterval); }
            set { applicationSettings.ApplicationUpdateInterval = value.Interval; }
        }

        public UpdateInterval BackgroundUpdateInterval
        {
            get { return BackgroundUpdateOptions.First(x => x.Interval == applicationSettings.BackgroundUpdateInterval); }
            set 
            { 
                applicationSettings.BackgroundUpdateInterval = value.Interval;

                if (value.Interval == TimeSpan.Zero)
                {
                    applicationSettings.BackgroundUpdateEnabled = false;
                }
            }
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
            UpdateInterval updateInterval = obj as UpdateInterval;

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
}
