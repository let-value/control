using System.Runtime.Serialization;
using System.Windows.Input;
using Mobile.Interface.SearchPage;
using ReactiveUI;

namespace Mobile.Interface.ControlPage
{
    [DataContract]
    public class ControlPageViewModel : ReactiveObject, IRoutableViewModel
    {
        [DataMember]
        public string UrlPathSegment => "control";
        public IScreen HostScreen { get; set; }
        
        public ICommand GoToSearch => ReactiveCommand.CreateFromObservable(
            () => HostScreen.Router.Navigate.Execute(new SearchPageViewModel())
        );

        private string _device;
        public string Device
        {
            get => _device;
            set => this.RaiseAndSetIfChanged(ref _device, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => this.RaiseAndSetIfChanged(ref _connected, value);
        }
        
        public ICommand Sleep;
        public ICommand Shutdown;
        public ICommand Reboot;
    }
}
