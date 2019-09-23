using Akavache;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Mobile
{
    public class AkavacheSuspensionDriver<TAppState> : ISuspensionDriver where TAppState : class
    {
        private const string AppStateKey = "appState";
        
        public AkavacheSuspensionDriver() => BlobCache.ApplicationName = "Control";

        public IObservable<Unit> InvalidateState() => BlobCache.UserAccount.Invalidate(AppStateKey);

        public IObservable<object> LoadState() => BlobCache.UserAccount
            .Get(AppStateKey)
            .Select(bytes =>
                JsonConvert.DeserializeObject<TAppState>(Encoding.UTF8.GetString(bytes), JsonSettings)
            );

        public IObservable<Unit> SaveState(object state) => BlobCache.UserAccount.Insert(
            AppStateKey,
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(state, JsonSettings))
            );

        // ReSharper disable once StaticMemberInGenericType
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}
