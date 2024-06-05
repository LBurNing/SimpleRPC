using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class RandomNode : CompositeNode
    {
        private int index = -1;
        public RandomNode(BTNode[] childs) : base(childs)
        {
        }

        public override void OnUpdate()
        {
            if (index == -1)
                index = Random.Range(0, _childs.Length);

            BTNode child = _childs[index];
            child.OnUpdate();

            if (child.status == NodeStatus.Fail)
            {
                index = -1;
                return;
            }
                
            status = child.status;
        }
    }
}