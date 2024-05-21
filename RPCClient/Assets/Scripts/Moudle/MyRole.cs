using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Game
{
    public class MyRole : IMoudle
    {
        private Role _role;

        public MyRole()
        {
        }

        public void Create(string id)
        {
            _role = new Role(id);
            Connect();
        }

        private void Connect()
        {
            BuffMessage buffMessage = GameFrame.message.GetBuffMessage();
            byte[] bytes = Encoding.UTF8.GetBytes(Id);
            bytes.CopyTo(buffMessage.bytes, 0);
            buffMessage.length = bytes.Length;
            Main.Socket.Send(buffMessage);
        }

        public void Init()
        {
        }

        public string Id
        {
            get
            {
                return _role.Id;
            }
        }

        public void UnInit()
        {
        }

        public void Update()
        {
        }
    }
}