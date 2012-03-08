using Microsoft.Phone.Info;
namespace RichardSzalay.PocketCiTray.Services
{
    public interface IDeviceInformationService
    {
        bool IsLowEndDevice { get; }
    }

    public class DeviceInformationService : IDeviceInformationService
    {
        public bool IsLowEndDevice
        {
            get { return DeviceStatus.ApplicationMemoryUsageLimit < 90; }
        }
    }

    public class LowEndDeviceInformationService : IDeviceInformationService
    {
        public bool IsLowEndDevice
        {
            get { return true; }
        }
    }
}
