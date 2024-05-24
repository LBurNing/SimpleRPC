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
        private Dictionary<string, Role> _roles;

        public Gateway()
        {
            _roles = new Dictionary<string, Role>();
        }

        public void Update()
        {
            foreach (var client in _roles)
            {
                client.Value.Update();

                if (!client.Value.Connect)
                    _roles.Remove(client.Key);
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
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            byte[] bytes = new byte[4];
            while (true)
            {
                await client.GetStream().ReadAtLeastAsync(bytes, sizeof(int));
                int dateLength = BitConverter.ToInt32(bytes);
                bytes = new byte[dateLength];
                await client.GetStream().ReadAtLeastAsync(bytes, dateLength);
                string id = Encoding.UTF8.GetString(bytes);

                Role? role;
                if(_roles.TryGetValue(id, out role))
                    role.Dispose();

                role = new Role(id, client);
                _roles[id] = role;
                LogHelper.Log($"{id} Client connected...");
                break;
            }
        }

        public void Send(BuffMessage message)
        {
            foreach (var role in _roles)
            {
                role.Value.Send(message);
            }
        }

        public Role GetRole(string id)
        {
            Role role;
            if (_roles.TryGetValue(id, out role))
                return role;

            return null;
        }

        public void RemoveRole(string id)
        {
            if (_roles.ContainsKey(id))
                _roles.Remove(id);
        }

        public void Destroy()
        {
            _listener?.Dispose();
            _listener = null;
        }
    }
}