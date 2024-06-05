using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public interface ICondition
    {
        public NodeStatus OnUpdate();
    }

    public class ConditionNode : BTNode
    {
        private BTNode _child;
        private ICondition _condition;
        public ConditionNode(BTNode child, ICondition condition)
        {
            _child = child;
            _condition = condition;
        }

        public override void OnUpdate()
        {
            status = _condition.OnUpdate();
            if(status == NodeStatus.Success)
            {
                _child.OnUpdate();
                status = _child.status;
            }
        }
    }
}