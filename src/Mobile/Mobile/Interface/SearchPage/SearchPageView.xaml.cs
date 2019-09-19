using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using Mobile.Models;
using Mobile.Networking;
using ReactiveUI;
using ReactiveUI.XamForms;
using Rssdp;
using Splat;
using Xamarin.Forms.Xaml;

namespace Mobile.Interface.SearchPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPageView : ReactiveContentPage<SearchPageViewModel>
    {
        private State State { get; set; }
        private DiscoveryService _discoveryService;
        private readonly SourceList<TargetDevice> _devicesSource = new SourceList<TargetDevice>();

        public SearchPageView()
        {
            InitializeComponent();

            this.WhenActivated(async disposables =>
            {
                ViewModel.HostScreen = Locator.Current.GetService<IScreen>();
                State = RxApp.SuspensionHost.GetAppState<State>();
                
                ViewModel
                    .WhenAnyValue(x => x.SelectedDevice)
                    .Where(x => x != null)
                    .Subscribe(device =>
                    {
                        State.PreviousDevice = device;
                        ViewModel.HostScreen.Router.NavigateBack.Execute();
                    })
                    .DisposeWith(disposables);

                ViewModel.RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, x => x.RefreshCommand, x => x.Refresh)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.Devices, x => x.DevicesList.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.SelectedDevice, x => x.DevicesList.SelectedItem)
                    .DisposeWith(disposables);

                _devicesSource
                    .Connect()
                    .Bind(ViewModel.Devices)
                    .DisposeMany()
                    .Subscribe()
                    .DisposeWith(disposables);

                ViewModel.RefreshCommand.Execute();
            });
        }

        private Task<DiscoveredSsdpDevice[]> RefreshAsync()
        {
            _devicesSource.Clear();
            return _discoveryService.SearchAsync();
        }

        protected override void OnAppearing()
        {
            _discoveryService = new DiscoveryService();

            _discoveryService.OnDevice += async (sender, device) =>
                _devicesSource.Add(await TargetDevice.FromDiscoveredDeviceAsync(device.DiscoveredDevice));

            _discoveryService.OnDeviceDisconnect += (sender, device) =>
                _devicesSource.RemoveMany(_devicesSource.Items.Where(x => x.Usn == device.DiscoveredDevice.Usn));

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            _discoveryService.Dispose();
            base.OnDisappearing();
        }
    }
}