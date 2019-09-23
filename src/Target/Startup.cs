using System;
using System.IO;
using Akavache.HostState;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Target.Models;
using Target.Networking;

namespace Target
{
    public class Startup
    {
        private IWebHostEnvironment Env { get; set; }
        private IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Env = env;
            Configuration = configuration;
        }
       
        public static Action<WebHostBuilderContext, IConfigurationBuilder> CreateConfiguration(
            string[] args
        ) =>
            (context, config) => config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", true, false)
                .AddJsonFile($"settings.{context.HostingEnvironment.EnvironmentName}.json", true, false)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder
                .AddConfiguration(Configuration.GetSection("Logging"))
                .AddDebug()
                .AddConsole()
                .AddEventLog()
            );

            services.AddMvcCore()
                .AddApiExplorer()
                .AddAuthorization()
                .AddCors()
                .AddDataAnnotations()
                .AddFormatterMappings();

            services.AddAkavacheState(() => new State());
            services.AddHostedService<DiscoveryService>();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseEndpoints(endpoints => endpoints.MapHub<MessageService>("/signals"));
        }
    }
}
