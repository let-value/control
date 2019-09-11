using ReactiveUI;

namespace Target.Interface
{
    public class MainWindowModel : ISupportsActivation
    {
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}
