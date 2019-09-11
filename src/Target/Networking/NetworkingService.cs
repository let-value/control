using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Target.Extensions;

namespace Target.Networking
{
    public class NetworkingService : IHostedService
    {
        IConfiguration Configuration { get; }
        IWebHost _host;
        readonly ILogger<NetworkingService> _logger;

        public NetworkingService(ILogger<NetworkingService> logger, IConfiguration configuration)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder
                .AddConfiguration(Configuration.GetSection("Logging"))
                .AddDebug()
                .AddConsole()
            );

            services.AddHostedService<DiscoveryService>();
            services.AddGrpc();

            services.AddMvcCore()
                .AddApiExplorer()
                .AddAuthorization()
                .AddCors()
                .AddDataAnnotations()
                .AddFormatterMappings();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapGrpcService<GreeterService>());
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _host = new WebHostBuilder()
                .UseConfiguration(Configuration)
                .UseKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")))
                .UseStartup<NetworkingService>()
                .Build();

            _logger.LogTrace("Start networking web host");
            await _host.StartAsync(cancellationToken);

            _logger.LogTrace("Start discovery service");
            await _host.Services.GetHostedService<DiscoveryService>().StartAsync(cancellationToken);

            _logger.LogInformation("Hosting started");

            foreach (var address in _host.Services
                .GetService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()
                .Addresses
            )
                _logger.LogInformation(address);    
            
        }

        public Task StopAsync(CancellationToken cancellationToken) => _host.StopAsync(cancellationToken);
    }
}
