using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class BTExample : MonoBehaviour
    {
        private BTNode _root;
        public GameObject _go;

        void Start()
        {
            BTNode[] childs = new BTNode[5] 
            { 
                new WaitAction(1),
                new MoveAction(_go, _go.transform.position + new Vector3(10,0,0), 1),
                new AttackAction(),
                new SelectorNode(new BTNode[2]
                {
                    new WaitAction(2),
                    new AttackAction(),
                }),
                new ParallelNode(new BTNode[3]
                {
                    new WaitAction(1),
                    new MoveAction(_go, _go.transform.position + new Vector3(0,0,0), 1),
                    new ConditionNode(new AttackAction(), new DistanceCondition(_go, Vector3.zero, 5)),
                })
            };

            _root = new SequenceNode(childs);
        }

        void Update()
        {
            _root.OnUpdate();
        }
    }
}