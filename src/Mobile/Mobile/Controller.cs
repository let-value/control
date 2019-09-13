using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Forms;

namespace Mobile
{
    [DataContract]
    public class Controller : ReactiveObject, IScreen
    {
        private RoutingState _router = new RoutingState();

        [DataMember]
        public RoutingState Router
        {
            get => _router;
            set => this.RaiseAndSetIfChanged(ref _router, value);
        }
    }
}
