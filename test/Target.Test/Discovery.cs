using System.Linq;
using Rssdp.Infrastructure;
using Target.Networking;
using Xunit;

namespace Target.Test
{
    public class Discovery
    {
        [Fact]
        public void DeviceDefinition()
        {
            var validator = new Upnp10DeviceValidator();
            
            var errors = validator
                .GetValidationErrors(DiscoveryService.DeviceDefinition.Value)
                .ToList();

            Assert.True(
                !errors.Any(),
                string.Join(',', errors)
            );
        }
    }
}
