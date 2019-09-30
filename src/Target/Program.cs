using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace Target
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (WindowsServiceHelpers.IsWindowsService())
                Directory.SetCurrentDirectory(
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                );
            
            var host = new WebHostBuilder()
                .ConfigureAppConfiguration(Startup.CreateConfiguration(args))
                .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")))
                .UseStartup<Startup>()
                .Build();

            if (WindowsServiceHelpers.IsWindowsService())
                host.RunAsService();
            else
                await host.RunAsync();

            //var avalonia = AppBuilder
            //    .Configure(new Application())
            //    .UsePlatformDetect()
            //    .UseReactiveUI()
            //    .SetupWithoutStarting()
            //    .Start<MainWindow>();
        }

        
    }
}
