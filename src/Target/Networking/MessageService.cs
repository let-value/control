using System.Collections.Generic;
using Akavache.HostState;
using Microsoft.AspNetCore.SignalR;
using Target.Models;

namespace Target.Networking
{
    public class MessageService : Hub
    {
        private readonly IState<State> _state;

        public MessageService(IState<State> state)
        {
            _state = state;
        }

        public async void Info(string id, string name)
        {
            Context.Items.TryAdd("id", id);
            await Clients.Caller.SendAsync("connected");
        }

        public void Sleep()
        {
            PowerControl.PowerControl.Sleep();   
        }

        public void Shutdown()
        {
            PowerControl.PowerControl.Shutdown();  
        }

        public void Reboot()
        {
            PowerControl.PowerControl.Reboot();   
        }
    }
}
