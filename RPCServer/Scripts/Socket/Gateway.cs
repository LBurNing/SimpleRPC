using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Concurrent;

namespace Game
{
    public class Gateway
    {
        private TcpListener? _listener;
        private List<Role> _roles;

        public Gateway()
        {
            _roles = new List<Role>();
        }

        public void Update()
        {
            foreach (var client in _roles)
            {
                client.Update();
            }
        }

        public async void RunServer()
        {
            _listener = new TcpListener(IPAddress.Any, 8888);
            _listener.Start();
            LogHelper.Log("Server started...");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _roles.Add(new Role(client));
                LogHelper.Log("connected...");
            }
        }

        public void Send(BuffMessage message)
        {
            foreach (var role in _roles)
            {
                role.Send(message);
            }
        }

        public void Remove(Role role)
        {
            _roles.Remove(role);
        }

        public void Destroy()
        {
            _listener?.Dispose();
            _listener = null;
        }
    }
}