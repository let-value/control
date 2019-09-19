using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Mobile.Models;
using Mobile.Networking;
using ReactiveUI;
using ReactiveUI.XamForms;
using Rssdp;
using Splat;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mobile.Interface.ControlPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPageView : ReactiveContentPage<ControlPageViewModel>
    {
        private State State { get; set; }

        public ControlPageView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                ViewModel.HostScreen = Locator.Current.GetService<IScreen>();
                State = RxApp.SuspensionHost.GetAppState<State>();

                this.OneWayBind(ViewModel, x => x.Connected.Name, x => x.Title)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, x => x.GoToSearch, x => x.GoToSearch)
                    .DisposeWith(disposables);

                var connected = ViewModel.WhenAnyValue(x => x.Connected).Select(x => x != null);

                ViewModel.Sleep = ReactiveCommand
                    .CreateFromTask(OnSleep, connected)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Sleep, x => x.Sleep)
                    .DisposeWith(disposables);

                ViewModel.Shutdown = ReactiveCommand
                    .CreateFromTask(OnShutdown, connected)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Shutdown, x => x.Shutdown)
                    .DisposeWith(disposables);

                ViewModel.Reboot = ReactiveCommand
                    .CreateFromTask(OnReboot, connected)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Reboot, x => x.Reboot)
                    .DisposeWith(disposables);

                ControlAsync(disposables);
            });
        }

        private async Task OnSleep()
        {
            
        }

        private async Task OnShutdown()
        {
            
        }

        private async Task OnReboot()
        {
            
        }

        private async Task ControlAsync(CompositeDisposable disposables)
        {
            OnDisconnected();

            if (!await TryConnectAsync(disposables))
            {
                await SearchAsync();
                await TryConnectAsync(disposables);
            }
        }

        private HubConnection Connection { get; set; }
        
        private void OnConnected()
        {
            Device.BeginInvokeOnMainThread(() => { ViewModel.Connected = State.PreviousDevice; });
        }

        private void OnDisconnected()
        {
            Device.BeginInvokeOnMainThread(() => { ViewModel.Connected = null; });
        }
        
        private async Task<bool> TryConnectAsync(CompositeDisposable disposables)
        {
            if (State?.PreviousDevice?.Location == null)
                return false;

            try
            {
                if (Connection != null)
                    await Connection.DisposeAsync();

                Connection = new HubConnectionBuilder()
                    .WithUrl(new UriBuilder(State?.PreviousDevice?.Location) {Path = "/signals"}.Uri.ToString())
                    .Build();

                Connection.Closed += async ex => OnDisconnected();

                Connection.On("connected", OnConnected)
                    .DisposeWith(disposables);

                await Connection.StartAsync();

                await Connection.SendAsync("info", State.Id, DeviceInfo.Name);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        

        private Task SearchAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(async () =>
            {
                using var discoveryService = new DiscoveryService();

                using var observer = new AnonymousObserver<DeviceAvailableEventArgs>(async e =>
                {
                    State.PreviousDevice = await TargetDevice.FromDiscoveredDeviceAsync(e.DiscoveredDevice);
                    tcs.SetResult(true);
                });

                var pipeline = Observable
                    .FromEventPattern<DeviceAvailableEventArgs>(
                        handler => discoveryService.OnDevice += handler,
                        handler => discoveryService.OnDevice -= handler
                    )
                    .Select(x => x.EventArgs);

                if (State.PreviousDevice == null)
                    pipeline = pipeline
                        .Take(1);

                if (State?.PreviousDevice?.Usn != null)
                    pipeline = pipeline
                        .Where(x => x.DiscoveredDevice.Usn == State.PreviousDevice.Usn)
                        .Take(1);

                pipeline.Subscribe(observer);

                await discoveryService.SearchAsync();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }
    }
}