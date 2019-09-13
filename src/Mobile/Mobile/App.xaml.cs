using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Forms;

namespace Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            RxApp.SuspensionHost.CreateNewAppState = () => new Controller();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new AkavacheSuspensionDriver<Controller>());

            RxApp.SuspensionHost.ObserveAppState<Controller>().Subscribe(state =>
            {
                Locator.CurrentMutable.RegisterConstant(state, typeof(IScreen));
                Locator.CurrentMutable.Register(() => new ControlPageView(), typeof(IViewFor<ControlPageViewModel>));

                if(!(state.Router.GetCurrentViewModel() is ControlPageViewModel))
                    state.Router.Navigate.Execute(new ControlPageViewModel(state));

                Device.BeginInvokeOnMainThread(() =>
                {
                    MainPage = new RoutedViewHost();
                });
            });

            MainPage = new MainPage();
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
