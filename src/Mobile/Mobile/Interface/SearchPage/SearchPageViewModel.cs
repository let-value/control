using System.Reactive;
using System.Runtime.Serialization;
using DynamicData.Binding;
using Mobile.Models;
using ReactiveUI;
using Rssdp;

namespace Mobile.Interface.SearchPage
{
    [DataContract]
    public class SearchPageViewModel : ReactiveObject, IRoutableViewModel
    {
        [DataMember]
        public string UrlPathSegment => "search";
        public IScreen HostScreen { get; set; }

        public ReactiveCommand<Unit, DiscoveredSsdpDevice[]> RefreshCommand;

        private TargetDevice _selectedDevice;
        public TargetDevice SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }

        public IObservableCollection<TargetDevice> Devices { get; } = new ObservableCollectionExtended<TargetDevice>();
    }
}
