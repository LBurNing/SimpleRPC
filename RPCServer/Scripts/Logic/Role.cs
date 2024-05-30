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
        private Udp _udp;

        public Role(string user, TcpClient tcp, UdpClient udp) 
        {
            _user = user;
            _tcp = new Tcp(tcp);
            _udp = new Udp(udp);
        }

        public void Update()
        {
            _tcp?.Update();
            _udp?.Update();
        }

        public void Dispose()
        {
            _tcp?.Dispose();
            _udp?.Dispose();
        }

        public void Send(BuffMessage message, ProtocolType type = ProtocolType.Tcp)
        {
            if(type == ProtocolType.Tcp)
            {
                _tcp.Send(message);
            }
            else if(type == ProtocolType.Udp)
            {
                _udp.Send(message);
            }
        }

        public bool TcpConnect { get { return _tcp.Connected; } }
        public string User { get { return _user; } }
    }
}
