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

        public Role(string user, TcpClient tcpClient) 
        {
            _user = user;
            _client = new SocketClient(tcpClient);
        }

        public void Update()
        {
            _client?.Update();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public void Send(BuffMessage message)
        {
            _client.Send(message);
        }

        public bool Connect { get { return _client.Connected; } }
        public string User { get { return _user; } }
    }
}
