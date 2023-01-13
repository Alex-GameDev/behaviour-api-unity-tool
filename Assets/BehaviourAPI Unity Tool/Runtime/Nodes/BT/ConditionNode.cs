using BehaviourAPI.BehaviourTrees.Decorators;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class ConditionNode : ConditionDecoratorNode
    {
        [SerializeReference] Perception _perception;

        public override void Start()
        {
            if(Perception == null) Perception = _perception;
            base.Start();
        }
    }
}
