using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Target.Extensions;
using Target.Networking;

namespace Target
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new WebHostBuilder()
                .ConfigureAppConfiguration(Startup.CreateConfiguration(args))
                .UseStartup<Startup>()
                .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")))
                .Build();

            var lifetime = host.Services.GetService<IHostApplicationLifetime>();
            var logger = host.Services.GetService<ILogger<Program>>();

            await host.StartAsync();
            await host.Services.GetHostedService<DiscoveryService>().StartAsync(CancellationToken.None);

            foreach (var address in host
                .ServerFeatures
                .Get<IServerAddressesFeature>()
                .Addresses
            )
                logger.LogInformation(address);

            await host.WaitForShutdownAsync(lifetime.ApplicationStopping);

            //var avalonia = AppBuilder
            //    .Configure(new Application())
            //    .UsePlatformDetect()
            //    .UseReactiveUI()
            //    .SetupWithoutStarting()
            //    .Start<MainWindow>();
        }

        
    }
}
