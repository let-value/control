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
            Context.Items.Add("id", id);
            await Clients.Caller.SendAsync("connected");
        }
    }
}
