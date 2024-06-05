using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class DistanceCondition : ICondition
    {
        private GameObject _go;
        private Vector3 _targetPos;
        private float _dis;

        public DistanceCondition(GameObject go, Vector3 targetPos, float dis) 
        {
            this._go = go;
            this._targetPos = targetPos;
            this._dis = dis;
        }

        public NodeStatus OnUpdate()
        {
            float dis = Vector3.Distance(_go.transform.position, _targetPos);
            if (dis <= this._dis)
                return NodeStatus.Success;

            return NodeStatus.Running;
        }
    }
}