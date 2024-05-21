using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Role
    {
        private string id;

        public Role(string id)
        {
            this.id = id;
        }

        public string Id
        {
            get { return id; }
        }
    }
}