using System;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Akavache.HostState
{
    public interface IState<out TState>
    {
        TState Value { get; }
        Task InvalidateAsync(CancellationToken cancellationToken = default);
        Task LoadAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(CancellationToken cancellationToken = default);
    }

    public class StateWrapper<TState> : IState<TState> where TState : new()
    {
        private readonly AkavacheStateDriver _driver;
        private readonly Func<TState> _factory;
        public TState Value { get; set; }

        public StateWrapper(Func<TState> stateFactory, AkavacheStateDriver driver)
        {
            _driver = driver;
            _factory = stateFactory;
        }

        public async Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            await _driver.InvalidateState<TState>().ToTask(cancellationToken);
            Value = _factory();
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            var state = await _driver.LoadState<TState>().ToTask(cancellationToken);
            Value = state;
        }

        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _driver.SaveState<TState>(Value).ToTask(cancellationToken);
        }
    }
}
