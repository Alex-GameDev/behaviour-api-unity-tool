using BehaviourAPI.BehaviourTrees.Decorators;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class ConditionNode : ConditionDecoratorNode
    {
        [SerializeReference] new Perception Perception;
    }
}
