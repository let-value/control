using Xunit;

namespace PowerControl.Test
{
    public class Test
    {
        [Fact]
        public void Sleep()
        {
            PowerControl.Sleep();

            Assert.True(true);
        }

        [Fact]
        public void Shutdown()
        {
            PowerControl.Shutdown();

            Assert.True(true);
        }

        [Fact]
        public void Reboot()
        {
            PowerControl.Reboot();

            Assert.True(true);
        }
    }
}