using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class AttackAction : BTNode
    {
        public override void OnUpdate()
        {
            LogHelper.Log("¹¥»÷");
            status = NodeStatus.Success;
        }
    }
}