using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Rssdp;
using Rssdp.Infrastructure;
using Shared;

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
                .Where(x => x.GatewayAddresses.Count > 0)
                .Select(@interface =>
                    @interface.UnicastAddresses
                        .Where(address => (address.Address.AddressFamily == AddressFamily.InterNetwork ||
                                           address.Address.AddressFamily == AddressFamily.InterNetworkV6) &&
                                          !IPAddress.IsLoopback(address.Address)
                        )
                        .Select(address => address.Address))
                .Aggregate((x, y) => x.Concat(y))
                .Select(address =>
                {
                    var locator = new SsdpDeviceLocator(new SsdpCommunicationsServer(new SocketFactory(address.ToString())))
                        {NotificationFilter = $"uuid:{Resources.From("Shared.Strings").GetString("discoveryUUID")}"};

                    locator.DeviceAvailable += OnDevice;
                    locator.DeviceUnavailable += OnDeviceDisconnect;
                    locator.StartListeningForNotifications();

                    return locator;
                })
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
