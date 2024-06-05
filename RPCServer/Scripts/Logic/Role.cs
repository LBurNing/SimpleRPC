using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Role : IDisposable
    {
        private string _user;
        private SocketClient _client;

        public Role(TcpClient client) 
        {
            _client = new SocketClient(client, this);
        }

        public void Update()
        {
            _client?.Update();
        }

        public void Dispose()
        {
            _client?.Dispose();
            Game.gateway.Remove(this);
        }

        public void Send(BuffMessage message)
        {
            _client.Send(message);
        }

        public SocketClient client
        {
            get { return _client; }
            set { _client = value; }
        }

        public string id
        {
            set { _user = value; }
            get { return _user; }
        }
    }
}
