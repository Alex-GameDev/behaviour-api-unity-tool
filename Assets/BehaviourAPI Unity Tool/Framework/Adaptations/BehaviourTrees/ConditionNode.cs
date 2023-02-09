using BehaviourAPI.Core;
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ConditionNode : BehaviourTrees.ConditionNode
    {
        public PerceptionAsset perception;

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Perception = perception?.perception;
        }
    }
}
