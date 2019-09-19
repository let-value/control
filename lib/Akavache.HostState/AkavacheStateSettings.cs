using Newtonsoft.Json;

namespace Akavache.HostState
{
    public class AkavacheStateSettings
    {
        public string ApplicationName => "AkavacheState";
        
        public JsonSerializerSettings JsonSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}
