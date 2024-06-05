using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class ParallelNode : CompositeNode
    {
        public ParallelNode(BTNode[] childs) : base(childs)
        {
        }

        public override void OnUpdate()
        {
            int successCount = 0;
            foreach (BTNode child in _childs)
            {
                if (child.status == NodeStatus.Success)
                {
                    successCount++;
                    continue;
                }

                if(child.status == NodeStatus.Fail)
                {
                    status = NodeStatus.Fail;
                }

                child.OnUpdate();
            }

            if (successCount >= _childs.Length)
                status = NodeStatus.Success;
        }
    }
}