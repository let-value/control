using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Akavache.HostState
{
    public class AkavacheStateDriver
    {
        private readonly ConcurrentDictionary<Type, string> _keyMemo = new ConcurrentDictionary<Type, string>();
        private readonly AkavacheStateSettings _settings;

        public AkavacheStateDriver(AkavacheStateSettings settings)
        {
            BlobCache.ApplicationName = settings.ApplicationName;

            _settings = settings;
        }

        public string GetKeyName<TState>() => _keyMemo.GetOrAdd(typeof(TState), type => type.FullName);

        public IObservable<Unit> InvalidateState<TState>() =>
            BlobCache
                .UserAccount
                .Invalidate(GetKeyName<TState>());

        public IObservable<TState> LoadState<TState>() =>
            BlobCache
                .UserAccount
                .Get(GetKeyName<TState>())
                .Select(bytes =>
                    JsonConvert.DeserializeObject<TState>(
                        Encoding.UTF8.GetString(bytes),
                        _settings.JsonSettings
                    )
                );

        public IObservable<Unit> SaveState<TState>(object state) =>
            BlobCache
                .UserAccount
                .Insert(
                    GetKeyName<TState>(),
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(
                            state,
                            _settings.JsonSettings
                        )
                    )
                );
    }
}
