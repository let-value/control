using System;
using System.Threading.Tasks;
using Rssdp;

namespace Mobile.Models
{
    public class TargetDevice
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public Uri Location { get; set; }
        public string Usn { get; set; }

        public static async Task<TargetDevice> FromDiscoveredDeviceAsync(DiscoveredSsdpDevice discoveredDevice)
        {
            var info = await discoveredDevice.GetDeviceInfo();
            return new TargetDevice
            {
                Name = info.FriendlyName,
                Location = new Uri($"{discoveredDevice.DescriptionLocation.Scheme}://{discoveredDevice.DescriptionLocation.Authority}"),
                Uuid = info.Uuid,
                Usn = discoveredDevice.Usn
            };
        }
    }
}
