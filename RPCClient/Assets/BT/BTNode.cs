using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public enum NodeStatus
    {
        Running,
        Success,
        Fail,
    }

    public abstract class BTNode
    {
        public NodeStatus status {  get; protected set; }
        public abstract void OnUpdate();
    }
}