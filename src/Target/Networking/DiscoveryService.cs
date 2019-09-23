using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Akavache.HostState;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rssdp;
using Rssdp.Infrastructure;
using Target.Models;
using IApplicationLifetime = Avalonia.Controls.ApplicationLifetimes.IApplicationLifetime;

namespace Target.Networking
{
    public class SsdpDetailsController : Controller
    {
        [Route("ssdpDetails.xml")]
        public IActionResult GetDetails() => Content(DiscoveryService.DeviceDefinition.Value.ToDescriptionDocument(), "application/xml");
    }

    public class DiscoveryService : IHostedService
    {
        private ILogger<DiscoveryService> _logger;

        SsdpDevicePublisher[] _publishers;
        public static Lazy<SsdpRootDevice> DeviceDefinition;

        private readonly IHostApplicationLifetime _lifetime;
        private CancellationTokenRegistration _startCallback;
        
        public DiscoveryService(ILogger<DiscoveryService> logger, IServer server, IState<State> state, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;

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
            _publishers = NetworkInterface
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
                        var locator = new SsdpDevicePublisher(
                            new SsdpCommunicationsServer(new SocketFactory(address.ToString())));
                        return locator;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToArray();

            _startCallback = _lifetime.ApplicationStarted.Register(AddDevices);

            return Task.CompletedTask;
        }

        private void AddDevices()
        {
            foreach (var publisher in _publishers)
                publisher.AddDevice(DeviceDefinition.Value);

            _startCallback.Dispose();

            _logger.LogInformation($"Discovery service started at {DeviceDefinition.Value.Location}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var publisher in _publishers)
            {
                publisher?.RemoveDevice(DeviceDefinition.Value);
                publisher?.Dispose();
            }
            
            return Task.CompletedTask;
        }
    }
}
