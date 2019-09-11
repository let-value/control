using Avalonia.Markup.Xaml;

namespace Target.Interface
{
    public class Application : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
