using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BT
{
    public class SequenceNode : CompositeNode
    {
        private int _currentIndex;

        public SequenceNode(BTNode[] childs) : base(childs)
        {
        }

        public override void OnUpdate()
        {
            if (_currentIndex < _childs.Length)
            {
                BTNode child = _childs[_currentIndex];
                child.OnUpdate();

                if (child.status == NodeStatus.Fail)
                {
                    return;
                }
                else if (child.status == NodeStatus.Success)
                {
                    _currentIndex++;
                    return;
                }
                else
                {
                    return;
                }
            }

            status = NodeStatus.Success;
        }
    }
}