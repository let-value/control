using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms.Xaml;

namespace Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPageView : ReactiveContentPage<ControlPageViewModel>
    {
        public ControlPageView()
        {
            InitializeComponent();
        }
    }

    [DataContract]
    public class ControlPageViewModel : ReactiveObject, IRoutableViewModel
    {
        private string _searchQuery;

        [DataMember]
        public string SearchQuery 
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public string UrlPathSegment => "control";
        public IScreen HostScreen { get; }

        public ControlPageViewModel(IScreen screen)
        {
            HostScreen = screen;
        }
    }
}