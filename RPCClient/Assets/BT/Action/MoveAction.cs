using DG.Tweening;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class MoveAction : BTNode
    {
        private GameObject _go;
        private Vector3 _position;
        private float _speed;
        private Tween _tween;

        public MoveAction(GameObject go, Vector3 position, float speed)
        {
            this._go = go;
            this._position = position;
            this._speed = speed;
        }

        public override void OnUpdate()
        {
            if (_go == null)
            {
                status = NodeStatus.Fail;
                return;
            }
              

            if (_tween == null)
            {
                _tween = _go.transform.DOMove(this._position, _speed);
                _tween.onComplete = OnComplete;
                status = NodeStatus.Running;
                LogHelper.Log("ÒÆ¶¯");
            }
        }

        private void OnComplete()
        {
            status = NodeStatus.Success;
        }
    }
}