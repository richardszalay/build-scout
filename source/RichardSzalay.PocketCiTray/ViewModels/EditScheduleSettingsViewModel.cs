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
    public class EditScheduleSettingsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationSettings applicationSettings;
        private readonly IApplicationResourceFacade applicationResources;
        private readonly ISettingsApplier settingsApplier;
        private readonly IDeviceInformationService deviceInformationService;

        private SerialDisposable updateDisposable;

        private List<String> changedProperties;

        public EditScheduleSettingsViewModel(INavigationService navigationService, ISchedulerAccessor schedulerAccessor, 
            IApplicationSettings applicationSettings, IApplicationResourceFacade applicationResources,
            ISettingsApplier settingsApplier, IDeviceInformationService deviceInformationService)
        {
            this.navigationService = navigationService;
            this.schedulerAccessor = schedulerAccessor;
            this.applicationSettings = applicationSettings;
            this.applicationResources = applicationResources;
            this.settingsApplier = settingsApplier;
            this.deviceInformationService = deviceInformationService;
        }

        public override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            bool isReturningFromSelectorPage = (e.NavigationMode == NavigationMode.Back && e.IsNavigationInitiator);

            if (isReturningFromSelectorPage)
            {
                return;
            }

            changedProperties = new List<string>();

            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

            updateDisposable = new SerialDisposable();

            ShowBackgroundScheduleOptions = !deviceInformationService.IsLowEndDevice;

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

        public override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            StartLoading(Strings.UpdatingStatusMessage);

            applicationSettings.Save();

            settingsApplier.ApplyToSession(applicationSettings);

            StopLoading();
        }

        public IApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
        }

        public UpdateInterval ForegroundUpdateInterval
        {
            get { return ForegroundUpdateOptions.First(x => x.Interval == applicationSettings.ForegroundUpdateInterval); }
            set { applicationSettings.ForegroundUpdateInterval = value.Interval; }
        }

        public UpdateInterval BackgroundUpdateInterval
        {
            get { return BackgroundUpdateOptions.First(x => x.Interval == applicationSettings.BackgroundUpdateInterval); }
            set { applicationSettings.BackgroundUpdateInterval = value.Interval; }
        }

        [NotifyProperty(AlsoNotifyFor=new[] { "ForegroundUpdateInterval" })]
        public ICollection<UpdateInterval> ForegroundUpdateOptions { get; private set; }

        [NotifyProperty]
        public bool ShowBackgroundScheduleOptions { get; private set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "BackgroundUpdateInterval" })]
        public ICollection<UpdateInterval> BackgroundUpdateOptions { get; private set; }

        private UpdateInterval CreateUpdateInterval(TimeSpan timeSpan)
        {
            return new UpdateInterval(timeSpan);
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
}
