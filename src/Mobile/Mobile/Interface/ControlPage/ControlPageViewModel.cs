using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using Mobile.Interface.SearchPage;
using Mobile.Models;
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

        private TargetDevice _connected;
        public TargetDevice Connected
        {
            get => _connected;
            set => this.RaiseAndSetIfChanged(ref _connected, value);
        }
        
        public ICommand Sleep;
        public ICommand Shutdown;
        public ICommand Reboot;
    }
}
