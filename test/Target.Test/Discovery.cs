using System.Linq;
using Akavache.HostState;
using Rssdp.Infrastructure;
using Target.Models;
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
            var discovery = new DiscoveryService(
                null,
                new StateWrapper<State>(
                    () => new State(),
                    new AkavacheStateDriver(new AkavacheStateSettings())
                )
            );

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
