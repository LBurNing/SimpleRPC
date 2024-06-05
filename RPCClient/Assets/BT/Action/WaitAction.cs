using Cysharp.Threading.Tasks;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class WaitAction : BTNode
    {
        private float _seconds;
        private float _time;

        public WaitAction(int seconds)
        {
           this._seconds = seconds;
        }

        public override void OnUpdate()
        {
            if (_time == 0)
            {
                this._time = _seconds + Time.time;
            }

            if (Time.time - _time > 0)
            {
                LogHelper.Log($"—”≥Ÿ¡À{_seconds}√Î");
                status = NodeStatus.Success;
                return;
            }

            status = NodeStatus.Running;
        }
    }
}