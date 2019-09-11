using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Shared.Extensions
{
    public static class NetworkInterfaceExtensions
    {
        public static IPAddress GetLocalAddress(this NetworkInterface[] interfaces) => interfaces
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
    }
}
