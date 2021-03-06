﻿using System;
using Microsoft.Phone.Info;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IDeviceInformationService
    {
        bool IsLowEndDevice { get; }
        string DeviceUniqueId { get; }
        string DeviceManufacturer { get; }
        string DeviceType { get; }
        string Device { get; }
        string OsVersion { get; }
    }

    public class DeviceInformationService : IDeviceInformationService
    {
        private bool? cachedIsLowEndDevice;

        public virtual bool IsLowEndDevice
        {
            get
            {
                if (!cachedIsLowEndDevice.HasValue)
                {
                    cachedIsLowEndDevice = GetIsLowEndDevice();
                }

                return cachedIsLowEndDevice.Value;
            }
        }

        private bool GetIsLowEndDevice()
        {
            try
            {
                long limit = (Int64)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");

                return limit < 94371840L;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public string DeviceUniqueId
        {
            get
            {
                var value = (byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId");
                return Convert.ToBase64String(value);
            }
        }

        public string DeviceManufacturer
        {
            get { return DeviceExtendedProperties.GetValue("DeviceManufacturer").ToString(); }
        }

        public string DeviceType
        {
            get { return DeviceExtendedProperties.GetValue("DeviceName").ToString(); }
        }

        public string Device
        {
            get { return string.Format("{0} - {1}", DeviceManufacturer, DeviceType); }
        }

        public string OsVersion
        {
            get { return string.Format("{0} {1}", Environment.OSVersion.Platform, Environment.OSVersion.Version); }
        }
    }

    public class LowEndDeviceInformationService : DeviceInformationService
    {
        public override bool IsLowEndDevice
        {
            get { return true; }
        }
    }
}
