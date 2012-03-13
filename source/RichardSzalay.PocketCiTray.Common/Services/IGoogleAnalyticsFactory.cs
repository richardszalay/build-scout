using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.WebAnalytics;
using Microsoft.WebAnalytics.Data;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IGoogleAnalyticsFactory
    {
        GoogleAnalytics Create();
    }

    public class GoogleAnalyticsFactory : IGoogleAnalyticsFactory
    {
        private readonly string trackingId;
        private readonly IDeviceInformationService deviceInformation;
        private readonly IApplicationInformation applicationInformation;

        public GoogleAnalyticsFactory(string trackingId,
            IDeviceInformationService deviceInformation, IApplicationInformation applicationInformation)
        {
            this.trackingId = trackingId;
            this.deviceInformation = deviceInformation;
            this.applicationInformation = applicationInformation;
        }

        public GoogleAnalytics Create()
        {

            GoogleAnalytics googleAnalytics = new GoogleAnalytics()
            {
                WebPropertyId = trackingId
            };

            Action<string, string> addFunc = (name,value) => 
                googleAnalytics.CustomVariables.Add(new PropertyValue { PropertyName = name, Value = value });

            addFunc("Application Version", applicationInformation.ApplicationVersion);
            addFunc("Trial Mode", applicationInformation.IsTrialMode.ToString());
            addFunc("Device ID", deviceInformation.DeviceUniqueId);
            addFunc("Device OS", deviceInformation.OsVersion);
            addFunc("Device", deviceInformation.Device);

            return googleAnalytics;
        }
    }
}
