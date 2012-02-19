using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Net.NetworkInformation;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface INetworkInterfaceFacade
    {
        bool IsConnectionAvailable { get; }

        bool IsOnWifi { get; }

        string NetworkName { get; }

        event EventHandler<NetworkNotificationEventArgs> NetworkChanged;
    }

    public class NetworkInterfaceFacade : INetworkInterfaceFacade
    {

        public bool IsConnectionAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }

        public bool IsOnWifi
        {
            get
            {
                return WifiNetwork != null;
            }
        }

        public string NetworkName
        {
            get
            {
                var network = WifiNetwork;

                if (network == null)
                {
                    throw new InvalidOperationException("WiFi not available");
                }

                return network.InterfaceName;
            }
        }

        private NetworkInterfaceInfo WifiNetwork
        {
            get
            {
                return new NetworkInterfaceList().Where(
                    x => x.InterfaceType == NetworkInterfaceType.Wireless80211 &&
                    x.InterfaceState == ConnectState.Connected).FirstOrDefault();
            }
        }

        public event EventHandler<NetworkNotificationEventArgs> NetworkChanged
        {
            add { DeviceNetworkInformation.NetworkAvailabilityChanged += value; }
            remove { DeviceNetworkInformation.NetworkAvailabilityChanged -= value; }
        }
    }
}
