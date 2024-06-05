using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BT
{
    public abstract class CompositeNode : BTNode
    {
        protected BTNode[] _childs;
        public CompositeNode(BTNode[] childs) 
        {
            this._childs = childs;
        }
    }
}