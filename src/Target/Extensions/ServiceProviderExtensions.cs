using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Target.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T GetHostedService<T>(this IServiceProvider provider)
        where T : class =>
            provider.GetServices<IHostedService>().FirstOrDefault(x => x is T) as T;
    }
}
