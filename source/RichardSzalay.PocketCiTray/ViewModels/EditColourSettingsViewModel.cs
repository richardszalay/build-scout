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
    public class EditColourSettingsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ISchedulerAccessor schedulerAccessor;
        private readonly IApplicationSettings applicationSettings;
        private readonly IApplicationResourceFacade applicationResources;
        private readonly ISettingsApplier settingsApplier;
        private readonly IDeviceInformationService deviceInformationService;

        private bool isActive = false;

        private List<String> changedProperties;

        public EditColourSettingsViewModel(INavigationService navigationService, ISchedulerAccessor schedulerAccessor, 
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

            isActive = true;

            changedProperties = new List<string>();

            bool isReturningFromSelectorPage = (e.NavigationMode == NavigationMode.Back && e.IsNavigationInitiator);

            if (isReturningFromSelectorPage)
            {
                return;
            }

            ColourOptions = AvailableColourResourceNames
                .Select(CreateResourceColor).ToList();

            SuccessfulColor = ColourOptions.First(x => x.Key == applicationSettings.SuccessColorResource);
            FailedColor = ColourOptions.First(x => x.Key == applicationSettings.FailedColorResource);
            UnavailableColor = ColourOptions.First(x => x.Key == applicationSettings.UnavailableColorResource);

            UseColoredTiles = applicationSettings.UseColoredTiles;

            applicationSettings.PropertyChanged += OnApplicationSettingChanged;
        }

        [NotifyProperty]
        public ResourceColor UnavailableColor { get; set; }

        [NotifyProperty]
        public ResourceColor FailedColor { get; set; }

        [NotifyProperty]
        public ResourceColor SuccessfulColor { get; set; }

        [NotifyProperty]
        public bool UseColoredTiles { get; set; }

        [NotifyProperty]
        public bool CanUseColoredTiles
        {
            get { return !deviceInformationService.IsLowEndDevice; }
        }

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

            if (!deviceInformationService.IsLowEndDevice)
            {
                settingsApplier.RebuildSharedResources(applicationSettings);
            }

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
            applicationSettings.SuccessColorResource = SuccessfulColor.Key;
            applicationSettings.FailedColorResource = FailedColor.Key;
            applicationSettings.UnavailableColorResource = UnavailableColor.Key;
            applicationSettings.UseColoredTiles = this.UseColoredTiles;
        }

        public IApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
        }

        [NotifyProperty]
        public List<ResourceColor> ColourOptions { get; set; }

        private ResourceColor CreateResourceColor(string resource)
        {
            return new ResourceColor(resource, Strings.ResourceManager.GetString(resource), 
                applicationResources.GetResource<SolidColorBrush>(resource));
        }

        private static readonly string[] AvailableColourResourceNames = new[]
        {
            "PhoneAccentBrush", "MagentaAccentBrush", "PurpleAccentBrush",
            "TealAccentBrush", "LimeAccentBrush", "BrownAccentBrush", 
            "PinkAccentBrush", "MangoAccentBrush", "BlueAccentBrush", 
            "RedAccentBrush", "GreenAccentBrush", "GrayAccentBrush" 
        };
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
    }
}
