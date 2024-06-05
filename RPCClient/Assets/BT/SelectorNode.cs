using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class SelectorNode : CompositeNode
    {
        public SelectorNode(BTNode[] childs) : base(childs)
        {
        }

        public override void OnUpdate()
        {
            foreach (var child in _childs)
            {
                child.OnUpdate();
                status = child.status;

                if (child.status != NodeStatus.Fail)
                    return;
            }

            status = NodeStatus.Fail;
        }
    }
}