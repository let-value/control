using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Akavache.HostState
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAkavacheState<TState>(
            this IServiceCollection services,
            Func<TState> stateFactory,
            Action<AkavacheStateSettings> options = null
        )
        where TState : class, new()
        {
            services.TryAddSingleton(typeof(AkavacheStateSettings), (context) =>
            {
                var settings = new AkavacheStateSettings();
                options?.Invoke(settings);
                return settings;
            });

            services.TryAddSingleton<AkavacheStateDriver>();

            return services
                .AddSingleton<IState<TState>>(context =>
                    new StateWrapper<TState>(
                        stateFactory,
                        context.GetService<AkavacheStateDriver>()
                    ))
                .AddHostedService<AkavacheStateService<TState>>();
        }
    }
}
