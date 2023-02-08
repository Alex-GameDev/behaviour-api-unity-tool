using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ConditionNode : BehaviourTrees.ConditionNode
    {
        public PerceptionAsset perception;

        public override void Start()
        {
            Perception = perception?.perception;
            base.Start();
        }
    }
}
