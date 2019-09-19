using System;
using System.Runtime.Serialization;
using Mobile.Models;
using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Forms;

namespace Mobile
{
    [DataContract]
    public class State : ReactiveObject, IScreen
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private RoutingState _router = new RoutingState();

        [DataMember]
        public RoutingState Router
        {
            get => _router;
            set => this.RaiseAndSetIfChanged(ref _router, value);
        }

        [DataMember]
        public TargetDevice PreviousDevice { get; set; } = null;
    }
}
