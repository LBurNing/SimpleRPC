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
        private Tcp _tcp;

        public Role(string user, TcpClient tcp) 
        {
            _user = user;
            _tcp = new Tcp(tcp);
        }

        public void Update()
        {
            _tcp?.Update();
        }

        public void Dispose()
        {
            _tcp?.Dispose();
        }

        public void Send(BuffMessage message)
        {
            _tcp.Send(message);
        }

        public bool TcpConnect { get { return _tcp.Connected; } }
        public string User { get { return _user; } }
    }
}
