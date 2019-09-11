using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Target.Interface;
using Target.Networking;

namespace Target
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(CreateConfiguration(args))
                .ConfigureServices(ConfigureServices)
                .Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            logger.LogTrace("Start app host");

            await host.RunAsync();

            //var avalonia = AppBuilder
            //    .Configure(new Application())
            //    .UsePlatformDetect()
            //    .UseReactiveUI()
            //    .SetupWithoutStarting()
            //    .Start<MainWindow>();
        }

        private static Action<HostBuilderContext, IConfigurationBuilder> CreateConfiguration(
            string[] args
        ) =>
            (context, config) => config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", true, false)
                .AddJsonFile($"settings.{context.HostingEnvironment.EnvironmentName}.json", true, false)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddLogging(builder => builder
                .AddConfiguration(context.Configuration.GetSection("Logging"))
                .AddDebug()
                .AddConsole()
            );

            services.AddHostedService<NetworkingService>();
            services.AddSingleton<MainWindowModel>();
        }
    }
}
