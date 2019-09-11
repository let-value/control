using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Resources;

namespace Shared
{
    public static class Resources
    {
        static readonly Lazy<Assembly> Assembly = new Lazy<Assembly>(System.Reflection.Assembly.GetExecutingAssembly);
        static readonly ConcurrentDictionary<string, ResourceManager> Managers = new ConcurrentDictionary<string, ResourceManager>();

        public static ResourceManager From(string target) => Managers.GetOrAdd(target, _ => new ResourceManager(target, Assembly.Value));
    }
}
