using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Rssdp;
using Shared;

namespace Target.Networking
{
    public class DiscoveryService : IHostedService
    {
        int _executionAttempt;
        readonly SsdpDevicePublisher _publisher = new SsdpDevicePublisher();
        public readonly Lazy<SsdpRootDevice> DeviceDefinition;

        public DiscoveryService(IServer server)
        {
            DeviceDefinition = new Lazy<SsdpRootDevice>(() =>
            {
                var address = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .OrderByDescending(x => x.Speed)
                    .FirstOrDefault(x =>
                        x.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        x.OperationalStatus == OperationalStatus.Up
                    )
                    ?.GetIPProperties()
                    ?.UnicastAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => x.Address)
                    .FirstOrDefault();

                var endpoint = server
                    ?.Features
                    .Get<IServerAddressesFeature>()
                    .Addresses
                    .Select(x => new Uri(x))
                    .FirstOrDefault() ?? new Uri("http://google.com");
                
                return new SsdpRootDevice
                {
                    CacheLifetime = TimeSpan.FromMinutes(30),
                    Location = new Uri($"{endpoint.Scheme}://{address}:{endpoint.Port}"),
                    DeviceTypeNamespace = "control-target",
                    DeviceType = "target",
                    FriendlyName = "Control target",
                    Manufacturer = "let.value",
                    ModelName = "target",
                    Uuid = Resources.From("Shared.Strings").GetString("discoveryUUID")
                };
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_executionAttempt++ == 0)
                return Task.CompletedTask;

            _publisher.AddDevice(DeviceDefinition.Value);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _publisher?.RemoveDevice(DeviceDefinition.Value);
            _publisher?.Dispose();
            return Task.CompletedTask;
        }
    }
}
