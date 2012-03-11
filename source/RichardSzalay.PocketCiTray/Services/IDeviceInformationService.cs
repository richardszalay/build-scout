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
            get { return DeviceStatus.ApplicationMemoryUsageLimit < 94371840; }
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
