using System;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Akavache.HostState
{
    public class AkavacheStateService<TState> : IHostedService
    where TState : new()
    {
        private readonly StateWrapper<TState> _stateWrapper;
        private readonly ILogger<AkavacheStateService<TState>> _logger;

        public AkavacheStateService(AkavacheStateDriver driver, IState<TState> state, ILogger<AkavacheStateService<TState>> logger)
        {
            _stateWrapper = state as StateWrapper<TState>;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var name = typeof(TState).FullName;
            
            try
            {
                await _stateWrapper.LoadAsync(cancellationToken);
                _logger.LogTrace($"Persisted application {name}");
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to restore {name}, creating from scratch");
                await _stateWrapper.InvalidateAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _stateWrapper.SaveAsync(cancellationToken);
            await BlobCache.UserAccount.Flush().ToTask(cancellationToken);
        }
    }
}
