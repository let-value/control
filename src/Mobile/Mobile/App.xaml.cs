using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using Mobile.Interface.ControlPage;
using Mobile.Interface.SearchPage;
using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Mobile
{
    public partial class App : Application
    {
        public App()
        {
            if(string.IsNullOrWhiteSpace(Preferences.Get("deviceId", string.Empty)))
                Preferences.Set("deviceId", Guid.NewGuid().ToString());

            RxApp.SuspensionHost.CreateNewAppState = () => new State();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new AkavacheSuspensionDriver<State>());

            RxApp.SuspensionHost.ObserveAppState<State>().Subscribe(state =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Locator.CurrentMutable.RegisterConstant<IScreen>(state);
                    Locator.CurrentMutable.Register<IViewFor<ControlPageViewModel>>(() => new ControlPageView());
                    Locator.CurrentMutable.Register<IViewFor<SearchPageViewModel>>(() => new SearchPageView());

                    if (state.Router.GetCurrentViewModel() == null)
                        state.Router.Navigate.Execute(new ControlPageViewModel());

                    MainPage = new RoutedViewHost();
                });
            });

            MainPage = new AppPlaceholder();

            InitializeComponent();
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            var caches = new [] 
            { 
                BlobCache.LocalMachine, 
                BlobCache.Secure, 
                BlobCache.UserAccount, 
                BlobCache.InMemory 
            };

            caches.Select(x => x.Flush()).Merge().Select(_ => Unit.Default).Wait();
        }

        protected override void OnResume()
        {
            
        }
    }
}
