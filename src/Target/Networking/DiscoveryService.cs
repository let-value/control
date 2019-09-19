using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Akavache.HostState;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Rssdp;
using Target.Models;

namespace Target.Networking
{
    public class SsdpDetailsController : Controller
    {
        [Route("ssdpDetails.xml")]
        public IActionResult GetDetails() => Content(DiscoveryService.DeviceDefinition.Value.ToDescriptionDocument(), "application/xml");
    }

    public class DiscoveryService : IHostedService
    {
        int _executionAttempt;
        readonly SsdpDevicePublisher _publisher = new SsdpDevicePublisher();
        public static Lazy<SsdpRootDevice> DeviceDefinition;
        
        public DiscoveryService(IServer server, IState<State> state)
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

                var endpoints = server
                    ?.Features
                    .Get<IServerAddressesFeature>()
                    .Addresses
                    .Select(x => new Uri(x))
                    .ToArray();

                var endpoint = endpoints?.FirstOrDefault(x => x.Scheme == "http") ?? new Uri("http://google.com");

                return new SsdpRootDevice
                {
                    CacheLifetime = TimeSpan.FromMinutes(30),
                    Location = new Uri($"{endpoint.Scheme}://{address}:{endpoint.Port}/ssdpDetails.xml"),
                    DeviceTypeNamespace = "control-target",
                    DeviceType = "ControlTarget",
                    DeviceVersion = 1,
                    FriendlyName = Environment.MachineName,
                    Manufacturer = "let.value",
                    ModelName = "target",
                    Uuid = state.Value.Id
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
