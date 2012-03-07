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
    public class EditNotificationSettingsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationSettings applicationSettings;
        private readonly IApplicationResourceFacade applicationResources;
        private readonly ISettingsApplier settingsApplier;

        private SerialDisposable updateDisposable;

        private List<String> changedProperties;

        public EditNotificationSettingsViewModel(INavigationService navigationService, ISchedulerAccessor schedulerAccessor, 
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
        }

        public override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        public override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            StartLoading(Strings.UpdatingStatusMessage);

            this.ApplyChanges();
            applicationSettings.Save();

            settingsApplier.ApplyToSession(applicationSettings);

            StopLoading();
        }

        private void ApplyChanges()
        {
            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;

            applicationSettings.NotificationStart = NotificationStart.TimeOfDay;
            applicationSettings.NotificationEnd = NotificationEnd.TimeOfDay;
            applicationSettings.NotificationDays = NotificationDays
                .Select(s => (DayOfWeek) Array.IndexOf(dayNames, s))
                .ToArray();
        }

        public IApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
        }

        [NotifyProperty]
        public IList<string> NotificationDays { get; set; }

        [NotifyProperty]
        public DateTime NotificationStart { get; set; }

        [NotifyProperty]
        public DateTime NotificationEnd { get; set; }

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
}
