using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Rssdp;
using Rssdp.Infrastructure;

namespace Mobile.Networking
{
    public class DiscoveryService : IDisposable
    {
        private SsdpDeviceLocator[] _locators;

        public event EventHandler<DeviceAvailableEventArgs> OnDevice;
        public event EventHandler<DeviceUnavailableEventArgs> OnDeviceDisconnect;

        public async Task<DiscoveredSsdpDevice[]> SearchAsync()
        {
            _locators = NetworkInterface
                .GetAllNetworkInterfaces()
                .Select(x => x.GetIPProperties())
                .Select(@interface =>
                    @interface.UnicastAddresses
                        .Where(address => (address.Address.AddressFamily == AddressFamily.InterNetwork ||
                                           address.Address.AddressFamily == AddressFamily.InterNetworkV6) &&
                                          !IPAddress.IsLoopback(address.Address)
                        )
                        .Select(address => address.Address))
                .Where(x => x.Any())
                .Aggregate(new List<IPAddress>(), (x, y) => x.Concat(y).ToList())
                .Select(address =>
                {
                    try
                    {
                        var locator = new SsdpDeviceLocator(
                                new SsdpCommunicationsServer(new SocketFactory(address.ToString())))
                            {NotificationFilter = "urn:control-target:device:ControlTarget:1"};

                        locator.DeviceAvailable += OnDevice;
                        locator.DeviceUnavailable += OnDeviceDisconnect;
                        locator.StartListeningForNotifications();

                        return locator;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToArray();
           
            var results = await Task.WhenAll(_locators.Select(x => x.SearchAsync()));

            return results
                .Aggregate((x, y) => x.Concat(y))
                .ToArray();
        }

        public void Dispose()
        {
            foreach (var locator in _locators)
            {
                locator.DeviceAvailable -= OnDevice;
                locator.DeviceUnavailable -= OnDeviceDisconnect;

                locator.StopListeningForNotifications();
                locator.Dispose();
            }
        }
    }
}
