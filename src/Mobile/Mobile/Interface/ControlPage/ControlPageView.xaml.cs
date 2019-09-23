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

                this.OneWayBind(ViewModel, x => x.Device, x => x.Title)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, x => x.GoToSearch, x => x.GoToSearch)
                    .DisposeWith(disposables);

                var connected = ViewModel
                    .WhenAnyValue(x => x.Connected);

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

#pragma warning disable 4014
                ControlAsync(disposables);
#pragma warning restore 4014
            });
        }

        private async Task OnSleep()
        {
            await Connection.SendAsync("sleep");
        }

        private async Task OnShutdown()
        {
            if(await DisplayAlert ("Warning", "Are you sure you want to shutdown the computer?", "Yes", "No"))
                await Connection.SendAsync("shutdown");
        }

        private async Task OnReboot()
        {
            if(await DisplayAlert ("Warning", "Are you sure you want to reboot the computer?", "Yes", "No"))
                await Connection.SendAsync("reboot");
        }

        private async Task ControlAsync(CompositeDisposable disposables)
        {
            OnDisconnected();

            if (!await TryConnectAsync(disposables))
            {
                await SearchAsync();
                if (!await TryConnectAsync(disposables))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ViewModel.Connected = false;
                        ViewModel.Device = "Not found";
                    });
                }
            }
        }

        private HubConnection Connection { get; set; }
        
        private void OnConnected()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ViewModel.Connected = true;
                ViewModel.Device = State.PreviousDevice.Name;
            });
        }

        private void OnDisconnected()
        {
            Device.BeginInvokeOnMainThread(() => { ViewModel.Connected = false; ViewModel.Device = "Disconnected"; });
        }
        
        private async Task<bool> TryConnectAsync(CompositeDisposable disposables)
        {
            if (State?.PreviousDevice?.Location == null)
                return false;

            try
            {
                Device.BeginInvokeOnMainThread(() => { ViewModel.Device = "Connecting"; });

                if (Connection != null)
                    await Connection.DisposeAsync();

                Connection = new HubConnectionBuilder()
                    .WithUrl(new UriBuilder(State?.PreviousDevice?.Location) {Path = "/signals"}.Uri.ToString())
                    .Build();

#pragma warning disable 1998
                Connection.Closed += async ex => OnDisconnected();
#pragma warning restore 1998

                Connection.On("connected", OnConnected)
                    .DisposeWith(disposables);

                await Connection.StartAsync();

                Device.BeginInvokeOnMainThread(() => { ViewModel.Device = "Waiting for permission"; });

                await Connection.SendAsync("info", State.Id, DeviceInfo.Name);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        

        private Task SearchAsync()
        {
            Device.BeginInvokeOnMainThread(() => { ViewModel.Device = "Searching"; });

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